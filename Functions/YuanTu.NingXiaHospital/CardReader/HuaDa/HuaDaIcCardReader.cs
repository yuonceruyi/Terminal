using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingXiaHospital.CardReader.HuaDa
{
    public class HuaDaIcCardReader : IIcCardReader
    {
        private readonly string _deviceName = "USB1";

        private int _handle;
        private int _ret;
        public string DeviceName => "HuaDa";
        public string DeviceId => DeviceName + "_MyIC";

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
            _ret = UnSafeMethods.ICC_Reader_PowerOn(_handle, (byte)slotNo, res);
            if (_ret > 0)
                return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]上电异常，接口返回值:{_handle}");
            return Result.Fail("社保卡读卡器上电异常");
        }

        public Result<byte[]> CPUCodeReset()
        {
            return Result<byte[]>.Success(new byte[]{});
        }

        public Result<CardPos> GetCardPosition()
        {
            return Result<CardPos>.Success(CardPos.IC位);
        }

        public Result<byte[]> CPUChipIO(bool t0, byte[] input)
        {
            var dic=new Dictionary<string,byte[]>();
            var buffer = new byte[256];
            var ret=UnSafeMethods.ICC_Reader_Application(_handle, 1, input.Length, input, buffer);
            if (ret <= 0)
                return Result<byte[]>.Fail(HuaDaGetError(ret));
            return Result<byte[]>.Success(buffer);
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



        public Result MoveCard(CardPos pos)
        {
            return Result.Success();
        }
        #region ErrorCode

        private static readonly Dictionary<int, string> ErrorDic = new Dictionary<int, string>
        {
            [0] = "执行成功",
            [-1] = "卡片类型不对",
            [-2] = "无卡",
            [-3] = "有卡未上电",
            [-4] = "卡片无应答",
            [-11] = "读卡器连接错",
            [-12] = "未建立连接(没有执行打开设备函数)",
            [-13] = "(动态库)不支持该命令",
            [-14] = "(发给动态库的)命令参数错",
            [-15] = "信息校验和出错"
        };

        public static string HuaDaGetError(int code)
        {
            if (ErrorDic.TryGetValue(code, out var message))
                return $"[{code}]{message}";
            return $"[{code}]";
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
