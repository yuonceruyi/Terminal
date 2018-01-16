using HidLibrary;
using System;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.Default.House.Device.Gate
{
    public interface IGateService : IService
    {
        Result ConnectDevice();

        Result OpenGate();

        Result CloseGate();

        Result GetStatus();

        Status GateStatus { get; }
        ServiceStatus ServiceStatus { get; }

        Result CheckStatusAndCloseGate();

        Task<Result> OpenGateAsync();

        Task<Result> CloseGateAsync();
    }

    public class GateService : IGateService
    {
        public string ServiceName { get; } = "健康小屋闸门";

        private HidDevice HidDevice { get; set; }
        private const int VendorId = 0x4543;
        private const int ProductId = 0x4618;
        private const int TimeOut = 30;
        private readonly byte[] _openCommand = new byte[] { 0x00, 0x02, 0x01, 0x04, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private readonly byte[] _closeCommand = new byte[] { 0x00, 0x02, 0x01, 0x05, 0x03, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        private readonly byte[] _getStatusCommand = new byte[] { 0x00, 0x02, 0x01, 0x06, 0x03, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public Result ConnectDevice()
        {
            try
            {
                if (HidDevice != null && HidDevice.IsConnected && HidDevice.IsOpen)
                    return Result.Success();
                HidDevice = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();
                if (HidDevice == null)
                {
                    ReportService.健康小屋闸门离线("未找到对应的HID设备", "检查设备连接");
                    Logger.Device.Debug($"[{ServiceName}] 闸门异常，未找到对应的HID设备");
                    return Result.Fail($"闸门异常，未找到对应的HID设备");
                }
                HidDevice?.OpenDevice();
                return Result.Success();
            }
            catch (Exception ex)
            {
                ReportService.健康小屋闸门离线("未找到对应的HID设备", "检查设备连接");
                Logger.Device.Debug($"[{ServiceName}] {ex.Message} {ex.StackTrace}");
                return Result.Fail($"闸门异常 {ex.Message}");
            }
        }

        public Result OpenGate()
        {
            //return SendCommand(_openCommand);
            var result = ConnectDevice();
            if (!result.IsSuccess)
            {
                _serviceStatus = ServiceStatus.Disconnected;
                return Result.Fail(result.Message);
            }

            _serviceStatus = ServiceStatus.Openning;
            Logger.Device.Debug($"[{ServiceName}]发送:{BitConverter.ToString(_openCommand)}");
            if (!HidDevice.Write(_openCommand))
            {
                _serviceStatus = ServiceStatus.OpenFailed;
                Logger.Device.Debug($"[{ServiceName}] 打开闸门，命令发送失败");
                return Result.Fail($"闸门异常 打开闸门失败");
            }
            var hidDeviceData = HidDevice.Read(TimeOut);
            if (hidDeviceData.Status != HidDeviceData.ReadStatus.Success)
            {
                _serviceStatus = ServiceStatus.OpenFailed;
                Logger.Device.Debug($"[{ServiceName}] 打开闸门失败，返回值：{hidDeviceData.Status}");
                return Result.Fail($"闸门异常 打开闸门失败");
            }

            var data = hidDeviceData.Data;
            Logger.Device.Debug($"[{ServiceName}]接收:{BitConverter.ToString(data)}");

            if (data[1] != 0x02 || data[2] != 0x02 || data[3] != 0x04)
            {
                _serviceStatus = ServiceStatus.OpenFailed;
                Logger.Device.Debug($"[{ServiceName}] 打开闸门失败，返回数据不符");
                return Result.Fail($"闸门异常 打开闸门失败");
            }
            if (data[4] == 0xff)
            {
                _serviceStatus = ServiceStatus.OpenFailed;
                Logger.Device.Debug($"[{ServiceName}] 打开闸门失败，接收命令错误");
                return Result.Fail($"闸门异常 打开闸门失败");
            }
            if (data[4] == 0x45)
            {
                _serviceStatus = ServiceStatus.OpenFailed;
                //失败，可能处于半开状态，需读取闸门状态后在进行相关动作
                return Result.Success();
            }
            _serviceStatus = ServiceStatus.Opened;
            Logger.Device.Debug($"[{ServiceName}] 打开闸门成功");
            return Result.Success();
        }

        private ServiceStatus _serviceStatus;

        public ServiceStatus ServiceStatus => _serviceStatus;

        private Result SendCommand(byte[] cmd)
        {
            try
            {
                if (HidDevice == null || !HidDevice.IsConnected || !HidDevice.IsOpen)
                {
                    HidDevice = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();
                    if (HidDevice == null)
                    {
                        Logger.Device.Debug($"[{ServiceName}] 闸门异常，未找到对应的HID设备");
                        return Result.Fail($"闸门异常，未找到对应的HID设备");
                    }
                    HidDevice?.OpenDevice();
                }

                HidDevice?.Write(cmd, ckb =>
                {
                    Logger.Device.Debug($"[{ServiceName}]发送:{BitConverter.ToString(cmd)}");
                    HidDevice.Read(rbk =>
                    {
                        Logger.Device.Debug($"[{ServiceName}]接收:{BitConverter.ToString(rbk.Data)}");
                    });
                }, TimeOut);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Device.Debug($"[{ServiceName}] {ex.Message} {ex.StackTrace}");
                return Result.Fail($"闸门异常 {ex.Message}");
            }
        }

        public Result CloseGate()
        {
            //return SendCommand(_closeCommand);
            var result = ConnectDevice();
            if (!result.IsSuccess)
            {
                _serviceStatus = ServiceStatus.Disconnected;
                return Result.Fail(result.Message);
            }
            _serviceStatus = ServiceStatus.Closing;
            Logger.Device.Debug($"[{ServiceName}]发送:{BitConverter.ToString(_closeCommand)}");
            if (!HidDevice.Write(_closeCommand))
            {
                _serviceStatus = ServiceStatus.CloseFailed;
                Logger.Device.Debug($"[{ServiceName}] 关闭闸门，命令发送失败");
                return Result.Fail($"闸门异常 关闭闸门失败");
            }
            var hidDeviceData = HidDevice.Read(TimeOut);
            if (hidDeviceData.Status != HidDeviceData.ReadStatus.Success)
            {
                _serviceStatus = ServiceStatus.CloseFailed;
                Logger.Device.Debug($"[{ServiceName}] 关闭闸门失败，返回值：{hidDeviceData.Status}");
                return Result.Fail($"闸门异常 关闭闸门失败");
            }

            var data = hidDeviceData.Data;
            Logger.Device.Debug($"[{ServiceName}]接收:{BitConverter.ToString(data)}");

            if (data[1] != 0x02 || data[2] != 0x02 || data[3] != 0x05)
            {
                _serviceStatus = ServiceStatus.CloseFailed;
                Logger.Device.Debug($"[{ServiceName}] 关闭闸门失败，返回数据不符");
                return Result.Fail($"闸门异常 关闭闸门失败");
            }
            if (data[4] == 0xff)
            {
                _serviceStatus = ServiceStatus.CloseFailed;
                Logger.Device.Debug($"[{ServiceName}] 关闭闸门失败，接收命令错误");
                return Result.Fail($"闸门异常 关闭闸门失败");
            }
            if (data[4] == 0x45)
            {
                //规定时间内没有成功关闭，打开闸门，可能是传感器盲区夹到障碍物
                OpenGate();
                return Result.Success();
            }
            if (data[4] == 0x75)
            {
                _serviceStatus = ServiceStatus.Opened;
                //关闭时遇到障碍物后开启闸门成功
                return Result.Success();
            }
            if (data[4] == 0x65)
            {
                //关闭时遇到障碍物后开启闸门失败，再次进行开启闸门操作
                OpenGate();
                return Result.Success();
            }
            _serviceStatus = ServiceStatus.Closed;
            Logger.Device.Debug($"[{ServiceName}] 关闭闸门成功");
            return Result.Success();
        }

        public Result GetStatus()
        {
            var result = ConnectDevice();
            if (!result.IsSuccess)
            {
                _serviceStatus = ServiceStatus.Disconnected;
                return Result.Fail(result.Message);
            }

            Logger.Device.Debug($"[{ServiceName}]发送:{BitConverter.ToString(_getStatusCommand)}");
            if (!HidDevice.Write(_getStatusCommand))
            {
                Logger.Device.Debug($"[{ServiceName}] 获取闸门状态，命令发送失败");
                return Result.Fail($"闸门异常 获取闸门状态失败");
            }
            var hidDeviceData = HidDevice.Read(TimeOut);
            if (hidDeviceData.Status != HidDeviceData.ReadStatus.Success)
            {
                Logger.Device.Debug($"[{ServiceName}] 获取闸门状态失败，返回值：{hidDeviceData.Status}");
                return Result.Fail($"闸门异常 获取闸门状态失败");
            }

            var data = hidDeviceData.Data;
            Logger.Device.Debug($"[{ServiceName}]接收:{BitConverter.ToString(data)}");
            if (data[1] != 0x02 || data[2] != 0x02 || data[3] != 0x06)
            {
                Logger.Device.Debug($"[{ServiceName}] 获取闸门状态失败，返回数据不符");
                return Result.Fail($"闸门异常 获取闸门状态失败");
            }
            if (data[4] == 0xff)
            {
                Logger.Device.Debug($"[{ServiceName}] 获取闸门状态失败，接收命令错误");
                return Result.Fail($"闸门异常 获取闸门状态失败");
            }
            GateStatus = new Status
            {
                IsOpen = data[11] == 0x01,
                HasObstacles = data[10] == 0x01,
                Sensor1 = data[9],
                Sensor2 = data[8],
                Sensor3 = data[7],
                SensorError = data[6] == 0x01
            };
            Logger.Device.Debug($"[{ServiceName}] 获取闸门状态成功，{GateStatus.ToString()}");
            return Result.Success();
        }

        public Status GateStatus { get; set; }

        public Result CheckStatusAndCloseGate()
        {
            var result = GetStatus();
            if (!result.IsSuccess)
            {
                return Result.Fail(result.Message);
            }
            if (!GateStatus.IsOpen)
            {
                if (!GateStatus.HasObstacles)
                    return Result.Success();
                OpenGate();
                CloseGate();
                return Result.Success();
            }
            result = CloseGate();
            return !result.IsSuccess ? Result.Fail(result.Message) : Result.Success();
        }

        public Task<Result> OpenGateAsync()
        {
            return Task.Factory.StartNew(OpenGate);
        }

        public Task<Result> CloseGateAsync()
        {
            return Task.Factory.StartNew(CloseGate);
        }
    }
}