using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Default.House.Device.BloodPressure;

namespace YuanTu.Default.House.Device.Temperature
{
    public interface ITemperatureService : IDeviceService
    {
        bool MeasureFinished { get; set; }
        bool MeasureSuccess { get; set; }
    }

    public class TempratureService : ITemperatureService
    {
        public bool MeasureFinished { get; set; }
        public bool MeasureSuccess { get; set; }


        [Dependency]
        public ITemperatureModel TemperatureModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public string ServiceName => "体温";
        private int Port { get; set; }
        private int Baud { get; set; }
        private SerialPort _serialPort;
        private bool Running { get; set; }
        public Result StartMeasure()
        {
            try
            {
                MeasureFinished = false;
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Port = ConfigurationManager.GetValueInt("Temperature:Port");
                    Baud = ConfigurationManager.GetValueInt("Temperature:Baud");
                    _serialPort = new SerialPort($"COM{Port}", Baud)
                    {
                        ReceivedBytesThreshold = 1
                    };
                    _serialPort.Open();
                    DataReceive();
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋体温仪离线("连接设备失败", "检查设备连接");
                Logger.Device.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"测量失败，原因：{ex.Message}");
            }
        }
        public virtual Result StopMeasure()
        {
            Running = false;
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            return Result.Success();
        }

        private void DataReceive()
        {
            Task.Run(() =>
            {
                Running = true;
                while (Running)
                {
                    try
                    {
                        var result = Receive(_serialPort);
                        if (!result)
                        {
                            _serialPort.DiscardInBuffer();
                        }
                        else
                        {
                            Decode(result.Value);
                            Running = false;
                            MeasureFinished = true;
                            MeasureSuccess = true;
                            _serialPort.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Running = false;
                        //测量失败
                        _serialPort.Close();
                        MeasureFinished = true;
                        MeasureSuccess = false;
                        Logger.Device.Debug($"[{ServiceName} {ex.Message} {ex.StackTrace}]");
                    }
                }
            });
        }

        private void Decode(ResPacket packet)
        {
            if (packet.Command == 0x10 && packet.Check && packet.Data.Length == 8)
            {
                var data = packet.Data;
                var suface = data[0] + data[1] * 0x100;
                var body = data[2] + data[3] * 0x100;
                var room = data[4] + data[5] * 0x100;
                var mode = data[6];
                var unit = data[7];
                if (unit == 0)
                {
                    TemperatureModel.表面温度 = suface / 10m;
                    TemperatureModel.人体温度 = body / 10m;
                    TemperatureModel.环境温度 = room / 10m;
                }
                else
                {
                    TemperatureModel.表面温度 = F2C(suface / 10m);
                    TemperatureModel.人体温度 = F2C(body / 10m);
                    TemperatureModel.环境温度 = F2C(room / 10m);
                }
                if (body > 365)
                    TemperatureModel.参考结果 = "偏高";
                else if (body < 355)
                    TemperatureModel.参考结果 = "偏低";
                else
                    TemperatureModel.参考结果 = "正常";
            }
        }

        decimal F2C(decimal f)
        {
            return (f - 32) / 1.8m;
        }

        Result<ResPacket> Receive(SerialPort p)
        {
            p.ReadTimeout = 10000;
            var bufferSize = 16;
            var buffer = new byte[bufferSize];
            int count = 0;
            byte dataLength = 0;
            int totalLength = 5;
            bool headDone = false;
            while (true)
            {
                var n = p.Read(buffer, count, bufferSize - count);
                count += n;
                if (count > 3)
                {
                    if (!headDone)
                    {
                        if (buffer[0] != 0xFA)
                        {
                            Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(buffer, count)}");
                            return Result<ResPacket>.Fail("Wrong Header");
                        }
                        dataLength = buffer[2];
                        totalLength = 5 + dataLength;
                        headDone = true;
                    }
                    if (count >= totalLength)
                    {
                        var data = new byte[dataLength];
                        var check = 0;
                        for (int i = 0; i < dataLength; i++)
                        {
                            data[i] = buffer[3 + i];
                            check += buffer[3 + i];
                        }
                        var bytes = new byte[count];
                        Array.Copy(buffer, bytes, count);
                        var packet = new ResPacket()
                        {
                            Command = buffer[1],
                            Data = data,
                            Check = check % 0x100 == buffer[3 + dataLength],
                            Bytes = bytes
                        };
                        Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(buffer, count)}");
                        return Result<ResPacket>.Success(packet);
                    }
                }
                if (count == bufferSize)
                {
                    break;
                }
            }
            Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(buffer, count)}");
            return Result<ResPacket>.Fail("Buffer Full");
        }

        class ReqPacket
        {
            public virtual byte Command { get; set; }
            public virtual byte[] Data { get; set; }

            public byte[] GetBytes()
            {
                int length = Data?.Length ?? 0;
                var bytes = new byte[5 + length];
                int check = 0;
                bytes[0] = 0xF5;
                bytes[1] = Command;
                bytes[2] = (byte)length;
                if (Data != null)
                    for (int i = 0; i < length; i++)
                    {
                        bytes[2 + i] = Data[i];
                        check += Data[i];
                    }
                bytes[3 + length] = (byte)(check % 0x100);
                bytes[4 + length] = 0xFF;
                return bytes;
            }
        }

        class ResPacket
        {
            public byte Command { get; set; }
            public byte[] Data { get; set; }
            public bool Check { get; set; }
            public byte[] Bytes { get; set; }
        }
    }
}
