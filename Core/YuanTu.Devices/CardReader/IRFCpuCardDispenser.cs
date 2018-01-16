using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;

namespace YuanTu.Devices.CardReader
{
    public interface IRFCpuCardDispenser:IDevice
    {
        /// <summary>
        ///     从卡槽中将空白卡传入
        /// </summary>
        /// <returns></returns>
        Result<byte[]> EnterCard();

        /// <summary>
        ///     获取卡片唯一ID
        /// </summary>
        /// <returns></returns>
        Result<byte[]> GetCardId();
        /// <summary>
        /// 执行与CPU的交互指令
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Result<byte[]> CpuTransmit(byte[] order);

        Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null);

        /// <summary>
        ///     从前端弹出卡片
        /// </summary>
        /// <returns></returns>
        Result MoveCardOut();
    }

    public class F6RfCpuCardDispenser : IRFCpuCardDispenser
    {
        public string DeviceName => "Act_F6";
        public string DeviceId => DeviceName + "_RFIC";
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;

        public F6RfCpuCardDispenser(IConfigurationManager configurationManager)
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
            _port = _configurationManager.GetValueInt("Act_F6:Port");
            var ret = UnSafeMethods.CommOpen("COM" + _port);
            if (ret == 0)
            {
                Logger.Device.Error($"[发卡器{DeviceId}]连接异常，端口:{_port}");
                return Result.Fail("发卡器连接失败");
            }
            _isConnectd = true;
            _handle = ret;
            _currentStatus = DeviceStatus.Connected;
            return Result.Success();
        }

        public Result Initialize()
        {
            if (_isConnectd)
            {
                var bts = new byte[256];
                var ret = UnSafeMethods.Reset(_handle, 0x30, bts);
                if (ret == 0)
                {
                    //点亮指示灯
                    UnSafeMethods.Led1Control(_handle, 0x31);
                    return Result.Success();
                }
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]初始化失败，返回码:{ret} 错误原因：{msg}");
                return Result.Fail("发卡器初始化失败");
            }
            return Result.Fail("发卡器未连接");
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //熄灭指示灯
            UnSafeMethods.Led1Control(_handle, 0x30);
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //熄灭指示灯
            _isConnectd = false;
            UnSafeMethods.Led1Control(_handle, 0x30);
            var ret = UnSafeMethods.CommClose(_handle);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "存在未知异常";
                Logger.Device.Error($"[发卡器{DeviceId}]断开连接失败， Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            return Result.Success();
        }

        public Result<byte[]> EnterCard()
        {
            var pos = new byte();
            var ret = UnSafeMethods.CheckCardPosition(_handle, ref pos);
            var buildErrorAction =
                (Func<int, string>)
                    (pt => ErrorDictionary.ContainsKey(pt) ? ErrorDictionary[pt] : "存在未知异常");
            if (ret != 0)
            {
                var msg = buildErrorAction(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]卡位置检测，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result<byte[]>.Fail(msg);
            }
            switch (pos)
            {
                case 读卡机内无卡:
                    {
                        ret = UnSafeMethods.EnterCard(_handle, 发卡, 20 * 1000);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 进卡异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移动到重入卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 移动到重入卡位置异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移到读卡器内部);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 移动到读卡器内部异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        break;
                    }
                case 卡片在读卡机射频卡位置:
                    {
                        ret = UnSafeMethods.MoveCard(_handle, 移动到重入卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到重入卡位置异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移到读卡器内部);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到读卡器内部异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        break;
                    }
                default:
                    {
                        Logger.Device.Error($"[发卡器{DeviceId}]卡片在读卡机射频卡位置 卡所在的位置异常,卡位置代码:0X{pos.ToString("X")}");
                        return Result<byte[]>.Fail($"发卡器内存在未被取走的卡！");
                    }
            }
            return GetCardId();
        }

        public Result<byte[]> GetCardId()
        {
            var len = 1024;
            var data = new byte[len];
            var ret = UnSafeMethods.A6_unmeet_activation(_handle, data/*, ref len*/);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]获取卡片ID失败，返回码：{ret} 错误原因:{msg}");
                return Result<byte[]>.Fail("未能正常读出卡片信息");
            }
            //len=data.ToList().IndexOf(0);
            var realdata = data.Take(len).ToArray();
            Logger.Device.Info($"[发卡器{DeviceId}]成功获取射频CPU卡原始ID:{BitConverter.ToString(realdata)}");
            return Result<byte[]>.Success(realdata);
        }

        public Result<byte[]> CpuTransmit(byte[] order)
        {
            var len = 1024;
            var buff = new byte[len];
            var ret = UnSafeMethods.A6_unmeet_APDU(_handle, (byte)order.Length, order,  buff/*, ref len*/);
            if (ret != 0)
            {
                Logger.Device.Error($"[发卡器{DeviceName}]命令操作失败，命令:{BitConverter.ToString(order)}，返回码:{ret}");
                return Result<byte[]>.Fail($"指令发送失败:{GetErrorMsg(ret)}，请确认卡介质是否正常");
            }
            //len = buff.ToList().IndexOf(0);
            return Result<byte[]>.Success(buff.Take(len).ToArray());
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            throw new NotImplementedException();
        }

        public Result MoveCardOut()
        {
            var buildErrorAction =
               (Func<int, string>)
                   (pt => ErrorDictionary.ContainsKey(pt) ? ErrorDictionary[pt] : "存在未知异常");
            var ret = UnSafeMethods.MoveCard(_handle, 从前端弹出);
            if (ret != 0)
            {
                var msg = buildErrorAction(ret);
                Logger.Device.Error(
                    $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到从前端弹出，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            return Result.Success();
        }

        #region[常量值]

        private const byte 允许卡片进入 = 0x30;
        private const byte 发卡 = 0x32;
        private const byte 禁卡进卡 = 0x33;
        private const byte 卡片在前端不夹卡位置 = 0x30;
        private const byte 卡片在前端夹卡位置 = 0x31;
        private const byte 卡片在读卡机射频卡位置 = 0x32;
        private const byte 卡片在IC卡位置 = 0x33;
        private const byte 卡片在后端夹卡位置 = 0x34;
        private const byte 读卡机内无卡 = 0x35;
        private const byte 卡不在标准位置 = 0x36;
        private const byte 移到读卡器内部 = 0x30;
        private const byte 移到IC卡位置 = 0x31;
        private const byte 移到前端夹卡位置 = 0x32;
        private const byte 移到后端夹卡位置 = 0x33;
        private const byte 从前端弹出 = 0x34;
        private const byte 吞入 = 0x35;
        private const byte 移动到重入卡位置 = 0x36;

        #endregion

        #region ErrorDictionary

        private static string GetErrorMsg(int retCode)
        {
            return ErrorDictionary.ContainsKey(retCode) ? ErrorDictionary[retCode] : "存在未知异常";
        }

        private static readonly Dictionary<int, string> ErrorDictionary = new Dictionary<int, string>
        {
            //命令错误代码
            {0x00, "未定义的命令"},
            {0x01, "命令参数错误"},
            {0x02, "命令数据错误"},
            {0x03, "命令不能执行"},
            {0x04, "命令执行失败"},
            //电源错误代码
            {0x05, "电源错误，过高>13v"},
            {0x06, "电源错误，过低<10v"},
            {0x07, "主电源过低或不存在。"},
            {0x08, "传感器异常"},
            //卡片动作错误代码
            {0x0a, "卡片堵塞"},
            {0x0b, "打开闸门失败"},
            {0x0c, "有长卡"},
            {0x0d, "有短卡"},
            {0x0e, "后进卡（系统默认的是30s）命令超时"},
            //IC卡操作错误代码----CPU卡
            {0x21, "CPU卡复位失败"},
            {0x22, "T=0的CPU卡片命令执行失败"},
            {0x23, "T=1的CPU卡片容量请求失败"},
            {0x24, "T=1的CPU卡片命令执行失败"},
            //IC卡操作错误代码----SAM卡
            {0x30, "SAM卡片复位失败"},
            {0x31, "T=0的SAM卡命令执行失败"},
            {0x32, "T=1的SAM卡容量请求失败"},
            {0x33, "T=1的SAM卡命令执行失败"},
            //IC卡操作错误代码----RF卡
            {0X40, "射频卡位置无卡,射频卡的操作命令不能执行"},
            {0x41, "寻卡失败"},
            {0x42, "读序列号失败"},
            {0x43, "验证密码错误"},
            {0x44, "选择卡片错误"},
            {0x45, "读数据失败"},
            {0x46, "写数据失败 "},
            {0x49, "增值失败"},
            {0x4a, "减值失败"},
            //IC卡操作错误代码----存储卡，逻辑加密卡
            {0x50, "IC卡位置无卡片，IC卡命令不能执行"},
            {0x51, "AT24系列卡读数据失败"},
            {0x52, "AT24系列卡写数据失败"},
            {0x53, "AT45DB041卡片复位失败"},
            {0x56, "AT88S1608卡片复位失败"},
            {0x57, "AT88S1608卡片密码校验错误"},
            {0x58, "AT88S1608卡片读数据错误"},
            {0x59, "AT88S1608卡片写数据错误"},
            {0x5a, "AT88S1608卡片写熔丝错误"},
            {0x5b, "AT88S1608卡片初始化认证错误"},
            {0x5c, "AT88S1608卡片校验认证错误"},
            {0x5d, "AT88S102卡片复位错误"},
            {0x5e, "AT88S102卡片校验密码错误"},
            {0x5f, "AT88S102卡片报销"},
            {0x60, "AT88S102擦拭操作失败"},
            {0x61, "AT88S102写操作失败"},
            {0x62, "AT88S102设置密码错误"},
            {0x63, "AT88S1604复位错误"},
            {0x64, "AT88S1604校验密码错误"},
            {0x65, "AT88S1604卡片报销"},
            {0x66, "AT88S1604擦除错误"},
            {0x67, "AT88S1604写错误"},
            {0x68, "AT88S1604读错误"},
            {0x69, "SLE4442复位错误"},
            {0x6a, "SLE4442卡片报销"},
            {0x6b, "SLE4442卡片密码错误"},
            {0x70, "SLE4428复位失败"},
            {0x71, "SLE4428卡片报销"},
            {0x72, "SLE4428卡片密码校验错误"},
            {0x73, "SLE4428设置密码错误"},
            {-1, "接收数据超时（1、串口未打开；2、串口选择错误 3、返回ACK数据包或者应答数据包超时）"},
            {-3, "进(磁卡和非磁卡命令)，后进卡命令，在发送命令，规定时间内没有进卡，超时"}
        };
        

        #endregion ErrorDictionary
    }

    public class F3RfCpuCardDispenser: IRFCpuCardDispenser
    {
        public string DeviceName => "Act_F3";
        public string DeviceId => DeviceName + "_RFIC";
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;
        private int _baud;

        public F3RfCpuCardDispenser(IConfigurationManager configurationManager)
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
            _port = _configurationManager.GetValueInt("Act_F3:Port");
            _baud = _configurationManager.GetValueInt("Act_F3:Baud");
            var ret = UnSafeMethods.F3_Connect(_port,_baud,0,ref _handle);
            if (ret == 0)
            {
                _isConnectd = true;
                _currentStatus = DeviceStatus.Connected;
            return Result.Success();

            }
            Logger.Device.Error($"[发卡器{DeviceId}]连接异常，端口:{_port}");
            return Result.Fail("发卡器连接失败");
        }

        public Result Initialize()
        {
            if (_isConnectd)
            {
                int reclen = 256;
                var bts = new byte[reclen];
                var ret = UnSafeMethods.F3_Initialize(_handle,UnSafeMethods.INIT_MODE.INIT_WITHOUT_MOVEMENT, true,bts,ref reclen);
                if (ret == 0)
                {
                    return Result.Success();
                }
                var msg = GetErrorMsg(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]初始化失败，返回码:{ret} 错误原因：{msg}");
                return Result.Fail("发卡器初始化失败");
            }
            return Result.Fail("发卡器未连接");
        }

        public Result UnInitialize()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //熄灭指示灯
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //熄灭指示灯
            _isConnectd = false;
            var ret = UnSafeMethods.F3_Disconnect(_handle);
            if (ret != 0)
            {
                var msg = GetErrorMsg(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]断开连接失败， Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            return Result.Success();
        }

        public Result<byte[]> EnterCard()
        {
            var pos = new UnSafeMethods.CRSTATUS();
            var ret = UnSafeMethods.F3_GetCRStatus(_handle, ref pos);
          
            if (ret != 0)
            {
                var msg = GetErrorMsg(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]卡位置检测，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result<byte[]>.Fail(msg);
            }
            switch (pos.bLaneStatus)
            {
                case UnSafeMethods.LaneStatus.LS_NO_CARD_IN:
                case UnSafeMethods.LaneStatus.LS_CARD_IN:
                {
                    ret = UnSafeMethods.F3_MoveCard(_handle, UnSafeMethods.CardPostition.MM_RETURN_TO_RF_POS);
                    if (ret != 0)
                    {
                        var msg = GetErrorMsg(ret);
                        Logger.Device.Error(
                            $"[发卡器{DeviceId}]读卡机内无卡 进卡异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                        return Result<byte[]>.Fail(msg);
                    }
                }
                    break;
                case UnSafeMethods.LaneStatus.LS_CARD_AT_GATE_POS:
                    return Result<byte[]>.Fail("前端有卡未被取走！");
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return GetCardId();
        }

        public Result<byte[]> GetCardId()
        {
            var len = 1024;
            var data = new byte[len];
            var ret = UnSafeMethods.F3_RfcActivate(_handle, data, ref len,UnSafeMethods.RFC_PROTOCOL.RFC_PROTOCOL_TYPE_A, UnSafeMethods.RFC_PROTOCOL.RFC_PROTOCOL_TYPE_B);
            if (ret != 0)
            {
                var msg = GetErrorMsg(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]获取卡片ID失败，返回码：{ret} 错误原因:{msg}");
                return Result<byte[]>.Fail("未能正常读出卡片信息");
            }
            var realdata = data.Take(len).ToArray();
            Logger.Device.Info($"[发卡器{DeviceId}]成功获取射频CPU卡原始ID:{BitConverter.ToString(realdata)}");
            return Result<byte[]>.Success(realdata);
        }

        public Result<byte[]> CpuTransmit(byte[] order)
        {
            var len = 1024;
            var buff = new byte[len];
            var ret = UnSafeMethods.F3_TypeACpuTransmit(_handle, order,(ushort)order.Length, buff, ref len);
            if (ret != 0)
            {
                Logger.Device.Error($"[发卡器{DeviceName}]命令操作失败，命令:{BitConverter.ToString(order)}，返回码:{ret}");
                return Result<byte[]>.Fail($"指令发送失败:{GetErrorMsg(ret)}，请确认卡介质是否正常");
            }
            //len = buff.ToList().IndexOf(0);
            Logger.Device.Info($"[发卡器{DeviceName}]命令发送结果，命令:{BitConverter.ToString(order)}，返回码:{ret}，结果:{BitConverter.ToString(buff,0,len)}");
            return Result<byte[]>.Success(buff.Take(len).ToArray());
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            throw new NotImplementedException();
        }

        public Result MoveCardOut()
        {
            
            var ret = UnSafeMethods.F3_MoveCard(_handle, UnSafeMethods.CardPostition.MM_EJECT_TO_FRONT);
            if (ret != 0)
            {
                var msg = GetErrorMsg(ret);
                Logger.Device.Error($"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到从前端弹出，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            return Result.Success();
        }

        #region ErrorDictionary

        private static string GetErrorMsg(int retCode)
        {
            return UnSafeMethods.F3_ErrorDictionary.ContainsKey(retCode) ? UnSafeMethods.F3_ErrorDictionary[retCode] : "存在未知异常";
        }
        

        #endregion
    }
}
