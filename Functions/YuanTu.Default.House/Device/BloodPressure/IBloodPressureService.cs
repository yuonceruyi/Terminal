using Microsoft.Practices.Unity;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Default.House.Device.BloodPressure
{
    public interface IBloodPressureService : IDeviceService
    {
        bool MeasureFinished { get; set; }
        bool MeasureSuccess { get; set; }
    }

    public class BloodPressureService : IBloodPressureService
    {
        [Dependency]
        public IBloodPressureModel BloodPressureModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        private readonly byte[] _startCommand = new byte[] { 0xAA, 0x55, 0xE1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20 };
        private int Port { get; set; }
        private int Baud { get; set; }
        private SerialPort _serialPort;
        private int _dataReceiveCount;
        private bool Running { get; set; }

        public virtual Result StartMeasure()
        {
            try
            {
                MeasureFinished = false;
                _dataReceiveCount = 0;
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Port = ConfigurationManager.GetValueInt("BloodPressure:Port");
                    Baud = ConfigurationManager.GetValueInt("BloodPressure:Baud");
                    _serialPort = new SerialPort($"COM{Port}", Baud)
                    {
                        ReceivedBytesThreshold = 1
                    };
                    _serialPort.Open();
                    DataReceive();
                }
                _serialPort.Write(_startCommand, 0, _startCommand.Length);
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋血压仪离线("连接设备失败", "检查设备连接");
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
                var dataLength = 34;
                while (Running)
                {
                    try
                    {
                        var data = new byte[dataLength];
                        var n = 0;
                        while (n < dataLength)
                            n += _serialPort.BaseStream.Read(data, n, dataLength - n);
                        Logger.Device.Debug($"[{ServiceName}]{BitConverter.ToString(data)}");
                        if (data[2] == 0xE0)
                        {
                            _dataReceiveCount++;
                            //首次响应返回
                        }
                        if (data[2] == 0xE1)
                        {
                            //测量成功
                            Running = false;
                            _serialPort.Close();
                            Decode(data);
                            MeasureFinished = true;
                            MeasureSuccess = true;
                        }
                        if (data[2] == 0xE2)
                        {
                            //测量失败
                            Running = false;
                            _serialPort.Close();
                            MeasureFinished = true;
                            MeasureSuccess = false;
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

        public bool MeasureFinished { get; set; }
        public bool MeasureSuccess { get; set; }

        private Result<string> GetReferenceResult()
        {
            if (BloodPressureModel.收缩压 > 180)
                return Result<string>.Success("三级高血压");
            if (BloodPressureModel.收缩压 > 160)
            {
                if (BloodPressureModel.舒张压 > 110)
                    return Result<string>.Success("三级高血压");
                return Result<string>.Success("二级高血压");
            }

            if (BloodPressureModel.收缩压 > 140)
            {
                if (BloodPressureModel.舒张压 > 110)
                    return Result<string>.Success("三级高血压");
                if (BloodPressureModel.舒张压 > 100)
                    return Result<string>.Success("二级高血压");
                return Result<string>.Success("一级高血压");
            }
            if (BloodPressureModel.收缩压 > 120)
            {
                if (BloodPressureModel.舒张压 > 110)
                    return Result<string>.Success("三级高血压");
                if (BloodPressureModel.舒张压 > 100)
                    return Result<string>.Success("二级高血压");
                if (BloodPressureModel.舒张压 > 90)
                    return Result<string>.Success("一级高血压");
                return Result<string>.Success("理想");
            }
            if (BloodPressureModel.收缩压 > 90)
            {
                if (BloodPressureModel.舒张压 > 110)
                    return Result<string>.Success("三级高血压");
                if (BloodPressureModel.舒张压 > 100)
                    return Result<string>.Success("二级高血压");
                if (BloodPressureModel.舒张压 > 90)
                    return Result<string>.Success("一级高血压");
                if (BloodPressureModel.舒张压 > 80)
                    return Result<string>.Success("理想");
                return Result<string>.Success("正常");
            }
            if (BloodPressureModel.收缩压 <= 90)
            {
                if (BloodPressureModel.舒张压 > 110)
                    return Result<string>.Success("三级高血压");
                if (BloodPressureModel.舒张压 > 100)
                    return Result<string>.Success("二级高血压");
                if (BloodPressureModel.舒张压 > 90)
                    return Result<string>.Success("一级高血压");
                if (BloodPressureModel.舒张压 > 80)
                    return Result<string>.Success("理想");
                if (BloodPressureModel.舒张压 > 70)
                    return Result<string>.Success("正常");
                return Result<string>.Success("低血压");
            }
            return Result<string>.Success("参数异常");
        }

        private void Decode(byte[] recv)
        {
            BloodPressureModel.舒张压 = Convert.ToInt32(recv[3].ToString());
            BloodPressureModel.收缩压 = Convert.ToInt32(recv[4].ToString()) + 25;//高压数据需要+25才是真正的测量数据
            BloodPressureModel.脉搏 = Convert.ToInt32(recv[6].ToString());
            BloodPressureModel.参考结果 = GetReferenceResult().Value;
        }

        public string ServiceName { get; } = "血压";
    }
}