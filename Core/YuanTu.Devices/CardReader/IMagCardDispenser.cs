using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.Common;

namespace YuanTu.Devices.CardReader
{
    /// <summary>
    ///     磁条发卡机
    /// </summary>
    public interface IMagCardDispenser : IDevice
    {
        /// <summary>
        ///     从卡槽中将空白卡传入机具内，并读出指定磁道内容
        /// </summary>
        /// <returns></returns>
        Result<Dictionary<TrackRoad, string>> EnterCard(TrackRoad road);

        /// <summary>
        ///     读取射频卡位上指定的磁道信息
        /// </summary>
        /// <param name="road">需要读取的磁道枚举</param>
        /// <returns></returns>
        Result<Dictionary<TrackRoad, string>> ReadTrack(TrackRoad road);

        /// <summary>
        ///     从前端弹出卡片
        /// </summary>
        /// <returns></returns>
        Result MoveCardOut();
        Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null);

        /// <summary>
        /// 移动卡片到目标位置
        /// </summary>
        /// <param name="destPos"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        Result MoveCard(CardPosF6 destPos, string reason = null);
    }

    public class MagCardDispenser : IMagCardDispenser
    {
        private readonly IConfigurationManager _configurationManager;
        private DeviceStatus _currentStatus = DeviceStatus.UnInitialized;
        private int _handle;
        private bool _isConnectd;
        private int _port;

        public MagCardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        public string DeviceName { get; } = "Act_F6";
        public string DeviceId => DeviceName + "_Mag";

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
            if (!_isConnectd)
                return Result.Fail("发卡器未连接");
            //点亮指示灯
            UnSafeMethods.Led1Control(_handle, 0x31);
            return Result.Success();
        }

        public Result<Dictionary<TrackRoad, string>> EnterCard(TrackRoad road)
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
                return Result<Dictionary<TrackRoad, string>>.Fail(msg);
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
                            return Result<Dictionary<TrackRoad, string>>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移动到重入卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 移动到重入卡位置异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<Dictionary<TrackRoad, string>>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移到读卡器内部);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]读卡机内无卡 移动到读卡器内部异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<Dictionary<TrackRoad, string>>.Fail(msg);
                        }
                        break;
                    }
                case 卡片在读卡机射频卡位置:
                case 卡片在重读卡位置:
                case 卡片在IC卡位置:
                    {
                        ret = UnSafeMethods.MoveCard(_handle, 移动到重入卡位置);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到重入卡位置异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<Dictionary<TrackRoad, string>>.Fail(msg);
                        }
                        ret = UnSafeMethods.MoveCard(_handle, 移到读卡器内部);
                        if (ret != 0)
                        {
                            var msg = buildErrorAction(ret);
                            Logger.Device.Error(
                                $"[发卡器{DeviceId}]卡片在读卡机射频卡位置 移动到读卡器内部异常，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                            return Result<Dictionary<TrackRoad, string>>.Fail(msg);
                        }
                        break;
                    }
                default:
                    {
                        Logger.Device.Error($"[发卡器{DeviceId}]卡片在读卡机射频卡位置 卡所在的位置异常,卡位置代码:0X{pos.ToString("X")}");
                        return Result<Dictionary<TrackRoad, string>>.Fail($"发卡器内存在未被取走的卡！");
                    }
            }
            return ReadTrack(road);
        }

        public Result<Dictionary<TrackRoad, string>> ReadTrack(TrackRoad road)
        {
            var mapping = new Dictionary<TrackRoad, byte>
            {
                [TrackRoad.Trace1] = 0x00,
                [TrackRoad.Trace2] = 0x31,
                [TrackRoad.Trace3] = 0x04
            };
            var enums = road.GetEnums();
            byte bt = 0x00;
            foreach (var trackRoad in enums)
            {
                bt |= mapping[trackRoad];
            }

            var len = 512;
            var data = new byte[len];
            Ret = UnSafeMethods.ReadMagcardDecode(_handle, 0x33, ref len, data); //读全3轨信息
            //Flag len
            if (len <= 6 /*|| data[0] != 0x60 || data[2] != 0x60 || data[4] != 0x60*/)
            {
                return Result<Dictionary<TrackRoad, string>>.Fail("无法读取磁道信息");
            }

            var dic = new Dictionary<TrackRoad, string>();
            int dataLen1 = data[1];
            int dataLen2 = data[3];
            int dataLen3 = data[5];
            if ((road & TrackRoad.Trace2) == TrackRoad.Trace2)
            {
                if (dataLen2 == 0)
                    return Result<Dictionary<TrackRoad, string>>.Fail("无法读取2磁道信息");
                var blist2 = new List<byte>();
                for (var i = 0; i < dataLen2; i++)
                {
                    blist2.Add(data[i + 4]);
                }
                var tmp2 = new byte[blist2.Count];
                blist2.CopyTo(tmp2);
                var track2 = Encoding.Default.GetString(tmp2);
                dic[TrackRoad.Trace2] = track2;
            }
            if ((road & TrackRoad.Trace3) == TrackRoad.Trace3)
            {
                if (dataLen3 == 0)
                    return Result<Dictionary<TrackRoad, string>>.Fail("无法读取3磁道信息");
                var blist3 = new List<byte>();
                for (var i = 0; i < dataLen3; i++)
                {
                    blist3.Add(data[i + 4 + dataLen2]);
                }
                var tmp3 = new byte[blist3.Count];
                blist3.CopyTo(tmp3);
                var track3 = Encoding.Default.GetString(tmp3);
                dic[TrackRoad.Trace3] = track3;
            }
            var kvs = dic.Select(p => $"{p.Key}:{p.Value}");
            Logger.Device.Info($"[发卡器{DeviceId}]成功获取卡片磁条信息:{string.Join(";", kvs)}");
            return Result<Dictionary<TrackRoad, string>>.Success(dic);
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

        public Result PrintContent(List<ZbrPrintTextItem> printInfo = null, List<ZbrPrintCodeItem> printCode = null)
        {
            throw new NotImplementedException();
        }

        public Result MoveCard(CardPosF6 destPos,string reason=null)
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
            var ret = UnSafeMethods.MoveCard(_handle, (byte)destPos);
            if (ret != 0)
            {
                var msg = buildErrorAction(ret);
                Logger.Device.Error(
                    $"[发卡器{DeviceId}移动卡到目标位置[{destPos}]，原因:{reason}，端口:{_port} Handle:{_handle} 返回值:{ret} 返回值解析:{msg}");
                return Result.Fail(msg);
            }
            Logger.Device.Info(
                    $"[发卡器{DeviceId}移动卡到目标位置[{destPos}]，原因:{reason}，端口:{_port} Handle:{_handle} 返回值:{ret}");
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

        public Result SetHandle(int handle)
        {
            throw new NotImplementedException();
        }

        public Result<int> GetHandle()
        {
            throw new NotImplementedException();
        }

        #endregion ErrorDictionary
    }

    public class ZbrMagCardDispenser : IMagCardDispenser
    {
        private IntPtr _handle;
        private int _prnType;
        private bool _isConnectd;
        private readonly IConfigurationManager _configurationManager;

        public string DeviceName { get; } = "ZBR";
        public string DeviceId => DeviceName + "_Mag";

        public ZbrMagCardDispenser(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
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
                _isConnectd = true;
                return Result.Success();
            }
            Logger.Device.Error($"[发卡器{DeviceId}]连接异常，打印机名称:{deviceName}");
            return Result.Fail("发卡器连接失败");
        }

        public Result DisConnect()
        {
            if (!_isConnectd)
            {
                return Result.Fail("发卡器未连接");
            }
            _isConnectd = false;
            int err;
            UnSafeMethods.ZBRCloseHandle(_handle, out err);            
            return Result.Success();
        }

        public Result<Dictionary<TrackRoad, string>> EnterCard(TrackRoad road)
        {
            int err;
            var track = new byte[128];
            int size;
            var ret = UnSafeMethods.ZBRPRNReadMagByTrk(_handle, _prnType,2, track, out size, out err);           
            var text = Encoding.ASCII.GetString(track, 0, size);
            var sb = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                sb.Append($"{track[i]:X} ");
            }
            if (ret != 1)
            {
                Logger.Device.Error($"发卡器 Read() ret={ret} err={err} {GetError(err)}\n Track={text} Size={size} Hex={sb}");
                return Result<Dictionary<TrackRoad, string>>.Fail(GetError(err));
            }
            else
            {
                var dic = new Dictionary<TrackRoad, string>();
                dic[TrackRoad.Trace2] = text;
                return Result<Dictionary<TrackRoad, string>>.Success(dic);
            }            
        }

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
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
                return Result.Fail("打印机连接失败");
            }
            graphics.InitGraphics(Encoding.GetEncoding("GB2312").GetBytes(deviceName), out errValue);

            //if (printInfoList != null)
            foreach (var printInfo in printInfoList)
            {
                var font = Encoding.GetEncoding("GB2312").GetBytes(printInfo.Font);
                var text = Encoding.GetEncoding("GB2312").GetBytes(printInfo.Text);
                graphics.DrawText(printInfo.X, printInfo.Y, text, font, printInfo.FontSize,
                    (int)printInfo.FontStyle, 0x000000, out errValue);
            }
            //if (printCodeList != null)
            foreach (var printCode in printCodeList)
            {
                var code = Encoding.GetEncoding("GB2312").GetBytes(printCode.BarCodeData);
                graphics.DrawBarcode(printCode.X, printCode.Y, printCode.Rotation, printCode.BarCodeType,
                    printCode.WidthRatio, printCode.Multiplier, printCode.Height, printCode.TextUnder, code,
                    out errValue);
            }

            ret = graphics.PrintGraphics(out errValue);
            graphics.CloseGraphics(out errValue);
            if (ret != 1)
            {
                return Result.Fail("打印机连接失败");
            }

            return Result.Success();
        }

        public Result MoveCard(CardPosF6 destPos, string reason = null)
        {
            int err;
            var ret = UnSafeMethods.ZBRPRNMovePrintReady(_handle, _prnType, out err);
            if (ret != 1)
            {
                Logger.Device.Error($"[发卡器{DeviceId}]移动卡位置异常，Handle:{_handle} 返回值:{ret}");
                return Result.Fail("发卡器移动卡位置失败");
            }
            ret = UnSafeMethods.ZBRPRNMoveCard(_handle, _prnType, 2300, out err);
            if (ret != 1)
            {
                Logger.Device.Error($"[发卡器{DeviceId}]移动卡位置异常，Handle:{_handle} 返回值:{ret}");
                return Result.Fail("发卡器移动卡位置失败");
            }
            return Result.Success();
        }

        public Result<Dictionary<TrackRoad, string>> ReadTrack(TrackRoad road)
        {
            throw new NotImplementedException();
        }
       
        private static readonly Dictionary<int, string> errorDictionary = new Dictionary<int, string>
        {
            {-1, "打印机机械故障"},
            {1, "色带损坏"},
            {2, "温度过高"},
            {3, "机械故障"},
            {4, "卡槽内无卡"},
            {5, "卡在写磁模块"},
            {6, "卡未在写磁模块"},
            {7, "打印头被打开"},
            {8, "色带已经用完"},
            {9, "移除色带"},
            {10, "传入的参数错误"},
            {11, "无效的坐标"},
            {12, "未知条码"},
            {13, "未知文本"},
            {14, "命令错误"},
            {20, "条码数据语法错误"},
            {21, "文本数据语法错误"},
            {22, "图形数据语法错误"},
            {30, "图像初始化错误"},
            {31, "超出图像最大宽度"},
            {32, "超出图像最大高度"},
            {33, "图像数据验证失败"},
            {34, "数据传输超时"},
            {35, "请检查色带"},
            {40, "磁条数据无效"},
            {41, "写磁条数据错误"},
            {42, "读磁条数据错误"},
            {43, "磁头故障"},
            {44, "磁头未响应"},
            {45, "未安装磁头或卡卡住了"},
            {47, "翻转错误"},
            {48, "顶盖被打开"},
            {49, "磁条数据编码错误"},
            {50, "磁头错误"},
            {51, "空白磁条"},
            {52, "光带错误"},
            {53, "无法访问"},
            {54, "序列错误"},
            {55, "PROX错误"},
            {56, "接触式数据错误"},
            {57, "PROX数据错误"},
            {60, "不支持该打印机"},
            {61, "无法获得打印机句柄"},
            {62, "无法获得打印机驱动"},
            {63, "无效的参数"},
            {64, "打印机正忙"},
            {65, "无效的打印机句柄"},
            {66, "关闭句柄错误"},
            {67, "通信错误"},
            {68, "缓存溢出"},
            {69, "读取数据错误"},
            {70, "写入数据错误"},
            {71, "装载动态库错误"},
            {72, "结构体对齐错误"},
            {73, "获取设备环境错误"},
            {74, "打印缓存错误"},
            {75, "内存不足"},
            {76, "磁盘空间不足"},
            {77, "操作被用户取消"},
            {78, "程序终止"},
            {79, "创建文件错误"},
            {80, "写入文件错误"},
            {81, "读取文件错误"},
            {82, "无效的媒体数据"},
            {83, "内存分配错误"},
            {255, "未知错误"}
        };
        private static string GetError(int i)
        {
            var message = "";
            errorDictionary.TryGetValue(i, out message);
            return message;
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