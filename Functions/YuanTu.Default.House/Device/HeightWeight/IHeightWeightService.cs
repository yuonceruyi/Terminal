using Microsoft.Practices.Unity;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Default.House.Device.HeightWeight
{
    public interface IHeightWeightService : IDeviceService
    {
        bool MeasureSuccess { get; set; }
        bool MeasureFinished { get; set; }
    }

    public class HeightWeightService : IHeightWeightService
    {
        private readonly byte[] _startCommand = new byte[] { 0x55, 0xAA, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x61 };

        public bool MeasureSuccess { get; set; }
        public bool MeasureFinished { get; set; }

        [Dependency]
        public IHeightWeightModel HeightWeightModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private int Port { get; set; }
        private int Baud { get; set; }
        private SerialPort _serialPort;
        private int DataReceiveCount { get; set; }
        private bool Running { get; set; }

        public virtual Result StartMeasure()
        {
            try
            {
                MeasureFinished = false;
                MeasureSuccess = false;
                DataReceiveCount = 0;
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Port = ConfigurationManager.GetValueInt("HeightWeight:Port");
                    Baud = ConfigurationManager.GetValueInt("HeightWeight:Baud");
                    _serialPort = new SerialPort($"COM{Port}", Baud)
                    {
                        ReceivedBytesThreshold = 1
                    };
                    _serialPort.Open();
                    //_serialPort.DataReceived += SerialPort_DataReceived;
                    DataReceive();
                }
                _serialPort.Write(_startCommand, 0, _startCommand.Length);
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋身高体重仪离线("连接设备失败", "检查设备连接");
                Logger.Device.Error($"{ex.Message} {ex.StackTrace}");
                return Result.Fail($"测量失败，原因：{ex.Message}");
            }
        }

        public virtual Result StopMeasure()
        {
            Running = false;
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
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
                        var data = new byte[10];
                        var n = 0;
                        while (n < 10)
                            n += _serialPort.BaseStream.Read(data, n, 10 - n);
                        Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(data)}");
                        if (data[2] == 0xA0 && data[3] == 0x00)
                        {
                            DataReceiveCount++;
                            if (DataReceiveCount == 2)
                            {
                                //收到测量失败结果
                                Running = false;
                                MeasureFinished = true;
                                MeasureSuccess = false;
                                _serialPort.Close();
                            }
                        }
                        if (data[2] == 0xA0 && data[3] != 0x00)
                        {
                            //收到测量结果
                            Running = false;
                            var height = new byte[2];
                            var weight = new byte[2];
                            Array.Copy(data, 3, height, 0, 2);
                            Array.Copy(data, 5, weight, 0, 2);

                            HeightWeightModel.身高 = 获取身高(height);
                            HeightWeightModel.体重 = 获取体重(weight);
                            HeightWeightModel.体质指数 = 获取体质指数(HeightWeightModel.身高, HeightWeightModel.体重);
                            HeightWeightModel.参考结果 = GetReferenceResult(HeightWeightModel.体质指数).Value;
                            Logger.Device.Debug($"{HeightWeightModel.身高} {HeightWeightModel.体重} {HeightWeightModel.体质指数} {HeightWeightModel.参考结果}");
                            MeasureFinished = true;
                            MeasureSuccess = true;
                            _serialPort.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        //收到测量失败结果
                        Running = false;
                        MeasureFinished = true;
                        MeasureSuccess = false;
                        _serialPort.Close();
                    }
                }
            });
        }

        private Result<string> GetReferenceResult(decimal 体质指数)
        {
            var _体质指数 = Convert.ToDouble(体质指数);
            if (_体质指数 <= 18.4)
                return Result<string>.Success("偏瘦");
            if (_体质指数 >= 18.5 && _体质指数 <= 23.9)
                return Result<string>.Success("正常");
            if (_体质指数 >= 24.0 && _体质指数 <= 27.9)
                return Result<string>.Success("偏重");
            if (_体质指数 >= 28.0)
                return Result<string>.Success("肥胖");
            return Result<string>.Success("未知");
        }

        private decimal 获取身高(byte[] b)
        {
            return Convert.ToDecimal(Convert.ToDouble($"{b[0]}{b[1].ToString("D2")}") / 10);
        }

        private decimal 获取体重(byte[] b)
        {
            return Convert.ToDecimal(Convert.ToDouble($"{b[0]}{b[1].ToString("D2")}") / 10);
        }

        private decimal 获取体质指数(decimal h, decimal w)
        {
            return decimal.Parse((w / ((h / 100) * (h / 100))).ToString("0.0"));
        }

        public string ServiceName { get; } = "身高体重";
    }
}