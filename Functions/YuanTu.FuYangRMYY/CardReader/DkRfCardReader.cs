using System;
using System.Linq;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.FuYangRMYY.CardReader
{
    public class DkRfCardReader : IRFCardReader
    {
        private static int nPort = -1;
        public string DeviceName => "DK";
        public string DeviceId => DeviceName + "_dk";
        private DeviceStatus _deviceStatus;

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (_deviceStatus == DeviceStatus.Connected)
            {
                return Result.Success();
            }
            var errMsg = new StringBuilder(1024);
            nPort = UnSafeMethods.iOpenPort(errMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{nPort},错误内容:{errMsg}");
                return Result.Fail("射频卡读卡器连接异常");
            }
            _deviceStatus = DeviceStatus.Connected;
            return Result.Success();
        }

        public Result Initialize()
        {
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (_deviceStatus!=DeviceStatus.Connected)
            {
                return Result.Success();
            }
            var errMsg = new StringBuilder(1024);
            nPort = UnSafeMethods.iClosePort(errMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]关闭异常，接口返回值:{nPort},错误内容:{errMsg}");
                return Result.Fail("射频卡读卡器关闭异常");
            }
            _deviceStatus = DeviceStatus.Disconnect;
            return Result.Success();
        }

        public Result<CardPos> GetCardPosition()
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> GetCardId()
        {
            var errMsg = new StringBuilder(1024);
            var cardNo = new StringBuilder(1024);
            nPort = UnSafeMethods.iReadM1CardNum(cardNo, errMsg);
            if (nPort != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读卡异常，接口返回值:{nPort},错误内容:{errMsg}");
                return Result<byte[]>.Fail("射频卡读卡器读卡异常");
            }
            var bCardNo = Encoding.Default.GetBytes(cardNo.ToString().ToArray());
            return Result<byte[]>.Success(bCardNo);
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> SendCommand(byte[] orders)
        {
            throw new NotImplementedException();
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }
    }
}