using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;

namespace YuanTu.YiWuArea.Devices
{
    public class A630RFCpuCardReader: IRFCpuCardReader
    {
        public string DeviceName => "ACT_A630";
        public string DeviceId => DeviceName + "_RFIC";

        private int _baud;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private  int _handle;
        private bool _isConnectd;
        private int _port;
        private readonly IConfigurationManager _configurationManager;

        public A630RFCpuCardReader(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

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
            _port = _configurationManager.GetValueInt("Act_A630:Port");
            _baud = _configurationManager.GetValueInt("Act_A630:Baud");
            var ret = UnsafeMethods.A6_Connect(_port, _baud, ref _handle);
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
            var ret = UnsafeMethods.A6_Reset(_handle, UnsafeMethods.ResetAction.RESET_EJECT, buffer, ref bufferLen);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]初始化异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }
            //2.设置允许允许前门进卡，
            ret = UnsafeMethods.A6_PermitInsertion(_handle, 0);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]进卡设置失败，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                return Result.Fail("读卡器初始化失败");
            }

            UnsafeMethods.A6_LedControl(_handle,UnsafeMethods.LedAction.LED_LIGHTEN);
            return Result.Success();
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("读卡器未连接");
            }
            UnsafeMethods.A6_LedControl(_handle,UnsafeMethods.LedAction.LED_OFF);
            //强制退卡
            var ret = UnsafeMethods.A6_MoveCard(_handle,UnsafeMethods.MoveCardPos.MOVE_TOGATEPOS);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]退卡失败，返回码:{ret}");
                return Result.Fail("退卡失败！");
            }
            //设置不允许进卡
            ret = UnsafeMethods.A6_DenieInsertion(_handle);
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
            var ret = UnsafeMethods.A6_DenieInsertion(_handle);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]禁止入卡失败，返回码:{ret}");
                return Result.Fail("禁卡失败！");
            }
            UnsafeMethods.A6_LedControl(_handle, UnsafeMethods.LedAction.LED_OFF);
            ret = UnsafeMethods.A6_Disconnect(_handle);
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
            var status = new UnsafeMethods.CardStatus();
            var ret = UnsafeMethods.A6_GetStatus(_handle, ref status);
            if (ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceName}]卡位置检测异常，返回码:{ret}");
                return Result<CardPos>.Fail("类型检测失败，请插入正确的卡片");
            }
            Logger.Device.Info($"[读卡器{DeviceName}]卡位置检测，返回值:{status}");
            CardPos cps;
            switch (status)
            {
                case UnsafeMethods.CardStatus.CARD_ATGATEPOS:
                    cps=CardPos.不持卡位;
                    break;
                case UnsafeMethods.CardStatus.CARD_ATFRONTEND:
                    cps=CardPos.持卡位;
                    break;
                case UnsafeMethods.CardStatus.CARD_ATRFPOS:
                    cps=CardPos.停卡位;
                    break;
                case UnsafeMethods.CardStatus.CARD_ATICPOS:
                    cps=CardPos.IC位;
                    break;
                case UnsafeMethods.CardStatus.CARD_ATREAREND:
                    cps = CardPos.后端持卡位;
                    break;
                case UnsafeMethods.CardStatus.CARD_NOTPRESENT:
                    cps = CardPos.无卡;
                    break;
                case UnsafeMethods.CardStatus.CARD_NOTINSTDPOS:
                    cps = CardPos.未知;
                    break;
                default:
                    cps = CardPos.未知;
                    break;
            }
            return Result<CardPos>.Success(cps);
        }

        public Result MoveCard(CardPos pos)
        {
            UnsafeMethods.MoveCardPos p;
            switch (pos)
            {
                case CardPos.不持卡位:
                    p = UnsafeMethods.MoveCardPos.MOVE_TOGATEPOS;
                    break;
                case CardPos.持卡位:
                    p = UnsafeMethods.MoveCardPos.MOVE_TOFRONT;
                    break;
                case CardPos.停卡位:
                    p = UnsafeMethods.MoveCardPos.MOVE_TORFPOS;
                    break;
                case CardPos.IC位:
                    p = UnsafeMethods.MoveCardPos.MOVE_TOICPOS;
                    break;
                case CardPos.后端持卡位:
                    p = UnsafeMethods.MoveCardPos.MOVE_TOREAR;
                    break;
                case CardPos.后端不持卡位:
                    p = UnsafeMethods.MoveCardPos.MOVE_RETRACT;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pos), pos, null);
            }
            var ret = UnsafeMethods.A6_MoveCard(_handle, p);
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
                    var ret = UnsafeMethods.A6_TypeABCpuActivate(_handle, buff, ref len);
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
            try
            {
                Logger.Device.Error($"[读卡器{DeviceName}]发送命令:{BitConverter.ToString(order)} Handler:{_handle}");
                var len = 2048;
                var buff = new byte[len];
                bool outmbit=false;
                var ret = UnsafeMethods.A6_TypeABCpuTransmit(_handle, order, (ushort)order.Length, false, buff, ref len, out outmbit);
                if (ret != 0)
                {
                    Logger.Device.Error($"[读卡器{DeviceName}]命令操作失败，命令:{BitConverter.ToString(order)}，返回码:{ret} {outmbit}");
                    return Result<byte[]>.Fail("指令发送失败，请确认卡介质是否正常");

                }
                Logger.Device.Error($"[读卡器{DeviceName}]发送命令返回【{ret}】:{BitConverter.ToString(buff.Take(len).ToArray())}");
                return Result<byte[]>.Success(buff.Take(len).ToArray());
            }
            catch (Exception ex)
            {

                Logger.Device.Error($"[读卡器{DeviceName}]命令操作失败，命令:{BitConverter.ToString(order)}，异常:{ex}");
                return Result<byte[]>.Fail("指令发送失败，请确认卡介质是否正常");
            }
        }

    }
}
