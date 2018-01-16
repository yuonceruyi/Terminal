using System;
using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;

namespace YuanTu.Devices.CardReader
{
    /// <summary>
    ///     射频卡读卡器
    /// </summary>
    public interface IRFCardReader : IDevice
    {
        /// <summary>
        ///     获取卡在设备上的位置
        /// </summary>
        /// <returns></returns>
        Result<CardPos> GetCardPosition();

        /// <summary>
        ///     获取射频卡唯一序列号
        /// </summary>
        /// <returns></returns>
        Result<byte[]> GetCardId();

        /// <summary>
        ///     读M1数据
        /// </summary>
        /// <param name="sec">扇区</param>
        /// <param name="block">块</param>
        /// <param name="isKeyA">验证是否是keyA</param>
        /// <param name="key">key值</param>
        /// <returns></returns>
        Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key);

        /// <summary>
        ///     写M1数据
        /// </summary>
        /// <param name="sec">扇区</param>
        /// <param name="block">块</param>
        /// <param name="isKeyA">验证是否是keyA</param>
        /// <param name="key">key值</param>
        /// <param name="data">要写入的数据</param>
        /// <returns></returns>
        Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data);

        /// <summary>
        ///     向卡片发送指令并得到结果
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        Result<byte[]> SendCommand(byte[] orders);
        /// <summary>
        /// 手动设置handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result SetHandle(int handle);
        /// <summary>
        /// 手动获取handle 用作一个读卡器同时读两种卡时 共用一个handle
        /// </summary>
        /// <returns></returns>
        Result<int> GetHandle();
    }

    public class DefaultRfCardReader : IRFCardReader
    {
        private readonly IConfigurationManager _configurationManager;
        private int _baud;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;

        private int _port;

        public DefaultRfCardReader(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string DeviceName { get; } = "Act_A6";
        public string DeviceId => DeviceName + "_RF";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            _port = _configurationManager.GetValueInt("Act_A6:Port");
            _baud = _configurationManager.GetValueInt("Act_A6:Baud");
            var ret = UnSafeMethods.A6_Connect(_port, _baud, ref _handle);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]连接异常，返回码:{ret} 端口:{_port} 波特率:{_baud}");
                return Result.Fail("读卡器连接失败");
            }
            _isConnectd = true;
            _currentStatus = DeviceStatus.Connected;
            return Result.Success();
        }

        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("读卡器未连接");
            }
            //1.初始化读卡器
            var bufferLen = 1024;
            var buffer = new byte[bufferLen];
            var ret = UnSafeMethods.A6_Initialize(_handle, 0x30, buffer, ref bufferLen);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]初始化异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }
            //2.设置允许允许前门进卡，
            ret = UnSafeMethods.A6_SetCardIn(_handle, 0x33, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]进卡设置失败，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }
            UnSafeMethods.A6_LedOn(_handle);
            return Result.Success();
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("读卡器未连接");
            }
            UnSafeMethods.A6_LedOff(_handle);
            //强制退卡
            var ret = UnSafeMethods.A6_MoveCard(_handle, 0x30);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]退卡失败，返回码:{ret}");
                return Result.Fail("退卡失败！");
            }
            //设置不允许进卡
            ret = UnSafeMethods.A6_SetCardIn(_handle, 0x31, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]禁止入卡失败，返回码:{ret}");
                return Result.Fail("禁卡失败！");
            }
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd || _handle == 0)
            {
                _isConnectd = false;
                _currentStatus = DeviceStatus.Disconnect;
                return Result.Success();
            }
            //设置不允许进卡
            var ret = UnSafeMethods.A6_SetCardIn(_handle, 0x31, 0x31);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]禁止入卡失败，返回码:{ret}");
                return Result.Fail("禁卡失败！");
            }
            UnSafeMethods.A6_LedOff(_handle);
            ret = UnSafeMethods.A6_Disconnect(_handle);
            if (ret != 0)
            {
                _currentStatus = _currentStatus | DeviceStatus.Error;
                Logger.Device.Error($"[读卡器{DeviceName}]连接断开异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器断开失败");
            }
            _isConnectd = false;
            _currentStatus = DeviceStatus.Disconnect;
            return Result.Success();
        }

        public Result<CardPos> GetCardPosition()
        {
            var status = new byte[3];
            var ret = UnSafeMethods.A6_GetCRCondition(_handle, status);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]卡位置检测异常，返回码:{ret}");
                return Result<CardPos>.Fail("射频读卡失败，请确认您的卡片是否已经损坏！");
            }
            return Result<CardPos>.Success((CardPos)status[0]);
        }

        public Result<byte[]> GetCardId()
        {
            var num = new byte[16];
            var len = 16;
            var n = UnSafeMethods.A6_SxxSelect(_handle);
            if (n != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]寻卡异常，返回码:{n}");

                return Result<byte[]>.Fail("寻卡失败！");
            }
            UnSafeMethods.A6_SxxGetUID(_handle, num, ref len);
            if (num[0] != 0x59)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]获取卡序列号异常，首位:{num[0]}");
                return Result<byte[]>.Fail("寻卡失败！");
            }

            Logger.Device.Info($"[读卡器{DeviceId}]成功获取射频卡原始ID:{BitConverter.ToString(num)}");
            return Result<byte[]>.Success(num.Skip(1).Take(4).ToArray());
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            var ret = UnSafeMethods.A6_SxxVerifyPassword(_handle, sec, isKeyA, key);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]验证扇区失败，返回码：{ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }
            var data = new byte[16];
            ret = UnSafeMethods.A6_SxxReadBlock(_handle, sec, block, data);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读块数据失败，返回码：{ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }

            return Result<byte[]>.Success(data);
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            var ret = UnSafeMethods.A6_SxxVerifyPassword(_handle, sec, isKeyA, key);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]验证扇区失败，返回码：{ret}");
                return Result.Fail("写卡失败");
            }

            ret = UnSafeMethods.A6_SxxWriteBlock(_handle, sec, block, data);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]写块数据失败，返回码：{ret}");
                return Result.Fail("写卡失败");
            }

            return Result.Success();
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

    public class HuaDaRfCardReader : IRFCardReader
    {
        private readonly string _deviceName = "USB1";

        private int _handle;
        private int _ret;
        public string DeviceName => "HuaDa";
        public string DeviceId => DeviceName + "_RF";

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            _handle = UnSafeMethods.ICC_Reader_Open(_deviceName);
            if (_handle > 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]连接异常，接口返回值:{_handle}");
            return Result.Fail("读卡器连接异常");
        }

        public Result Initialize()
        {
            _ret = UnSafeMethods.ICC_PosBeep(_handle, 30);
            if (_ret >= 0) return Result.Success();
            Logger.Device.Error($"[读卡器{DeviceId}]初始化（蜂鸣）异常，接口返回值:{_ret}");
            return Result.Fail("读卡器初始化（蜂鸣）异常");
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
            return Result.Fail("读卡器断开连接异常");
        }

        public Result<CardPos> GetCardPosition()
        {
            _ret = UnSafeMethods.PICC_Reader_Request(_handle);
            return _ret == 0
                ? Result<CardPos>.Success(CardPos.停卡位)
                : Result<CardPos>.Fail("读卡器请求卡片失败");
        }

        public Result<byte[]> GetCardId()
        {
            _ret = UnSafeMethods.PICC_Reader_Request(_handle);
            //if (_ret != 0)
            //{
            //    Logger.Device.Error($"[读卡器{DeviceId}]读取序列号异常，接口返回值:{_ret}");
            //    return Result<byte[]>.Fail("读卡器读取序列号失败");
            //}
            var uid = new byte[256];
            _ret = UnSafeMethods.PICC_Reader_anticoll(_handle, uid);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读取序列号异常，接口返回值:{_ret}");
                return Result<byte[]>.Fail("读卡器读取序列号失败");
            }
            return Result<byte[]>.Success(uid);
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            _ret = UnSafeMethods.PICC_Reader_Authentication_Pass(_handle, isKeyA ? (byte)Mode.KeyA : (byte)Mode.KeyB,
                sec, key);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]验证扇区失败，返回码：{_ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }
            var data = new byte[16];
            /*  S50的M1卡和S70的M1卡兼容
             *  两种卡前32个扇区都是每个扇区4块
             *  S70的后8个扇区每个扇区16块
             */
            var realBlock = sec <= 32 ? (byte)(sec * 4 + block) : (byte)((sec - 33) * 16 + 33 * 4 + block);

            _ret = UnSafeMethods.PICC_Reader_Read(_handle, realBlock, data);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]读块数据失败，返回码：{_ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }

            return Result<byte[]>.Success(data);
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            _ret = UnSafeMethods.PICC_Reader_Authentication_Pass(_handle, isKeyA ? (byte)Mode.KeyA : (byte)Mode.KeyB,
                sec, key);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]验证扇区失败，返回码：{_ret} ");
                return Result.Fail("写卡失败");
            }
            /*  S50的M1卡和S70的M1卡兼容
             *  两种卡前32个扇区都是每个扇区4块
             *  S70的后8个扇区每个扇区16块
             */
            var realBlock = sec <= 32 ? (byte)(sec * 4 + block) : (byte)((sec - 33) * 16 + 33 * 4 + block);

            _ret = UnSafeMethods.PICC_Reader_Write(_handle, realBlock, data);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]写块数据失败，返回码：{_ret} ");
                return Result.Fail("写卡失败");
            }

            return Result.Success();
        }

        public Result<byte[]> SendCommand(byte[] orders)
        {
            throw new NotImplementedException();
        }

        private enum Mode : byte
        {
            KeyA = 0x60,
            KeyB = 0x61
        }

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            return Result<int>.Success(_handle);
        }
    }
}