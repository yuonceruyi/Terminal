using System;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.Devices.CardReader
{
    public interface IIcCardReader : IDevice
    {
        /// <summary>
        ///     接触 CPU 卡上电复位
        /// </summary>
        /// <param name="SlotNo">卡座选择一般为大卡座</param>
        /// <returns></returns>
        Result PowerOn(SlotNo slotNo);

        Result<byte[]> CPUCodeReset();

        /// <summary>
        ///     获取卡在设备上的位置
        /// </summary>
        /// <returns></returns>
        Result<CardPos> GetCardPosition();

        /// <summary>
        /// 移动卡位置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        Result MoveCard(CardPos pos);

        Result<byte[]> CPUChipIO(bool t0, byte[] input);

        /// <summary>
        ///     手动设置handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result SetHandle(int handle);

        /// <summary>
        ///     手动获取handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result<int> GetHandle();
    }

    public class HuaDaICCardReader : IIcCardReader
    {
        private readonly string _deviceName = "USB1";

        private int _handle;
        private int _ret;
        public string DeviceName => "HuaDa";
        public string DeviceId => DeviceName + "_IC";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            _handle = UnSafeMethods.ICC_Reader_Open(_deviceName);
            if (_handle > 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{_handle}");
            return Result.Fail("社保卡读卡器连接异常");
        }

        public Result PowerOn(SlotNo slotNo)
        {
            var res = new byte[256];
            _ret = UnSafeMethods.ICC_Reader_PowerOn(_handle, (byte) slotNo, res);
            if (_ret > 0)
                return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]上电异常，接口返回值:{_handle}");
            return Result.Fail("社保卡读卡器上电异常");
        }

        public Result<byte[]> CPUCodeReset()
        {
            throw new NotImplementedException();
        }

        public Result<CardPos> GetCardPosition()
        {
            throw new NotImplementedException();
        }

        public Result MoveCard(CardPos pos)
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> CPUChipIO(bool t0, byte[] input)
        {
            throw new NotImplementedException();
        }

        public Result Initialize()
        {
            _ret = UnSafeMethods.ICC_PosBeep(_handle, 30);
            if (_ret >= 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]初始化（蜂鸣）异常，接口返回值:{_ret}");
            return Result.Fail("社保卡读卡器初始化（蜂鸣）异常");
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        public Result DisConnect()
        {
            _ret = UnSafeMethods.ICC_Reader_Close(_handle);
            if (_ret == 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]断开连接异常，接口返回值:{_ret}");
            return Result.Fail("社保卡读卡器断开连接异常");
        }

        public Result SetHandle(int handle)
        {
            _handle = handle;
            return Result.Success();
        }

        public Result<int> GetHandle()
        {
            return Result<int>.Success(_handle);
        }
    }
}