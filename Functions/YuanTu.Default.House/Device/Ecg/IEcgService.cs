using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using LiveCharts.Geared;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.House.Device.SpO2;

namespace YuanTu.Default.House.Device.Ecg
{
    public interface IEcgService : IDeviceService
    {
        bool MeasureFinished { get; set; }
        bool MeasureSuccess { get; set; }
    }

    public class EcgService : IEcgService
    {
        private readonly Dictionary<byte, string> _diags = new Dictionary<byte, string>
        {
            [0X00] = "波形未见异常",
            [0X01] = "波形疑似心跳稍快请注意休息",
            [0X02] = "波形疑似心跳过快请注意休息",
            [0X03] = "波形疑似阵发性心跳过快请咨询医生",
            [0X04] = "波形疑似心跳稍缓请注意休息",
            [0X05] = "波形疑似心跳过缓请注意休息",
            [0X06] = "波形疑似偶发心跳间期缩短请咨询医生",
            [0X07] = "波形疑似心跳间期不规则请咨询医生",
            [0X08] = "波形疑似心跳稍快伴有偶发心跳间期缩短请咨询医生",
            [0X09] = "波形疑似心跳稍缓伴有偶发心跳间期缩短请咨询医生",
            [0X0A] = "波形疑似心跳稍缓伴有心跳间期不规则请咨询医生",
            [0X0B] = "波形有漂移请重新测量",
            [0X0C] = "波形疑似心跳过快伴有波形漂移请咨询医生",
            [0X0D] = "波形疑似心跳过缓伴有波形漂移请咨询医生",
            [0X0E] = "波形疑似偶发心跳间期缩短伴有波形漂移请咨询医生",
            [0X0F] = "波形疑似心跳间期不规则伴有波形漂移请咨询医生",
            [0XFF] = "信号较差请重新测量"
        };

        private readonly List<double> _valuesBuffer = new List<double>();

        private SerialPort _serialPort;

        private readonly int _size = 5;

        private DateTime _startTime;

        [Dependency]
        public IEcgModel EcgModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private int Port { get; set; }
        private int Baud { get; set; }

        private bool Running { get; set; }
        public string ServiceName { get; } = "心电";

        public bool MeasureFinished { get; set; }
        public bool MeasureSuccess { get; set; }

