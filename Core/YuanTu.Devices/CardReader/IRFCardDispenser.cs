using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.Common;
using static YuanTu.Devices.UnSafeMethods;

namespace YuanTu.Devices.CardReader
{
    /// <summary>
    ///     射频卡发卡机
    /// </summary>
    public interface IRFCardDispenser : IDevice
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
        ///     向卡片发送指令，并得到指令结果
        /// </summary>
        /// <param name=""></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        Result<byte[]> SendCommand(byte[] orders);

        Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null);

        /// <summary>
        ///     从前端弹出卡片
        /// </summary>
        /// <returns></returns>
        Result MoveCardOut();

        Result MoveCard(CardPostition cardPostition);
        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="iskeyA"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key);
        /// <summary>
        /// 读取块信息
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="block"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Result MfReadSector(byte sec, byte block, out byte[] data);
        /// <summary>
        /// 写入块信息
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="block"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Result MfWriteSector(byte sec, byte block, byte[] data);
    }

    public class DefaultRfCardDispenser : IRFCardDispenser
    {
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;

        public DefaultRfCardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string DeviceName { get; } = "Act_F6";
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
                case 卡片在读卡机射频卡位置 | 卡片在重读卡位置 | 卡片在IC卡位置:
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
            var data = new byte[4];
            var ret = UnSafeMethods.S50GetCardID(_handle, data);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]获取卡片ID失败，返回码：{ret} 错误原因:{msg}");
                return Result<byte[]>.Fail("未能正常读出卡片信息");
            }
            Logger.Device.Info($"[发卡器{DeviceId}]成功获取射频卡原始ID:{BitConverter.ToString(data)}");
            return Result<byte[]>.Success(data);
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            var ret = UnSafeMethods.A6_SxxVerifyPassword(_handle, sec, isKeyA, key);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]验证扇区失败，返回码：{ret} 错误原因:{msg}");
                return Result<byte[]>.Fail("读卡失败");
            }
            var data = new byte[16];
            ret = UnSafeMethods.A6_SxxReadBlock(_handle, sec, block, data);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]读块数据失败，返回码：{ret} 错误原因:{msg}");
                return Result<byte[]>.Fail("读卡失败");
            }

            return Result<byte[]>.Success(data);
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            var ret = UnSafeMethods.A6_SxxVerifyPassword(_handle, sec, isKeyA, key);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]验证扇区失败，返回码：{ret} 错误原因:{msg}");
                return Result.Fail("写卡失败");
            }

            ret = UnSafeMethods.A6_SxxWriteBlock(_handle, sec, block, data);
            if (ret != 0)
            {
                var msg = ErrorDictionary.ContainsKey(ret) ? ErrorDictionary[ret] : "";
                Logger.Device.Error($"[发卡器{DeviceId}]写块数据失败，返回码：{ret} 错误原因:{msg}");
                return Result.Fail("写卡失败");
            }

            return Result.Success();
        }

        public Result<byte[]> SendCommand(byte[] orders)
        {
            throw new NotImplementedException();
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo, List<ZbrPrintCodeItem> printCode)
        {
            return Result.Success();
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

        public Result MoveCard(CardPostition cardPostition)
        {
            throw new NotImplementedException();
        }

        public Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result MfReadSector(byte sec, byte block, out byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result MfWriteSector(byte sec, byte block, byte[] data)
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
        private const byte 卡片在重读卡位置 = 0x37;
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

    public class ZbrRfCardDispenser : IRFCardDispenser
    {
        #region Implementation of IDevice

        private IntPtr _handle;
        private int _prnType;
        private bool _isConnectd;
        private readonly IConfigurationManager _configurationManager;

        public ZbrRfCardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        /// <summary>
        ///     硬件名称
        /// </summary>
        public string DeviceName { get; } = "ZBR";

        /// <summary>
        ///     硬件唯一标识(整个系统中唯一)
        /// </summary>
        public string DeviceId => DeviceName + "_RF";

        /// <summary>
        ///     获取设备实时状态
        /// </summary>
        /// <returns></returns>
        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     1.连接设备
        /// </summary>
        /// <returns></returns>
        public Result Connect()
        {
            if (_isConnectd)
            {
                return Result.Success();
            }
            var deviceName = _configurationManager.GetValue("Printer:ZBRPrinter");

            int err;
            var ret = UnSafeMethods.ZBRGetHandle(out _handle,
                Encoding.GetEncoding("GB2312").GetBytes(deviceName).ToArray(), out _prnType, out err);
            if (ret == 1)
            {
                var icmport = _configurationManager.GetValueInt("Icm522:Port");
                var icmbaud = _configurationManager.GetValueInt("Icm522:Baud");
                var icmRet = Icm522.Open(icmport, icmbaud);
                if (icmRet.IsSuccess)
                {
                    _isConnectd = true;
                    return Result.Success();
                }
                DisConnect();
                return Result.Fail(icmRet.Message);
            }
            Logger.Device.Error($"[发卡器{DeviceId}]连接异常，打印机名称:{deviceName}");
            return Result.Fail("发卡器连接失败");
        }

        /// <summary>
        ///     2.初始化
        /// </summary>
        /// <returns></returns>
        public Result Initialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            return Result.Success();
        }

        /// <summary>
        ///     3.反初始化
        /// </summary>
        /// <returns></returns>
        public Result UnInitialize()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            return Result.Success();
        }

        /// <summary>
        ///     4.断开连接
        /// </summary>
        /// <returns></returns>
        public Result DisConnect()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            _isConnectd = false;
            int err;
            UnSafeMethods.ZBRCloseHandle(_handle, out err);
            Icm522.Close();
            return Result.Success();
        }

        #endregion

        #region Implementation of IRFCardDispenser

        /// <summary>
        ///     从卡槽中将空白卡传入
        /// </summary>
        /// <returns></returns>
        public Result<byte[]> EnterCard()
        {
            var first = GetCardId();
            if (first.IsSuccess)
            {
                return first;
            }

            int err;
            var ret = UnSafeMethods.ZBRPRNMovePrintReady(_handle, _prnType, out err);
            if (ret != 1)
            {
                Logger.Device.Error($"[发卡器{DeviceId}]移动卡位置异常，Handle:{_handle} 返回值:{ret}");
                return Result<byte[]>.Fail("发卡器进卡失败");
            }
            ret = UnSafeMethods.ZBRPRNMoveCard(_handle, _prnType, 2300, out err);
            if (ret != 1)
            {
                Logger.Device.Error($"[发卡器{DeviceId}]移动卡位置异常，Handle:{_handle} 返回值:{ret}");
                return Result<byte[]>.Fail("发卡器移动卡位置失败");
            }
            return GetCardId(5);
        }

        /// <summary>
        ///     获取卡片唯一ID
        /// </summary>
        /// <returns></returns>
        public Result<byte[]> GetCardId(int mt)
        {
            Result<byte[]> r = Result<byte[]>.Fail("");
            int times = 0;
            while ((!r.IsSuccess) && times < mt)
            {
                r = Icm522.Find();
                times++;
                Thread.Sleep(100 * times);
            }
            return r;
        }
        public Result<byte[]> GetCardId()
        {
            return GetCardId(1);
        }

        /// <summary>
        ///     读M1数据
        /// </summary>
        /// <param name="sec">扇区</param>
        /// <param name="block">块</param>
        /// <param name="isKeyA">验证是否是keyA</param>
        /// <param name="key">key值</param>
        /// <returns></returns>
        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            byte bKey;
            if (isKeyA)
            {
                bKey = 0x00;
            }
            else
            {
                bKey = 0x01;
            }
            int times = 0;
            byte[] date = null;
            while (date == null && times < 3)
            {
                date = Icm522.Read(sec, block, bKey, key);
                times++;
                Thread.Sleep(100 * times);
            }
            var result = new Result<byte[]>();
            if (date == null)
            {
                result.IsSuccess = false;
            }
            else
            {
                result.IsSuccess = true;
                result.Value = date;
            }
            return result;
        }

        /// <summary>
        ///     写M1数据
        /// </summary>
        /// <param name="sec">扇区</param>
        /// <param name="block">块</param>
        /// <param name="isKeyA">验证是否是keyA</param>
        /// <param name="key">key值</param>
        /// <param name="data">要写入的数据</param>
        /// <returns></returns>
        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            byte bKey;
            if (isKeyA)
            {
                bKey = 0x00;
            }
            else
            {
                bKey = 0x01;
            }
            int times = 0;
            var sign = false;
            while ((!sign) && times < 20)
            {
                sign = Icm522.Write(sec, block, bKey, key, data);
                times++;
                Thread.Sleep(10 * times);
            }
            Logger.Device.Info($"写卡{times.ToString()}次，结果{sign.ToString()}");
            var result = new Result { IsSuccess = sign };
            return result;
        }

        /// <summary>
        ///     向卡片发送指令，并得到指令结果
        /// </summary>
        /// <param name=""></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public Result<byte[]> SendCommand(byte[] orders)
        {
            throw new NotImplementedException();
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfoList, List<ZbrPrintCodeItem> printCodeList)
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            var graphics = new ZBRGraphics();
            var deviceName = _configurationManager.GetValue("Printer:ZBRPrinter");
            int errValue;

            var ret = graphics.IsPrinterReady(Encoding.GetEncoding("GB2312").GetBytes(deviceName), out errValue);

            if (ret != 1)
            {
                Logger.Device.Error($"[{DeviceId}]证卡打印机连接失败，返回错误码:{ret}");
                return Result.Fail("打印机连接失败");
            }
            graphics.InitGraphics(Encoding.GetEncoding("GB2312").GetBytes(deviceName), out errValue);

            if (printInfoList != null)
                foreach (var printInfo in printInfoList)
                {
                    var font = Encoding.GetEncoding("GB2312").GetBytes(printInfo.Font);
                    var text = Encoding.GetEncoding("GB2312").GetBytes(printInfo.Text);
                    var retVal = graphics.DrawText(printInfo.X, printInfo.Y, text, font, printInfo.FontSize,
                         (int)printInfo.FontStyle, 0x000000, out errValue);
                    Logger.Device.Info($"[{DeviceId}]绘制文本:<X:{printInfo.X},Y:{printInfo.Y}> 内容:{printInfo.Text} 返回值:{retVal} 出参:{errValue}");
                }
            if (printCodeList != null)
                foreach (var printCode in printCodeList)
                {
                    var code = Encoding.GetEncoding("GB2312").GetBytes(printCode.BarCodeData);
                    var retVal = graphics.DrawBarcode(printCode.X, printCode.Y, printCode.Rotation, printCode.BarCodeType,
                        printCode.WidthRatio, printCode.Multiplier, printCode.Height, printCode.TextUnder, code,
                        out errValue);
                    Logger.Device.Info(
                        $"[{DeviceId}]绘制条码[{printCode.BarCodeType}]:<X:{printCode.X},Y:{printCode.Y}> 内容:{printCode.BarCodeData} 返回值:{retVal} 出参:{errValue}");

                }

            ret = graphics.PrintGraphics(out errValue);
            graphics.CloseGraphics(out errValue);
            if (ret != 1)
            {
                return Result.Fail("打印机连接失败");
            }

            return Result.Success();
        }

        /// <summary>
        ///     从前端弹出卡片
        /// </summary>
        /// <returns></returns>
        public Result MoveCardOut()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            var graphics = new ZBRGraphics();
            var deviceName = _configurationManager.GetValue("Printer:ZBRPrinter");
            int errValue;

            var ret = graphics.IsPrinterReady(Encoding.GetEncoding("GB2312").GetBytes(deviceName), out errValue);

            if (ret != 1)
            {
                return Result.Fail("打印机连接失败");
            }
            graphics.InitGraphics(Encoding.GetEncoding("GB2312").GetBytes(deviceName), out errValue);

            var font = Encoding.GetEncoding("GB2312").GetBytes("黑体");
            graphics.DrawText(160, 55, Encoding.GetEncoding("GB2312").GetBytes(string.Empty), font, 12,
                (int)FontStyle.Bold, 0x000000, out errValue);

            ret = graphics.PrintGraphics(out errValue);
            graphics.CloseGraphics(out errValue);
            if (ret != 1)
            {
                return Result.Fail("打印机连接失败");
            }

            return Result.Success();
        }

        public Result MoveCard(CardPostition cardPostition)
        {
            throw new NotImplementedException();
        }

        public Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result MfReadSector(byte sec, byte block, out byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result MfWriteSector(byte sec, byte block, byte[] data)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ACTF3CardDispenser : IRFCardDispenser
    {
        public ACTF3CardDispenser()
        {
        }

        private readonly IConfigurationManager _configurationManager;

        private static int _port;

        private static int _baud;

        private static int _handle;

        private static int _ret;

        private static bool _isConnect;
        private enum CBS_Status : byte
        {
            CBS_EMPTY = 0x30,
            CBS_INSUFFICIENT = 0x31,
            CBS_ENOUGH = 0x32
        }
        public ACTF3CardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _port = _configurationManager.GetValueInt("Act_F3:Port");
            _baud = _configurationManager.GetValueInt("Act_F3:Baud");
        }
        public string DeviceName { get; } = "Act_F3";
        public string DeviceId => DeviceName + "_RF";

        public Result MoveCard(CardPostition cardPostition)
        {
            _ret = UnSafeMethods.F3_MoveCard(_handle, cardPostition);
            if (_ret != 0)
            {
                Logger.Device.Error($"[发卡器F3{cardPostition}]移动异常，端口:{_port}");
                return Result.Fail("发卡器移卡失败");
            }
            return Result.Success();
        }

        public Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key)
        {
            _ret = UnSafeMethods.F3_MfVerifyPassword(_handle, sec, iskeyA, key);
            if (_ret != 0)
            {
                return Result.Fail($"发卡机验证扇区{sec}密码{key}失败，错误代码{_ret}");
            }
            return Result.Success();
        }

        public Result MfReadSector(byte sec, byte block, out byte[] data)
        {
            data = new byte[512];
            var pcbLength = (uint)data.Length;
            Logger.Main.Info($"发卡机开始读{sec}扇区{block}块数据");
            _ret = UnSafeMethods.F3_MfReadSector(_handle, sec, block, data, ref pcbLength);
            Logger.Main.Info($"发卡机开始读{sec}扇区{block}块数据，返回码{_ret}");
            if (_ret != 0)
            {
                data = null;
                return Result.Fail($"发卡机读{sec}扇区{block}块数据失败，错误代码{_ret}");
            }
            Logger.Main.Info($"发卡机开始读{sec}扇区{block}块数据成功");
            return Result.Success();
        }

        public Result MfWriteSector(byte sec, byte block, byte[] data)
        {
            var length = (byte)data.Length;
            _ret = UnSafeMethods.F3_MfWriteSector(_handle, sec, block, length, data);
            if (_ret != 0)
            {
                return Result.Fail($"发卡机写{sec}扇区{block}块数据失败，错误代码{_ret}");
            }
            return Result.Success();
        }

        public Result Connect()
        {
            if (_handle != 0)
                return Result.Success();
            Logger.Main.Info($"[发卡器{DeviceId}]开始连接，端口:{_port}");
            _ret = UnSafeMethods.F3_Connect(_port, _baud, 0, ref _handle);
            if (_ret != 0)
            {
                Logger.Main.Info($"[发卡器{DeviceId}]开始连接失败，端口:{_port}");
                return Result.Fail("发卡器连接失败");
            }
            Logger.Main.Info($"[发卡器{DeviceId}]连接成功");
            _isConnect = true;
            return Result.Success();
        }

        public Result DisConnect()
        {
            if (!_isConnect)
                return Result.Fail("发卡器未连接");
            _ret = UnSafeMethods.F3_Disconnect(_handle);
            _handle = 0;
            if (_ret != 0)
            {
                return Result.Fail("发卡器断开连接失败"); ;
            }
            _handle = 0;
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
            if (pos.bLaneStatus == UnSafeMethods.LaneStatus.LS_CARD_IN || pos.bLaneStatus == UnSafeMethods.LaneStatus.LS_NO_CARD_IN)
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
            else if (pos.bLaneStatus == UnSafeMethods.LaneStatus.LS_CARD_AT_GATE_POS)
            {
                return Result<byte[]>.Fail("前端有卡未被取走！");
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            return GetCardId();
        }

        public Result<byte[]> GetCardId()
        {
            var len = 1024;
            var data = new byte[len];
            var ret = UnSafeMethods.F3_RfcActivate(_handle, data, ref len, UnSafeMethods.RFC_PROTOCOL.RFC_PROTOCOL_TYPE_A, UnSafeMethods.RFC_PROTOCOL.RFC_PROTOCOL_TYPE_B);
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

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Initialize()
        {
            throw new NotImplementedException();
        }

        public Result MoveCardOut()
        {
            var ret = UnSafeMethods.F3_MoveCard(_handle, CardPostition.MM_EJECT_TO_FRONT);
            if (ret != 0)
            {
                var msg = GetErrorMsg(ret);
                Logger.Device.Error(
                    $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到从前端弹出，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            return Result.Success();
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result<byte[]> SendCommand(byte[] orders)
        {
            throw new NotImplementedException();
        }

        public Result UnInitialize()
        {
            if (!_isConnect)
            {
                return Result.Fail("发卡器未连接");
            }
            return Result.Success();
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            throw new NotImplementedException();
        }
        private static string GetErrorMsg(int retCode)
        {
            return UnSafeMethods.F3_ErrorDictionary.ContainsKey(retCode) ? UnSafeMethods.F3_ErrorDictionary[retCode] : "存在未知异常";
        }
        #region [常量]
        public const int F3_S_SUCCESS = 0;
        public const int F3_E_PORT_UNAVAILABLE = 0x3000;
        public const int F3_E_DEV_NOT_RECOGNIZED = 0x3002;
        public const int F3_E_COMM_ERROR = 0x3010;
        public const int F3_E_COMM_TIMEOUT = 0x3011;
        public const int F3_E_UNKNOWN_ERROR = 0x3021;
        public const int F3_E_MESSAGE_TOO_LONG = 0x4010;
        public const int F3_E_NO_MEMORY = 0x4020;
        public const int F3_E_BUFFER_TOO_SMALL = 0x4000;
        public const int F3_E_INVALID_HANDLE = 0x4001;
        public const int F3_E_UNDEFINED_COMMAND = 0x4E00;
        public const int F3_E_INVALID_PARAMETER = 0x4E01;
        public const int F3_E_DISABLED_COMMAND = 0x4E02;
        public const int F3_E_UNSUPPORTED_COMMAND = 0x4E03;
        public const int F3_E_CONTACT_NO_RELEASE = 0x4E05;
        public const int F3_E_CARD_JAMMED = 0x4E10;
        public const int F3_E_SENSOR_ABNORMALITY = 0x4E12;
        public const int F3_E_TOO_LONG_CARD = 0x4E13;
        public const int F3_E_TOO_SHORT_CARD = 0x4E14;
        public const int F3_E_CARD_WITHDRAWN = 0x4E40;
        public const int F3_E_IC_SOLENOID_ERROR = 0x4E41;
        public const int F3_E_CANT_MOVED_TO_IC_POS = 0x4E43;
        public const int F3_E_CARD_POSITION_CHANGE = 0x4E45;
        public const int F3_E_COUNTER_OVERFLOW = 0x4E50;
        public const int F3_E_MOTOR_ABNORMALITY = 0x4E51;
        public const int F3_E_POWER_SHORT = 0x4E60;
        public const int F3_E_ICC_ACTIVATION_ERROR = 0x4E61;
        public const int F3_E_ICC_NOT_ACTIVATED = 0x4E65;
        public const int F3_E_UNSUPPORTED_ICC = 0x4E66;
        public const int F3_E_ICC_RECEPTION_ERROR = 0x4E67;
        public const int F3_E_ICC_COMM_TIMEOUT = 0x4E68;
        public const int F3_E_MISMATCH_EMV = 0x4E69;
        public const int F3_E_CARD_BOX_EMPTY = 0x4EA1;
        public const int F3_E_CAPTURE_BOX_FULL = 0x4EA2;
        public const int F3_E_WAITING_FOR_RESET = 0x4EB0;
        public const int F3_E_COMMAND_FAILURE = 0x6F00;
        public const int F3_E_DISAGREEMENT_OF_VC = 0x6F01;
        public const int F3_E_CARD_LOCKED = 0x6F02;
        public const int F3_E_ADDRESS_OVERFLOW = 0x6B00;
        public const int F3_E_LENGTH_OVERFLOW = 0x6700;
        #endregion
    }

    public class FagooHuaDaRfDispenser : IRFCardDispenser
    {
        private readonly string _deviceName = "USB1";
        private readonly IConfigurationManager _configurationManager;
        private readonly string _FagooName = "Fagoo P280E";
        private List<ZbrPrintTextItem> _printInfo;
        private List<ZbrPrintCodeItem> _printCode;
        private int _handle;
        private int _ret;
        public string DeviceName => "FagooHuaDa";
        public string DeviceId => DeviceName + "_RF";

        public FagooHuaDaRfDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            _FagooName = _configurationManager.GetValue("Printer:ZBRPrinter");
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            uint dwRet = 0;
            byte[] dataBuf = new byte[256];
            uint dwBufSize = 256;
            string temp;
            uint dwValue1 = 0;
            dwRet = PavoApi.PAVO_GetDeviceInfo("Fagoo P280E", PavoApi.PavoDevinfoCardPosition, dataBuf, ref dwBufSize);
            if (dwRet != 0)
            {
                return Result<DeviceStatus>.Fail("读卡器请求卡片失败");
            }
            dwValue1 = BitConverter.ToUInt32(dataBuf, 0);
            switch (dwValue1)
            {
                default:
                case 0: temp = "Out of printer"; return Result<DeviceStatus>.Success(DeviceStatus.Error);
                case 1: temp = "Start printing position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 2: temp = "Mag out position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 3: temp = "Mag in position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 4: temp = "Contact encoder position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 5: temp = "Contactless encoder position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 6: temp = "Flipper position"; return Result<DeviceStatus>.Success(DeviceStatus.Connected);
                case 7: temp = "Card jam"; return Result<DeviceStatus>.Success(DeviceStatus.Error);
            }
        }

        public Result Connect()
        {
            uint dwRet = 0;
            uint dwStatus = 0;
            dwRet = PavoApi.PAVO_CheckPrinterStatus("Fagoo P280E", ref dwStatus);
            if (dwRet == PavoApi.PavoDsOffline)
            {
                Logger.Device.Error($"[读卡器{DeviceId}打印机没有连接.");
                return Result.Fail("打印机没有连接");
            }

            else if (dwRet == PavoApi.PavoDs0200IcMissing)
            {
                Logger.Device.Error($"[读卡器{DeviceId}未检测到芯片.");
                return Result.Fail($"[读卡器{DeviceId}未检测到芯片.");
            }
            else if (dwRet == PavoApi.PavoDs0201RibbonMissing)
            {
                Logger.Device.Error($"[读卡器{DeviceId}未检测到色带.");
                return Result.Fail($"[读卡器{DeviceId}未检测到色带.");
            }
            else if (dwRet == PavoApi.PavoDs0301RibbonOut)
            {
                Logger.Device.Error($"[读卡器{DeviceId}色带用完.");
                return Result.Fail($"[读卡器{DeviceId}色带用完.");
            }
            else if (dwRet == PavoApi.PavoDs0400CardOut)
            {
                Logger.Device.Error($"[读卡器{DeviceId}卡片用完,请更换新的色带.");
                return Result.Fail($"[读卡器{DeviceId}卡片用完,请更换新的色带.");
            }
            else if (dwRet == 0)
            {
                return Result.Success();
            }
            Logger.Device.Error($"[读卡器{DeviceId}未知异常.");
            return Result.Fail("未知异常");
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
            return Result.Success();
        }

        public Result<byte[]> EnterCard()
        {
            uint dwRet = 0;
            uint dwPosition = 0;
            dwPosition = PavoApi.MoveCardToRfidEncoder;
            dwRet = PavoApi.PAVO_MoveCard(_FagooName, dwPosition);
            if (dwRet != 0)
            {
                if (dwRet == 1024)
                {
                    Restart();
                }
                Logger.Device.Error($"[读卡器{DeviceId}]:移动到非接触位置失败{_ret}");
                return Result<byte[]>.Fail("移动到非接触位置失败");
            }
            return GetCardId();
        }

        public Result<byte[]> GetCardId()
        {
            var res = HuadaConnect();
            if (!res)
            {
                return Result<byte[]>.Fail(res.Message);
            }
            _ret = PICC_Reader_Request(_handle);
            var uid = new byte[256];
            _ret = PICC_Reader_anticoll(_handle, uid);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块读取序列号异常，接口返回值:{_ret}");
                return Result<byte[]>.Fail("[读卡器{DeviceId}]华大模块读取序列号失败");
            }
            return Result<byte[]>.Success(uid);
        }

        public Result<byte[]> ReadBlock(byte sec, byte block, bool isKeyA, byte[] key)
        {
            var res = HuadaConnect();
            if (!res)
            {
                return Result<byte[]>.Fail(res.Message);
            }
            _ret = UnSafeMethods.PICC_Reader_Authentication_Pass(_handle, isKeyA ? (byte)Mode.KeyA : (byte)Mode.KeyB,
                sec, key);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块验证扇区失败，返回码：{_ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }
            var data = new byte[16];
            _ret = UnSafeMethods.PICC_Reader_Read(_handle, block, data);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块读块数据失败，返回码：{_ret} ");
                return Result<byte[]>.Fail("读卡失败");
            }

            return Result<byte[]>.Success(data);
        }

        public Result WirteBlock(byte sec, byte block, bool isKeyA, byte[] key, byte[] data)
        {
            var res = HuadaConnect();
            if (!res)
            {
                return Result.Fail(res.Message);
            }
            _ret = UnSafeMethods.PICC_Reader_Authentication_Pass(_handle, isKeyA ? (byte)Mode.KeyA : (byte)Mode.KeyB,
                sec, key);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块验证扇区失败，返回码：{_ret} ");
                return Result.Fail("写卡失败");
            }

            _ret = UnSafeMethods.PICC_Reader_Write(_handle, block, data);
            if (_ret != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块写块数据失败，返回码：{_ret} ");
                return Result.Fail("写卡失败");
            }

            return Result.Success();
        }

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            try
            {
                _printInfo = printInfo;
                PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = _FagooName;
                pd.DefaultPageSettings.Margins.Left = 0;
                pd.DefaultPageSettings.Margins.Top = 0;
                pd.DefaultPageSettings.Margins.Right = 0;
                pd.DefaultPageSettings.Margins.Bottom = 0;
                pd.DocumentName = "远图";
                pd.PrintPage += pd_PrintPage;
                pd.PrintController = new StandardPrintController();
                pd.Print();
                pd.PrintPage -= pd_PrintPage;
                return Result.Success();
            }
            catch (Exception e)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]打印异常，接口返回值:{_ret}");
                return Result.Fail(e.Message);
            }

        }

        public Result MoveCardOut()
        {
            return MoveCard(CardPostition.MM_RETURN_TO_FRONT);
        }

        public Result MoveCard(CardPostition cardPostition)
        {
            uint dwRet = 0;
            uint dwPosition = 0;
            switch (cardPostition)
            {
                case CardPostition.MM_EJECT_TO_FRONT:
                case CardPostition.MM_CAPTURE_TO_BOX:
                case CardPostition.MM_RETURN_TO_FRONT:
                    dwPosition = PavoApi.MoveCardToHopper;
                    dwRet = PavoApi.PAVO_MoveCard(_FagooName, dwPosition);
                    if (dwRet != 0)
                    {
                        Logger.Device.Error($"[读卡器{DeviceId}]出卡失败 返回码:{dwRet}");
                        return Result.Fail("移动卡片到出卡盒失败.");
                    }
                    return Result.Success();
                case CardPostition.MM_RETURN_TO_IC_POS:
                    dwPosition = PavoApi.MoveCardToIcEncoder;
                    dwRet = PavoApi.PAVO_MoveCard(_FagooName, dwPosition);
                    if (dwRet != 0)
                    {
                        Logger.Device.Error($"[读卡器{DeviceId}]出卡失败 返回码:{dwRet}");
                        return Result.Fail("移动卡片到出卡盒失败.");
                    }
                    return Result.Success();
                case CardPostition.MM_RETURN_TO_RF_POS:
                    dwPosition = PavoApi.MoveCardToRfidEncoder;
                    dwRet = PavoApi.PAVO_MoveCard(_FagooName, dwPosition);
                    if (dwRet != 0)
                    {
                        Logger.Device.Error($"[读卡器{DeviceId}]出卡失败 返回码:{dwRet}");
                        return Result.Fail("移动卡片到出卡盒失败.");
                    }
                    return Result.Success();
                default:
                    throw new ArgumentOutOfRangeException(nameof(cardPostition), cardPostition, null);
            }
        }

        private void Restart()
        {
            uint dwRet = 0;
            uint dwCommand = 0;
            dwCommand = PavoApi.PavoCommandResetPrinter;
            dwRet = PavoApi.PAVO_DoCommand(_FagooName, dwCommand);
            if (dwRet != 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]重启异常 返回码:{_ret}");
            }
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            Image colorImg = null;
            IntPtr hPrinterDC;
            e.Graphics.PageUnit = GraphicsUnit.Pixel;
            hPrinterDC = e.Graphics.GetHdc();
            e.Graphics.ReleaseHdc(hPrinterDC);
            foreach (var info in _printInfo)
            {
                e.Graphics.DrawString(info.Text, new Font(new FontFamily(info.Font), info.FontSize), System.Drawing.Brushes.Black, info.X, info.Y);
            }
            //e.Graphics.DrawString(info, new Font(new FontFamily("宋体"), 12), System.Drawing.Brushes.Black, 180, 48);
            //e.Graphics.DrawString(cardNo, new Font(new FontFamily("宋体"), 12), System.Drawing.Brushes.Black, 560, 48);

        }

        private Result HuadaConnect()
        {
            _handle = ICC_Reader_Open(_deviceName);
            if (_handle <= 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块连接异常，接口返回值:{_handle}");
                return Result.Fail($"读卡器{DeviceId}]华大模块连接异常");
            }

            _ret = ICC_PosBeep(_handle, 30);
            if (_ret < 0)
            {
                Logger.Device.Error($"[读卡器{DeviceId}]华大模块初始化（蜂鸣）异常，接口返回值:{_ret}");
                return Result.Fail($"读卡器{DeviceId}华大模块初始化（蜂鸣）异常");
            }
            return Result.Success();
        }

        #region UnUseMethods

        public Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result MfReadSector(byte sec, byte block, out byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result MfWriteSector(byte sec, byte block, byte[] data)
        {
            throw new NotImplementedException();
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

        #endregion
    }

    public class Kc100RfDispenser : IRFCardDispenser
    {
        public string DeviceName => "KC100";
        public string DeviceId => DeviceName + "_RF";
        private static int _handle;
        public static string Port = "USB";
        public static int Baud = 115200;
        private static UInt32 ui32Result;
        private static UInt32 ui32Status, ui32Config, ui32Information, ui32Warning, ui32Error, ui32Ext1, ui32Ext2, ui32Ext3, ui32Ext4;

        public Result Connect()
        {
            Logger.Device.Info("KC100Self开始连接");
            ui32Result = FangDian_OpenPrinter();
            if (ui32Result != ERROR_SUCCESS)
            {
                Logger.Device.Info("KC100Self连接失败");
                return (ui32Result & 0xFF000000) == ERROR_EVOLIS ? Result.Fail($"Evolis打印机内部自定义错误，错误号：0x{(ui32Result & 0xFF000000):X08}") :
                    Result.Fail($"Windows系统错误,可以通过FormatMessage获得错误描述,错误号:0x{ui32Result:X08}");
            }
            else
            {
                Logger.Device.Info("KC100MT开始连接");
                var mtconnResult = ConnectMt();
                if (!mtconnResult)
                {
                    return mtconnResult;
                }
            }
            return Result.Success();
        }

        private Result ConnectMt()
        {
            if (_handle <= 0)
            {
                _handle = Mt_Open(Port, Baud);
                if (_handle > 0)
                {
                    return Result.Success();
                }
                Logger.Device.Info($"爱丽丝明泰连接失败");
                return Result.Fail("爱丽丝明泰连接失败");
            }
            return Result.Success();
        }

        public Result Initialize()
        {
            var statusRes = GetDeviceStatus();
            if (!statusRes && !string.IsNullOrEmpty(statusRes.Message))
            {
                return Result.Fail($"打印机状态异常,请联系管理人员{statusRes.Message}");
            }
            return Result.Success();
        }

        public Result UnInitialize()
        {
            return Result.Success();
        }

        private static void MtClose()
        {
            Mt_Close(_handle);
            _handle = -1;
        }

        public Result DisConnect()
        {
            ui32Result = FangDian_ClosePrinter();
            if (ui32Result != ERROR_SUCCESS)
            {
                Logger.Device.Info($"关闭打印机错误，错误号：0x{ui32Result:X08}");
                return Result.Fail($"关闭打印机错误，错误号：0x{ui32Result:X08}");
            }
            else
            {
                Mt_Close(_handle);
                return Result.Success();
            }
        }

        public Result<byte[]> EnterCard()
        {
            var moveRes = MoveCard(CardPostition.Contactless);
            if (!moveRes)
            {
                return Result<byte[]>.Fail(moveRes.Message);
            }
            var readRes = GetCardId();
            if (!readRes)
            {
                return Result<byte[]>.Fail(moveRes.Message);
            }
            return Result<byte[]>.Success(readRes.Value);
        }

        public Result<byte[]> GetCardId()
        {
            byte[] uidData = new byte[4];
            var res = Mt_card(_handle, 0, uidData);
            if (res == 0)
            {
                return Result<byte[]>.Success(uidData);
            }
            else
            {
                MtClose();
                ConnectMt();
                res = Mt_card(_handle, 0, uidData);
                if (res == 0)
                {
                    return Result<byte[]>.Success(uidData);
                }
            }
            Logger.Device.Info($"爱丽丝明泰读卡失败：返回码{res}");
            return Result<byte[]>.Fail($"爱丽丝明泰读卡失败：返回码{res}");
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

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            if (printInfo == null)
            {
                return Result.Success();
            }
            var moveRes = MoveCard(CardPostition.PrintReady);
            if (!moveRes)
            {
                return moveRes;
            }
            printInfo.ForEach(p =>
            {
                FangDian_PrintText(p.X, p.Y, p.Text, p.Font, 12, FW_BOLD, 0, true);
            });
            FangDian_PrintFlush();
            return Result.Success();
        }

        public Result MoveCardOut()
        {
            return Result.Success();
        }

        public Result MoveCard(UnSafeMethods.CardPostition cardPostition)
        {
            throw new NotImplementedException();
        }

        public Result MoveCard(CardPostition cardPostition)
        {
            ui32Result = FangDian_MoveCard((Int32)cardPostition);
            if (ui32Result != ERROR_SUCCESS)
            {
                Logger.Device.Info($"移动卡片{(Int32)cardPostition}失败");
                return Result.Fail($"移动卡片{(Int32)cardPostition}失败");
            }
            return Result.Success();
        }

        public Result MfVerifyPassword(byte sec, bool iskeyA, byte[] key)
        {
            throw new NotImplementedException();
        }

        public Result MfReadSector(byte sec, byte block, out byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result MfWriteSector(byte sec, byte block, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            var data = new string[17];
            ui32Result = FangDian_GetStatus(out ui32Status, out ui32Config, out ui32Information, out ui32Warning, out ui32Error, out ui32Ext1, out ui32Ext2, out ui32Ext3, out ui32Ext4);
            if (ui32Result == 0)
            {
                // 未描述状态或错误，请参考 ReferenceGuide_PremiumSDK.pdf 《ESPF – 技术规格 A4》5.1节
                data[0] = ((ui32Config & TYPEOF_PRIMACY) != 0) ? "Primacy" : (((ui32Config & TYPEOF_ZENIUS) != 0) ? "Zenius" : (((ui32Config & TYPEOF_ELYPSO) != 0) ? "Elypso" : "Unknown"));
                data[1] = ((ui32Config & 0x00100000) != 0) ? ",双面" : ",单面";
                data[2] = ui32Status.ToString() + ": {" + ui32Config.ToString("X08") + "-" + ui32Information.ToString("X08") + "-" + ui32Warning.ToString("X08") + "-" + ui32Error.ToString("X08") + "-" + ui32Ext1.ToString("X08") + "-" + ui32Ext2.ToString("X08") + "-" + ui32Ext3.ToString("X08") + "-" + ui32Ext4.ToString("X08") + "}";
                data[3] = ((ui32Config & 0x00800000) != 0) ? "可用" : "不可用";
                data[4] = ((ui32Config & 0x00400000) != 0) ? "可用" : "不可用";
                data[5] = ((ui32Config & 0x00080000) != 0) ? "有" : "无";
                data[6] = ((ui32Config & 0x00040000) != 0) ? "有" : "无";
                data[7] = ((ui32Config & 0x00020000) != 0) ? "有" : "无";
                data[8] = ((ui32Config & 0x00000040) != 0) ? "有" : "无";

                data[9] = ((ui32Information & 0x20000000) == 0x20000000) ? "有卡片" : "无卡片";
                data[10] = ((ui32Information & 0x10000000) == 0x10000000) ? "有卡片" : "无卡片";
                data[11] = ((ui32Information & 0x08000000) == 0x08000000) ? "有卡片" : "无卡片";
                data[12] = ((ui32Information & 0x04000000) == 0x04000000) ? "有卡片" : "无卡片";
                data[13] = ((ui32Information & 0x02000000) == 0x02000000) ? "有卡片" : "无卡片";
                data[14] = ((ui32Information & 0x01000000) == 0x01000000) ? "有卡片" : "无卡片";

                data[15] = ((ui32Information & 0x00200000) == 0x00200000) ? "待机模式" : "联机模式";

                StringBuilder sbrErr = new StringBuilder();
                // ERROR
                if ((ui32Error & ERR_BLANK_TRACK) == ERR_BLANK_TRACK) sbrErr.Append("\r\n磁条编码故障，请检查送卡器的卡片位置");
                if ((ui32Error & ERR_COVER_OPEN) == ERR_COVER_OPEN) sbrErr.Append("\r\n盖子被打开");
                if ((ui32Error & ERR_HEAD_TEMP) == ERR_HEAD_TEMP) sbrErr.Append("\r\n打印头温度过高");
                if ((ui32Error & ERR_MAGNETIC_DATA) == ERR_MAGNETIC_DATA) sbrErr.Append("\r\n磁条编码故障，无效数据格式");
                if ((ui32Error & ERR_MECHANICAL) == ERR_MECHANICAL) sbrErr.Append("\r\n机械故障");
                if ((ui32Error & ERR_READ_MAGNETIC) == ERR_READ_MAGNETIC) sbrErr.Append("\r\n磁道读取故障");
                if ((ui32Error & ERR_REJECT_BOX_FULL) == ERR_REJECT_BOX_FULL) sbrErr.Append("\r\n废料箱满");
                if ((ui32Error & ERR_RIBBON_ERROR) == ERR_RIBBON_ERROR) sbrErr.Append("\r\n色带已剪切或与卡片粘合");
                if ((ui32Error & ERR_WRITE_MAGNETIC) == ERR_WRITE_MAGNETIC) sbrErr.Append("\r\n磁条写后读故障");
                if ((ui32Warning & DEF_FEEDER_EMPTY) == DEF_FEEDER_EMPTY) sbrErr.Append("\r\n送卡器问题，请检查送卡器的卡片位置并进行隔距调整");
                if ((ui32Information & INF_WRONG_ZONE_EXPIRED) == INF_WRONG_ZONE_EXPIRED) sbrErr.Append("\r\n色带无效或已达到打印次数限制");
                if ((ui32Error & ERR_RIBBON_ENDED) == ERR_RIBBON_ENDED) sbrErr.Append("\r\n色带耗尽，请更换新色带");
                if ((ui32Ext2 & ERR_LAMINATE_END) == ERR_LAMINATE_END) sbrErr.Append("\r\n薄膜用完。请更换薄膜");
                if ((ui32Ext2 & ERR_LAMINATE) == ERR_LAMINATE) sbrErr.Append("\r\n薄膜已剪切或与卡片粘连");
                if ((ui32Ext2 & ERR_LAMI_MECHANICAL) == ERR_LAMI_MECHANICAL) sbrErr.Append("\r\n覆膜模块中发生机械错误");
                if ((ui32Ext2 & ERR_LAMI_TEMPERATURE) == ERR_LAMI_TEMPERATURE) sbrErr.Append("\r\n覆膜模块出现温度错误");
                if ((ui32Ext2 & ERR_LAMI_COVER_OPEN) == ERR_LAMI_COVER_OPEN) sbrErr.Append("\r\n覆膜模块机盖在覆膜过程中打开,请将其关闭并重试");

                if (sbrErr.Length > 0)
                {
                    return Result<DeviceStatus>.Fail(sbrErr.ToString());
                }

                StringBuilder sbrWarning = new StringBuilder();
                // WARNING
                if ((ui32Information & INF_LOW_RIBBON) == INF_LOW_RIBBON) sbrWarning.Append("\r\n色带快耗尽了，请补充后继续");
                if ((ui32Ext1 & INF_FEEDER_NEAR_EMPTY) == INF_FEEDER_NEAR_EMPTY) sbrWarning.Append("\r\n送卡器的卡片即将用尽，请重新装入卡片");
                if ((ui32Warning & DEF_COOLING) == DEF_COOLING) sbrWarning.Append("\r\n打印机正在冷却");
                if ((ui32Warning & DEF_COVER_OPEN) == DEF_COVER_OPEN) sbrWarning.Append("\r\n请关闭您的打印机盖");
                if ((ui32Warning & DEF_HOPPER_FULL) == DEF_HOPPER_FULL) sbrWarning.Append("\r\n输出托盒满,请从输出托盒取出所有打印卡片以继续打印操作");
                if ((ui32Warning & DEF_NO_RIBBON) == DEF_NO_RIBBON) sbrWarning.Append("\r\n无色带");
                if ((ui32Warning & DEF_PRINTER_LOCKED) == DEF_PRINTER_LOCKED) sbrWarning.Append("\r\n与打印机的通信被锁定");
                if ((ui32Warning & DEF_UNSUPPORTED_RIBBON) == DEF_UNSUPPORTED_RIBBON) sbrWarning.Append("\r\n插入的色带不适用于该打印机型号");
                if ((ui32Warning & DEF_WAIT_CARD) == DEF_WAIT_CARD) sbrWarning.Append("\r\n请手动插入您的卡片");
                if ((ui32Information & INF_CLEANING_ADVANCED) == INF_CLEANING_ADVANCED) sbrWarning.Append("\r\n需要打印机高级清洁");
                if ((ui32Information & INF_CLEAN_LAST_OUTWARRANTY) == INF_CLEAN_LAST_OUTWARRANTY) sbrWarning.Append("\r\n必须定期清洁，请点击‘取消’并立即清洁");
                if ((ui32Information & INF_CLEANING_REQUIRED) == INF_CLEANING_REQUIRED) sbrWarning.Append("\r\n必须定期清洁——您的管理员不允许发卡");
                if ((ui32Information & INF_UNKNOWN_RIBBON) == INF_UNKNOWN_RIBBON) sbrWarning.Append("\r\n无法识别色带请使用手动设置继续");
                if ((ui32Information & INF_WRONG_ZONE_ALERT) == INF_WRONG_ZONE_ALERT) sbrWarning.Append("\r\n色带和打印机存在兼容性问题。只剩少于50次打印输出。请联系经销商");
                if ((ui32Information & INF_WRONG_ZONE_RIBBON) == INF_WRONG_ZONE_RIBBON) sbrWarning.Append("\r\n色带和打印机存在兼容性问题。请联系经销商");
                if ((ui32Warning & DEF_RIBBON_ENDED) == DEF_RIBBON_ENDED) sbrWarning.Append("\r\n色带耗尽，请更换新色带");
                if ((ui32Ext2 & DEF_NO_LAMINATE) == DEF_NO_LAMINATE) sbrWarning.Append("\r\n覆膜模块中没有薄膜。请更换薄膜");
                if ((ui32Ext2 & INF_LAMINATE_UNKNOWN) == INF_LAMINATE_UNKNOWN) sbrWarning.Append("\r\n未知薄膜。请联系经销商");
                if ((ui32Ext2 & INF_LAMINATE_LOW) == INF_LAMINATE_LOW) sbrWarning.Append("\r\n薄膜即将用尽。请安排补充");
                if ((ui32Ext2 & DEF_LAMINATE_END) == DEF_LAMINATE_END) sbrWarning.Append("\r\n薄膜用完。请更换薄膜");
                if ((ui32Ext2 & DEF_LAMINATE_UNSUPPORTED) == DEF_LAMINATE_UNSUPPORTED) sbrWarning.Append("\r\n薄膜与覆膜模块不兼容。请联系经销商");
                if ((ui32Ext2 & DEF_LAMI_COVER_OPEN) == DEF_LAMI_COVER_OPEN) sbrWarning.Append("\r\n覆膜模块的机盖打开。关闭覆膜模块机盖");
                if ((ui32Ext1 & INF_LAMI_TEMP_NOT_READY) == INF_LAMI_TEMP_NOT_READY) sbrWarning.Append("\r\n薄膜模块温度正在调整。请稍候");
                if ((ui32Ext2 & DEF_LAMI_HOPPER_FULL) == DEF_LAMI_HOPPER_FULL) sbrWarning.Append("\r\n覆膜输出受阻。请取出卡片并重试");
                if (sbrWarning.Length>0)
                {
                    return Result<DeviceStatus>.Fail(sbrWarning.ToString());
                }

                StringBuilder sbr = new StringBuilder();
                // READY
                if ((ui32Information & INF_CLEANING) == INF_CLEANING) sbr.Append("\r\n打印机需要清洁");
                if ((ui32Information & INF_CLEAN_2ND_PASS) == INF_CLEAN_2ND_PASS) sbr.Append("\r\n请插入您的粘附式清洁卡,如果您想要继续打印则‘取消’");
                if ((ui32Information & INF_CLEANING_RUNNING) == INF_CLEANING_RUNNING) sbr.Append("\r\n正在清洁");
                if ((ui32Information & INF_ENCODING_RUNNING) == INF_ENCODING_RUNNING) sbr.Append("\r\n正在编码");
                if ((ui32Information & INF_PRINTING_RUNNING) == INF_PRINTING_RUNNING) sbr.Append("\r\n正在打印");
                if ((ui32Information & INF_LAMINATING_RUNNING) == INF_LAMINATING_RUNNING) sbr.Append("\r\n正在覆膜");
                if ((ui32Ext2 & INF_LAMI_CLEANING_RUNNING) == INF_LAMI_CLEANING_RUNNING) sbr.Append("\r\n覆膜模块正在清洁");
                if ((ui32Information & INF_SLEEP_MODE) == INF_SLEEP_MODE) sbr.Append("\r\n打印机处于待机状态");

                return Result<DeviceStatus>.Success(DeviceStatus.Idle);
            }
            return Result<DeviceStatus>.Success(DeviceStatus.Idle);
        }

        public enum CardPostition
        {
            /// <summary>
            /// 触点位置
            /// </summary>
            SmartCard = 0x00,       // Sis
            /// <summary>
            /// 打印位置
            /// </summary>
            PrintReady = 0x01,      // Si
            /// <summary>
            /// 非接触位
            /// </summary>
            Contactless = 0x02,     // Sic
            /// <summary>
            /// 回收箱
            /// </summary>
            EjectBack = 0x03,       // Seb
            /// <summary>
            /// 排卡
            /// </summary>
            Reject = 0x04,          // Ser
            /// <summary>
            /// 废卡箱
            /// </summary>
            Eject = 0x05,           // Se
        };

        #region font
        public const Int32 FW_DONTCARE = 0;
        public const Int32 FW_THIN = 100;
        public const Int32 FW_EXTRALIGHT = 200;
        public const Int32 FW_LIGHT = 300;
        public const Int32 FW_NORMAL = 400;
        public const Int32 FW_MEDIUM = 500;
        public const Int32 FW_SEMIBOLD = 600;
        public const Int32 FW_BOLD = 700;
        public const Int32 FW_EXTRABOLD = 800;
        public const Int32 FW_HEAVY = 900;
        #endregion
    }
}