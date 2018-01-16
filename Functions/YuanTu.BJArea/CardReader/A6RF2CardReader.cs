using System;
using System.Text;
using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.Consts.Services;
using YuanTu.Devices;

namespace YuanTu.BJArea.CardReader
{
    public class A6RF2CardReader : IRFCardReader
    {
        private readonly IConfigurationManager _configurationManager;
        private int _baud;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;

        private int _port;

        public A6RF2CardReader(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string DeviceName { get; } = "Act_A6";
        public string DeviceId => DeviceName + "_RF2";

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
            _port = _configurationManager.GetValueInt("Act_A6_2:Port");
            _baud = _configurationManager.GetValueInt("Act_A6_2:Baud");
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

    public class A6RFCpu2CardReader : IRFCpuCardReader
    {
        public virtual string DeviceName => "ACT_A6";
        public virtual string DeviceId => DeviceName + "_RFIC2";
        private int _baud;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;
        private readonly IConfigurationManager _configurationManager;

        public A6RFCpu2CardReader(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            return Result<DeviceStatus>.Success(_currentStatus);
        }

        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            _port = _configurationManager.GetValueInt("Act_A6_2:Port");
            _baud = _configurationManager.GetValueInt("Act_A6_2:Baud");
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

        public RFCpuType CpuType { get; set; }
        public Result<CardPos> GetCardPosition()
        {
            var status = new byte[3];
            var ret = UnSafeMethods.A6_GetCRCondition(_handle, status);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]卡位置检测异常，返回码:{ret}");
                return Result<CardPos>.Fail("类型检测失败，请插入正确的卡片");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]卡位置检测，返回值:{status[0]}");
            return Result<CardPos>.Success((CardPos)status[0]);
        }

        public Result MoveCard(CardPos pos)
        {
            CardMovePos p;
            switch (pos)
            {
                case CardPos.不持卡位:
                    p = CardMovePos.前端不持卡;
                    break;
                case CardPos.持卡位:
                    p = CardMovePos.前端持卡;
                    break;
                case CardPos.停卡位:
                    p = CardMovePos.非接;
                    break;
                case CardPos.IC位:
                    p = CardMovePos.IC;
                    break;
                case CardPos.后端持卡位:
                    p = CardMovePos.后端持卡;
                    break;
                case CardPos.后端不持卡位:
                    p = CardMovePos.后端不持卡;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pos), pos, null);
            }
            var ret = UnSafeMethods.A6_MoveCard(_handle, (int)p);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]移动卡位置异常，返回码:{ret}");
                return Result.Fail("移动卡失败！");
            }
            return Result.Success();
        }

        public Result<byte[]> GetCardId()
        {
            var len = 1024;
            var buff = new byte[len];
            switch (CpuType)
            {
                case RFCpuType.TypeA:
                    var ret = UnSafeMethods.A6_TypeACpuGetUID(_handle, buff, ref len);
                    if (ret != 0)
                    {
                        Logger.Device.Error($"[读卡器{DeviceName}]获取序列号失败，返回码:{ret}");
                        return Result<byte[]>.Fail("获取序列号失败，请确认卡介质是否正常");
                    }
                    return Result<byte[]>.Success(buff.Take(len).ToArray());
                default:
                    return Result<byte[]>.Fail($"您的卡类型为{CpuType} 该类型不允许获取卡序列号");
            }
        }

        public Result<byte[]> CpuTransmit(byte[] order)
        {
            var len = 1024;
            var buff = new byte[len];
            var ret = UnSafeMethods.A6_TypeABCpuTransmit(_handle, order, (ushort)order.Length, buff, ref len);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]命令操作失败，命令:{BitConverter.ToString(order)}，返回码:{ret}");
                return Result<byte[]>.Fail("指令发送失败，请确认卡介质是否正常");
            }
            return Result<byte[]>.Success(buff.Take(len).ToArray());
        }


    }
}