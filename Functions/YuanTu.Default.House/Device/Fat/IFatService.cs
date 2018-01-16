using Microsoft.Practices.Unity;
using System;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Default.House.Device.Fat
{
    public interface IFatService : IDeviceService
    {
        Result StartMeasure(Input input);

        Result StartMeasure(int height, double weight, int age, int sex);

        bool MeasureFinished { get; set; }
        bool MeasureSuccess { get; set; }
    }

    public class FatService : IFatService
    {
        [Dependency]
        public IFatModel FatModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private int Port { get; set; }
        private int Baud { get; set; }

        private SerialPort _serialPort;

        public virtual  Result StartMeasure(Input input)
        {
            return StartMeasure(input.Height, input.Weight, input.Age, input.Sex);
        }

        public virtual  Result StartMeasure(int height, double weight, int age, int sex)
        {
            try
            {
                MeasureFinished = false;
                MeasureSuccess = false;
                DataReceiveCount = 0;
                var h = Convert.ToString(height, 16);
                var w = Convert.ToString((int)(weight * 10), 16).PadLeft(4, '0');
                var a = Convert.ToString(age, 16);
                var s = Convert.ToString(sex, 16).PadLeft(2, '0');

                var cmd = $"55 AA C1 00 {h} {w.Substring(2, 2)} {w.Substring(0, 2)} {a} {s} 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 4A";
                //var cmd = $"55 AA C1 00 b4 20 03 1e 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 4A";
                var bcmd = cmd.Split(' ').Select(p => byte.Parse(p, NumberStyles.HexNumber, null)).ToArray();
                bcmd[33] = CheckSum(bcmd, 9);
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Port = ConfigurationManager.GetValueInt("HeightWeight:Port");
                    Baud = ConfigurationManager.GetValueInt("HeightWeight:Baud");
                    _serialPort = new SerialPort($"COM{Port}", Baud)
                    {
                        ReceivedBytesThreshold = 1
                    };
                    _serialPort.Open();
                    // _serialPort.DataReceived += SerialPort_DataReceived;
                    DataReceive();
                }

                _serialPort.Write(bcmd, 0, bcmd.Length);
                Console.WriteLine(cmd);
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋体脂仪离线("连接设备失败", "检查设备连接");
                Logger.Device.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"测量失败，原因：{ex.Message}");
            }
        }

        private int DataReceiveCount { get; set; }
        private bool Running { get; set; }
        private void DataReceive()
        {
            Task.Run(() =>
            {
                Running = true;
                var dataLength = 10;
                while (Running)
                {
                    try
                    {
                        var data = new byte[dataLength];
                        var n = 0;
                        while (n < dataLength)
                            n += _serialPort.BaseStream.Read(data, n, dataLength - n);
                        Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(data)}");
                        if (data[2] == 0xC1)
                        {
                            DataReceiveCount++;
                            dataLength = 13;
                            //首次响应返回
                        }
                        if (data[2] == 0xC2)
                        {
                            //没有得到脂肪数据
                            Running = false;
                            _serialPort.Close();
                            MeasureFinished = true;
                            MeasureSuccess = false;
                        }
                        if (data[2] == 0x77)
                        {
                            //成功
                            Running = false;
                            _serialPort.Close();
                            Decode(data);
                            MeasureFinished = true;
                            MeasureSuccess = true;
                        }
                        if (data[2] == 0x00)
                        {
                            //返回错误脂肪数据
                            Running = false;
                            _serialPort.Close();
                            MeasureFinished = true;
                            MeasureSuccess = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        //返回错误脂肪数据
                        Running = false;
                        _serialPort.Close();
                        MeasureFinished = true;
                        MeasureSuccess = false;
                        Logger.Device.Error($"[{ServiceName}] {ex.Message} {ex.StackTrace}");
                    }
                }
            });
        }

        public bool MeasureFinished { get; set; }
        public bool MeasureSuccess { get; set; }

        private void Decode(byte[] recv)
        {
            var b1 = Convert.ToInt32(recv[3].ToString());
            var b2 = Convert.ToInt32(recv[4].ToString());
            FatModel.脂肪含量 = Convert.ToDecimal((b1 + b2 * 256) / 10.0);
            var b3 = Convert.ToInt32(recv[5].ToString());
            var b4 = Convert.ToInt32(recv[6].ToString());
            FatModel.体质指数 = Convert.ToDecimal((b3 + b4 * 256) / 10.0);
            var b5 = Convert.ToInt32(recv[7].ToString());
            var b6 = Convert.ToInt32(recv[8].ToString());
            FatModel.基础代谢值 = (b5 + b6 * 256);
            var b7 = Convert.ToInt32(recv[9].ToString());
            FatModel.体质参考结果 = 获取体质指数(b7);
            var b8 = Convert.ToInt32(recv[10].ToString());
            FatModel.体型参考结果 = 获取体型结果(b8);
        }

        private string 获取体质指数(int i)
        {
            switch (i)
            {
                case 1:
                    return "偏低";

                case 2:
                    return "标准";

                case 3:
                    return "偏高";

                case 4:
                    return "高";

                default:
                    return "未知";
            }
        }

        private string 获取体型结果(int i)
        {
            switch (i)
            {
                case 1:
                    return "消瘦";

                case 2:
                    return "标准";

                case 3:
                    return "隐藏性肥胖";
                //case 4:
                //    return "肌肉性肥胖/健壮";
                case 4:
                    return "健壮";

                case 5:
                    return "肥胖";

                default:
                    return "未知";
            }
        }

        public string ServiceName { get; } = "体脂";

        public Result StartMeasure()
        {
            throw new NotImplementedException();
        }

        public Result StopMeasure()
        {
            Running = false;
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
            return Result.Success();
        }

        private byte CheckSum(byte[] data, int len)
        {
            var n = 0;
            for (var i = 0; i < len; i++)
                n += data[i];
            return (byte)(256 - n);
        }
    }

    public class Input
    {
        public int Height { get; set; }
        public double Weight { get; set; }
        public int Age { get; set; }
        public int Sex { get; set; }
    }
}