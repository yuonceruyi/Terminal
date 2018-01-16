using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShengZhouHospital.Special
{
    public class MagCardDispenserByF6ForZHIFEIKA : IMagCardDispenser
    {
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;

        public MagCardDispenserByF6ForZHIFEIKA(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }
        public string DeviceName { get; } = "Act_F6";
        public string DeviceId => DeviceName + "_CPU";
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
        public Result Initialize()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //点亮指示灯
            UnSafeMethods.Led1Control(_handle, 0x31);
            return Result.Success();
        }
        public Result UnInitialize()
        {
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //熄灭指示灯
            UnSafeMethods.Led1Control(_handle, 0x30);
            return Result.Success();
        }
        public Result MoveCardOut()
        {
            if (!_isConnectd)
            {
                var result = Connect();
                if (!result.IsSuccess)
                {
                    Logger.Device.Error($"[发卡器{DeviceId}] {result.Message}");
                    return Result.Fail($"[发卡器{DeviceId}] {result.Message}");
                }
            }
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
        public Result Recovery()
        {
            var ret = UnSafeMethods.MoveCard(_handle, 吞入);
            return ret == 0 ? Result.Success() : Result.Fail("回收失败");
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
                case 吞入:
                    {
                        ret = UnSafeMethods.EnterCard(_handle, 发卡, 20 * 1000);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 进卡异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移到IC卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 移到IC卡位置内部异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        break;
                    }
                case 卡片在读卡机射频卡位置:
                    {
                        ret = UnSafeMethods.MoveCard(_handle, 移到IC卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移到IC卡位置异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<byte[]>.Fail(msg);
                        }
                        break;
                    }
                case 卡片在IC卡位置:
                    {
                        break;
                    }
                default:
                    {
                        Logger.Device.Error($"[发卡器{DeviceId}]卡所在的位置异常,卡位置代码:0X{pos.ToString("X")}");
                        return Result<byte[]>.Fail($"发卡器内存在未被取走的卡！");
                    }
            }
            return GetCardNo();
        }
        public Result Reset()
        {
            var ret = UnSafeMethods.Sle4428Reset(_handle);
            return ret == 0 ? Result.Success() : Result.Fail("复位失败");
        }
        public Result VerifyPWD(int n)
        {
            var bytes = new[] { Convert.ToByte(n >> 8), Convert.ToByte(n & 255) };
            var ret =UnSafeMethods.Sle4428VerifyPWD(_handle, bytes);
            return ret == 0?Result.Success():Result.Fail("密码验证失败");
        }
        public Result Write(int address, byte[] data)
        {
            if (data.Length > 255)
            {
                return Result.Fail("data过长");
            }
            var ret = UnSafeMethods.Sle4428Write(_handle, address, Convert.ToByte(data.Length), data);
            return ret == 0 ? Result.Success() : Result.Fail("写入失败");
        }
        public Result<byte[]> GetCardNo()
        {
            Logger.Device.Info($"开始上电");
            var ret = UnSafeMethods.IcCardPowerOn(_handle);
            Logger.Device.Info($"开始读卡");
            var bytes = new byte[70];
            ret = UnSafeMethods.Sle4428Read(_handle, 100, 70, bytes);
            if (bytes[0] == 0xFF || bytes[50] == 0xFF)
            {
                Logger.Device.Info($"卡类型不对或放置方向错误");
                return Result<byte[]>.Fail("卡类型不对或放置方向错误");
            }
            var CardId = Encoding.ASCII.GetString(bytes, 0, 30).Trim();
            var  CardNo = Encoding.ASCII.GetString(bytes, 50, 20).Trim();
            return Result<byte[]>.Success(bytes);
         
        }
        public Result<Dictionary<TrackRoad, string>> EnterCard(TrackRoad road)
        {
            throw new NotImplementedException();

        }
        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            throw new NotImplementedException();
        }
        public Result MoveCard(CardPosF6 destPos, string reason = null)
        {
            throw new NotImplementedException();
        }

        public Result<Dictionary<TrackRoad, string>> ReadTrack(TrackRoad road)
        {
            throw new NotImplementedException();
        }
        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
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

        private static int ret;

        private static int Ret
        {
            set
            {
                ret = value;
                if (ret == 0)
                    return;

                string message;
                ErrorDictionary.TryGetValue(ret, out message);
                throw new Exception(message);
            }
            get { return ret; }
        }

        #endregion ErrorDictionary
    }
}