        public Result StartMeasure()
        {
            try
            {
                MeasureFinished = false;
                MeasureSuccess = false;
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Port = ConfigurationManager.GetValueInt("Ecg:Port");
                    Baud = ConfigurationManager.GetValueInt("Ecg:Baud");
                    _serialPort = new SerialPort($"COM{Port}", Baud)
                    {
                        ReceivedBytesThreshold = 1
                    };
                    _serialPort.Open();
                    EcgModel.Values = new GearedValues<double>();
                    Running = true;
                    Task.Run(() =>
                    {
                        while (Running)
                            DataReceive();
                    });
                    Task.Run(async () =>
                    {
                        while (Running)
                        {
                            await Task.Delay(150);
                            lock (_valuesBuffer)
                            {
                                EcgModel.Values.AddRange(_valuesBuffer);
                                _valuesBuffer.Clear();
                            }
                            SetAxisLimits();
                        }
                    });
                }
                _startTime = DateTime.Now;
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋心电仪离线("连接设备失败", "检查设备连接");
                Logger.Device.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"测量失败，原因：{ex.Message}");
            }
        }

        public Result StopMeasure()
        {
            Running = false;
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            return Result.Success();
        }

        private void Decode(byte[] recv)
        {
            EcgModel.Diag = recv[0];
            EcgModel.PR = recv[2];
            string diag;
            if (!_diags.TryGetValue(recv[0], out diag))
                diag = "未知结果";
            EcgModel.参考结果 = diag;
        }

        private void SetAxisLimits()
        {
            var c = EcgModel.Values.Count;
            var n = c / 300 + (c % 300 > 0 ? 1 : 0) - 1;
            EcgModel.XMax = n * 300 + 300;
            EcgModel.XMin = n * 300;
        }

        private void DataReceive()
        {
            try
            {
                var result = Receive(_serialPort);
                if (!result)
                {
                    if (DateTime.Now - _startTime > TimeSpan.FromSeconds(60))
                    {
                        Running = false;
                        _serialPort.Close();
                        MeasureFinished = true;
                        MeasureSuccess = false;
                    }
                    return;
                }
                var packet = result.Value;
                if (packet.Token == 0x33 && packet.Type == 0x01)
                {
                    Running = false;
                    _serialPort.Close();
                    Decode(packet.Data);
                    MeasureFinished = true;
                    MeasureSuccess = true;
                }
                if (packet.Token == 0x30 && packet.Type == 0x02)
                {
                    Running = false;
                    _serialPort.Close();
                    MeasureFinished = true;
                    MeasureSuccess = false;
                }
                if (packet.Token == 0x32 && packet.Type == 0x01)
                {
                    var data = packet.Data;
                    var buffer = new int[25];
                    for (var i = 0; i < 25; i++)
                    {
                        var b1 = data[1 + i * 2];
                        var b2 = data[1 + i * 2 + 1];
                        var beat = (b1 & 0x40) > 0;
                        int value;
                        var expand = (b1 & 0x20) > 0;
                        if (expand)
                            value = (b1 & 0x0F) * 0x100 + b2;
                        else
                            value = b2;
                        buffer[i] = value;
                    }

                    lock (_valuesBuffer)
                    {
                        for (var i = 0; i < 25 / _size; i++)
                        {
                            var value = buffer.Skip(i * _size).Take(_size).Average();
                            _valuesBuffer.Add(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //返回错误数据
                Running = false;
                _serialPort.Close();
                MeasureFinished = true;
                MeasureSuccess = false;
                Logger.Device.Error($"[{ServiceName}] {ex.Message} {ex.StackTrace}");
            }
        }

        private Result<ResPacket> Receive(SerialPort p)
        {
            p.ReadTimeout = 10000;
            var bufferSize = 16;
            var buffer = new byte[bufferSize];
            var count = 0;
            byte dataLength = 0;
            var totalLength = 5;
            var headDone = false;
            while (true)
            {
                if (totalLength > bufferSize)
                {
                    var newBuffer = new byte[totalLength];
                    Array.Copy(buffer, newBuffer, count);
                    buffer = newBuffer;
                    bufferSize = totalLength;
                }
                var n = p.Read(buffer, count, totalLength - count);
                count += n;
                if (count > 4)
                {
                    if (!headDone)
                    {
                        if (buffer[0] != 0xAA || buffer[1] != 0x55)
                            return Result<ResPacket>.Fail("Wrong Header");
                        dataLength = (byte) (buffer[3] - 2);
                        totalLength = 2 + 1 + 1 + 1 + dataLength + 1;
                        headDone = true;
                    }
                    if (count >= totalLength)
                    {
                        var data = new byte[dataLength];
                        for (var i = 0; i < dataLength; i++)
                            data[i] = buffer[5 + i];
                        var bytes = new byte[count];
                        Array.Copy(buffer, bytes, count);

                        var crc = buffer[totalLength - 1];
                        var crcCheck = CRC8.CRC(buffer, 0, totalLength - 1);
                        var packet = new ResPacket
                        {
                            Token = buffer[2],
                            Type = buffer[4],
                            Data = data,
                            Check = crc == crcCheck,
                            Bytes = bytes
                        };
                        Console.WriteLine(BitConverter.ToString(bytes, 0, totalLength));
                        return Result<ResPacket>.Success(packet);
                    }
                }
                if (count == bufferSize)
                    break;
            }
            return Result<ResPacket>.Fail("Buffer Full");
        }

        private class ReqPacket
        {
            public virtual byte Token { get; set; }
            public virtual byte Type { get; set; }
            public virtual byte[] Data { get; set; }

            public byte[] GetBytes()
            {
                var length = Data?.Length ?? 0;
                var bytes = new byte[6 + length];
                bytes[0] = 0xAA;
                bytes[1] = 0x55;
                bytes[2] = Token;
                bytes[3] = (byte) length;
                bytes[4] = Type;
                ;
                if (Data != null)
                    for (var i = 0; i < length; i++)
                        bytes[4 + i] = Data[i];
                bytes[5 + length] = CRC8.CRC(bytes, 0, 4 + length);
                return bytes;
            }
        }

        private class ResPacket
        {
            public byte Token { get; set; }
            public byte Type { get; set; }
            public byte[] Data { get; set; }

            public bool Check { get; set; }
            public byte[] Bytes { get; set; }
        }
    }
}