using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.Devices.UnionPay
{
    public enum Key : byte
    {
        未知 = 0x00,
        超时 = 0x02,
        退格 = 0x08,
        按键 = 0x2A,
        取消 = 0x1B,
        确认 = 0x0D,
        清空 = 0x3C,
        无输入 = 0xFF
    }
    public enum TransType
    {
        消费 = 00,
        撤销 = 01,
        退货 = 02,
        查余 = 03,
        结算 = 04,
        签到 = 05,
        冲正 = 06,
        预授权 = 08,
        预授权撤销 = 09,
        预授权完成 = 10
    }

	public class KeyText
    {
        public Key Key { get; set; }
        public string KeyContent { get; set; }
    }
    public interface IMisposUnionService : IService
    {
        bool IsConnected { get; set; }
        bool IsBusy { get; set; }
        Result Initialize(Business businessType, string misposdllPath, BanCardMediaType bankMediaType);
        Result SetReq(TransType transType, decimal totalMoneySencods);
        Result<string> ReadCard(BanCardMediaType bankMediaType);
        Result StartKeyboard(Action<KeyText> keyAction);
        Result<TransResDto> DoSale(decimal totalMoneySencods);
        Result<TransResDto> Refund(string reason);
        Result DisConnect(string reason);
        Result UnInitialize(string reason);
    }

    public class MisposUnionService : IMisposUnionService
    {
        // private bool _isConnected;

        public string ServiceName => "MisPos";

        protected bool _hasEject = false;

        private string _path = "";

        public bool IsConnected { get;  set; }
        public bool IsBusy { get;  set; }
        public virtual Result Initialize(Business businessType, string misposdllPath, BanCardMediaType bankMediaType)
        {
            IsBusy = true;
            try
            {
                _path = Path.IsPathRooted(misposdllPath) ? misposdllPath : Path.GetFullPath(misposdllPath);
                _libraryHandle = UnSafeMethods.LoadLibrary(_path);

                if (_libraryHandle == IntPtr.Zero)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                _Init = LoadExternalFunction<IntIntDelegate>("UMS_Init");
                if (bankMediaType.HasFlag(BanCardMediaType.闪付))
                {
                    _SetReq = LoadExternalFunction<IntStringDelegate>("UMS_SetReq");
                }
                _EnterCard = LoadExternalFunction<IntDelegate>("UMS_EnterCard");
                _CheckCard = LoadExternalFunction<IntOutByteDelegate>("UMS_CheckCard");
                _ReadCard = LoadExternalFunction<IntByteArrayDelegate>("UMS_ReadCard");
                _EjectCard = LoadExternalFunction<IntDelegate>("UMS_EjectCard");
                _CardClose = LoadExternalFunction<IntDelegate>("UMS_CardClose");
                _CardSwallow = LoadExternalFunction<IntDelegate>("UMS_CardSwallow");
                _StartPin = LoadExternalFunction<IntDelegate>("UMS_StartPin");
                _GetOnePass = LoadExternalFunction<IntOutByteDelegate>("UMS_GetOnePass");
                _GetPin = LoadExternalFunction<IntDelegate>("UMS_GetPin");
                _TransCard = LoadExternalFunction<IntStringByteArrayDelegate>("UMS_TransCard");
                var ret = UMS_Init(1);
                if (ret != 0)
                {
                    var error = nameof(UMS_Init).GetErrorMsgDetail(ret);
                    Logger.POS.Error($"[{ServiceName}] 初始化失败，详情:{error}");
                    return Result.Fail("初始化银联失败," + error);
                }
                IsConnected = true;
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"MisPos{ServiceName} 初始化异常，详情:{ex}\r\n{ex.StackTrace}");
                return Result.Fail("初始化银联失败");
            }
            finally
            {
                IsBusy = false;
            }

        }

        public virtual Result SetReq(TransType transType, decimal totalMoneySencods)
        {
            IsBusy = true;
            try
            {
                var req = new TransReq()
                {
                    Amount = totalMoneySencods,
                    TransType = transType
                };
                Logger.POS.Info($"[{ServiceName}]开始设置TransReq:{req}");
                var ret = UMS_SetReq(req.ToString());
                Logger.POS.Info($"[{ServiceName}]设置TransReq结果:{ret}");
                if (ret != 0)
                {
                    var error = nameof(UMS_SetReq).GetErrorMsgDetail(ret);
                    return Result.Fail(error);
                }
                else
                {
                    return Result.Success();
                }

            }
            catch (Exception ex)
            {
                Logger.POS.Info($"[MisPos]设置TransReq异常:{ex.Message} {ex.StackTrace}");
                return Result.Fail(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public virtual Result<string> ReadCard(BanCardMediaType bankMediaType)
        {
            IsBusy = true;
            string error;
            var ret = UMS_EnterCard();
            if (ret != 0)
            {
                error = nameof(UMS_EnterCard).GetErrorMsgDetail(ret);
                IsBusy = false;
                return Result<string>.Fail(error);
            }
            while (IsConnected)
            {
                if (bankMediaType != BanCardMediaType.闪付)//非接不能调用checkCard，否则非接读卡读不到
                {
                    byte bState = 0;
                    ret = UMS_CheckCard(out bState);

                    if (ret != 0)
                    {
                        error = nameof(UMS_CheckCard).GetErrorMsgDetail(ret);
                        IsBusy = false;
                        return Result<string>.Fail(error);
                    }
                    /*
                   未知 = 0,
                   卡在插卡器卡口位置 = 0x34,
                   未检测到卡 = 0x35,
                   插卡器内部有卡 = 0x37,
                   卡在外置非接感应器上 = 0x38
                   卡在非接联机感应器上 = 0x39
                    */

                    _hasEject = false;
                    if (bState != 0x37)
                    {
                        Thread.Sleep(500);
                        IsBusy = false;
                        continue;
                    }
                }
                if (!IsConnected)
                {
                    continue;
                }
                var buffer = new byte[32];
                ret = UMS_ReadCard(buffer);
                if (ret < 0)
                {
                    error = nameof(UMS_ReadCard).GetErrorMsgDetail(ret);
                    if (bankMediaType == BanCardMediaType.闪付)
                    {
                        Thread.Sleep(500);
                        if (error?.EndsWith("-999") ?? false)
                        {
                            IsBusy = false;
                            return Result<string>.Fail("该卡为芯片卡，请插入读卡器");
                        }
                        IsBusy = false;
                        continue;
                    }

                    IsBusy = false;
                    return Result<string>.Fail(error);
                }
                IsBusy = false;
                return Result<string>.Success(Encoding.ASCII.GetString(buffer));
            }
            IsBusy = false;
            return Result<string>.Fail("取消操作");
        }

        public virtual Result StartKeyboard(Action<KeyText> keyAction)
        {
            if (!IsConnected)
            {
                return Result.Fail("取消操作");
            }
            var error = string.Empty;
            var ret = UMS_StartPin();
            if (ret != 0)
            {
                error = nameof(UMS_StartPin).GetErrorMsgDetail(ret);
                return Result.Fail(error);
            }
            var keycontent = String.Empty;
            while (IsConnected)
            {
                error = string.Empty;
                byte keyByte;
                /*             
               未知 = 0x00,
               超时 = 0x02,
               退格 = 0x08,
               按键 = 0x2A,
               取消 = 0x1B,
               确认 = 0x0D,
               无输入 = 0xFF
                */
                ret = UMS_GetOnePass(out keyByte);
                var key = (Key)keyByte;
                if (ret != 0)
                {
                    error = nameof(UMS_GetOnePass).GetErrorMsgDetail(ret);
                    return Result.Fail(error);
                }
                switch (key)
                {

                    case Key.退格:
                        if (keycontent.Length > 0)
                            keycontent = keycontent.Substring(0, keycontent.Length - 1);
                        break;
                    case Key.按键:
                        keycontent += "*";
                        break;
                    case Key.超时:
                        return Result.Fail("键盘输入超时");
                    case Key.取消:
                        return Result.Fail("键盘输入取消");
                    case Key.确认:
                    case Key.未知:
                    case Key.无输入:
                        break;
                    default:
                        Logger.POS.Error($"MisPos{ServiceName} 键盘输入，详情:获取到未知键值{key}");
                        break;
                }
                var ktext = new KeyText { Key = key, KeyContent = keycontent };
                keyAction?.Invoke(ktext);
                if (keycontent.Length == 6 || key == Key.确认)
                {
                    return Result.Success();
                }
                Thread.Sleep(200);
            }
            return Result.Fail("取消键盘操作");
        }

        public virtual Result<TransResDto> DoSale(decimal totalMoneySencods)
        {
            if (!IsConnected)
            {
                return Result<TransResDto>.Fail("取消操作");
            }
            Thread.Sleep(500);
            var ret = UMS_GetPin();
            if (ret != 0)
            {
                var error = nameof(UMS_GetPin).GetErrorMsgDetail(ret);
                return Result<TransResDto>.Fail(error);
            }
            var req = new TransReq()
            {
                Amount = totalMoneySencods,
                TransType = TransType.消费
            };
            Logger.POS.Info($"[Mispos消费]入参:{req} 入参:{req.ToJsonString()}");
           var buffer = new byte[2048];
            ret = UMS_TransCard(req.ToString(), buffer);
            var res = Encoding.Default.GetString(buffer);
            Logger.POS.Info($"Mispos消费 金额:{totalMoneySencods}\r\n入参:{req.ToJsonString()}\r\n出参:{res.ToJsonString()}");
            var pret = Parse(buffer);
            if (pret.RespCode == "00")
            {
                pret.Receipt = GetPrint();
                return Result<TransResDto>.Success(pret);
            }
            else
            {
                return Result<TransResDto>.Fail("由于银联服务异常，扣费失败。异常原因:" + pret.RespInfo);
            }
        }

        public virtual Result<TransResDto> Refund(string reason)
        {
            if (!IsConnected)
            {
                return Result<TransResDto>.Fail("取消操作");
            }
            var req = new TransReq()
            {
                TransType = TransType.冲正
            };
            var buffer = new byte[2048];
            var ret = UMS_TransCard(req.ToString(), buffer);
            var res = Encoding.Default.GetString(buffer);
            Logger.POS.Info($"Mispos冲正 原因:{reason}\r\n入参:{req.ToJsonString()}\r\n出参:{res.ToJsonString()}");
            var pret = Parse(buffer);
            if (pret.RespCode == "00")
            {
                pret.Receipt = GetPrint();
                return Result<TransResDto>.Success(pret);

            }
            else
            {
                return Result<TransResDto>.Fail("由于银联服务异常，扣费撤消失败。异常原因:" + pret.RespInfo);
            }
        }

        public virtual Result DisConnect(string reason)
        {
            if (IsConnected)
            {
                IsConnected = false;
            }
            return Result.Success();
        }

        public virtual Result UnInitialize(string reason)
        {
            DisConnect(reason);
            if (!_hasEject)
            {
                var ret = UMS_EjectCard();
                if (ret != 0)
                {
                    var error = nameof(UMS_EjectCard).GetErrorMsgDetail(ret);
                    return Result.Fail(error);

                }
                ret = UMS_CardClose();
                _hasEject = true;
            }
            _EnterCard =null;
            _CheckCard = null;
            _ReadCard = null;
            _EjectCard = null;
            _CardClose = null;
            _CardSwallow = null;
            _StartPin = null;
            _GetOnePass = null;
            _GetPin = null;
            _TransCard = null;
            //仅卸载主依赖库
            var list = new List<string>
            {
                "umsapiInter.dll",
                "EmvModule.dll",
                "giftcard.dll",
                "pack.dll",
                "umscup.dll",
                "umsdevice.dll",
                "umspub.dll",
                "ums-ssl-lib.dll",
                "umstools.dll",
                "UnionPayCard.dll",
                "EPP_API.dll",
                "ZTPinpad.dll",
                "ZTCom.dll",
                "ZTUsb.dll",
                "RFReader.dll",
                "msvcr100d.dll",
                "umsapi.dll",
            };

            foreach (var module in list)
            {
                UnSafeMethods.Unload(module);
            }

            return Result.Success();
        }


        protected  T LoadExternalFunction<T>(string functionName) where T : class
        {
            var functionPointer = UnSafeMethods.GetProcAddress(_libraryHandle, functionName);

            if (functionPointer == IntPtr.Zero)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            return Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(T)) as T;
        }

        #region Fields & Delegates

        private IntPtr _libraryHandle;

        //private readonly string _path;

       protected IntIntDelegate _Init;
       protected IntStringDelegate _SetReq;
       protected IntDelegate _EnterCard;
       protected IntOutByteDelegate _CheckCard;
       protected IntByteArrayDelegate _ReadCard;
       protected IntDelegate _EjectCard;
       protected IntDelegate _CardClose;
       protected IntDelegate _CardSwallow;
       protected IntDelegate _StartPin;
       protected IntOutByteDelegate _GetOnePass;
       protected IntDelegate _GetPin;
       protected IntStringByteArrayDelegate _TransCard;
       protected delegate int IntDelegate();
       protected delegate int IntIntDelegate(int i);
       protected delegate int IntStringDelegate(string s);
       protected delegate int IntByteArrayDelegate(byte[] data);
       protected delegate int IntStringByteArrayDelegate(string s, byte[] data);
       protected delegate int IntOutByteDelegate(out byte state);

        #endregion Fields & Delegates

        #region Dll Import

        protected int UMS_Init(int appType)
        {
            try
            {
                if (_Init==null)
                {
                    return -1;
                }
                return _Init(appType);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_Init)}异常，原因:{ex.Message}");
                return -1;

            }

        }

        protected int UMS_SetReq(string strReq)
        {
            try
            {
                if (_SetReq == null)
                {
                    return -1;
                }
                return _SetReq(strReq);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_SetReq)}异常，原因:{ex.Message}");
                return -1;

            }

        }

        protected int UMS_EnterCard()
        {
            try
            {
                if (_EnterCard == null)
                {
                    return -1;
                }
                return _EnterCard();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_EnterCard)}异常，原因:{ex.Message}");
                return -1;

            }

        }

        protected int UMS_CheckCard(out byte state)
        {
            try
            {
                state = 0;
                if (_CheckCard == null)
                {
                    return -1;
                }
                return _CheckCard(out state);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_CheckCard)}异常，原因:{ex.Message}");
                state=Byte.MinValue;
                return -1;

            }

        }

        protected int UMS_ReadCard(byte[] cpData)
        {
            try
            {
                if (_ReadCard == null)
                {
                    return -1;
                }
                return _ReadCard(cpData);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_ReadCard)}异常，原因:{ex.Message}");
                return -1;

            }
            
        }

        protected int UMS_EjectCard()
        {
            try
            {
                if (_EjectCard == null)
                {
                    return -1;
                }
                return _EjectCard?.Invoke() ?? 0;
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_EjectCard)}异常，原因:{ex.Message}");
                return -1;

            }

        }

        protected int UMS_CardClose()
        {
           

            try
            {
                if (_CardClose == null)
                {
                    return -1;
                }
                return _CardClose?.Invoke() ?? 0;
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_CardClose)}异常，原因:{ex.Message}");
                return -1;

            }
        }

       protected int UMS_CardSwallow()
        {
            
            try
            {
                if (_CardSwallow == null)
                {
                    return -1;
                }
                return _CardSwallow();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_CardSwallow)}异常，原因:{ex.Message}");
                return -1;

            }
        }
       
       protected int UMS_StartPin()
        {
           
            try
            {
                if (_StartPin == null)
                {
                    return -1;
                }
                return _StartPin();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_StartPin)}异常，原因:{ex.Message}");
                return -1;

            }
        }
       
       protected int UMS_GetOnePass(out byte key)
        {
           
            try
            {
                key = 0;
                if (_GetOnePass == null)
                {
                    return -1;
                }
                return _GetOnePass(out key);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_GetOnePass)}异常，原因:{ex.Message}");
                key = 0x00;
                return -1;

            }
        }
       
       protected int UMS_GetPin()
        {
           
            try
            {
                if (_GetPin == null)
                {
                    return -1;
                }
                return _GetPin();
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_GetPin)}异常，原因:{ex.Message}");
                return -1;

            }
        }
       
       protected int UMS_TransCard(string strReq, byte[] strResp)
        {
            try
            {
                if (_TransCard == null)
                {
                    return -1;
                }
                return _TransCard(strReq, strResp);
            }
            catch (Exception ex)
            {
                Logger.POS.Error($"[{ServiceName}]{nameof(UMS_TransCard)}异常，原因:{ex.Message}");
                return -1;

            }
        }

        #endregion Dll Import
        public class TransReq
        {
            public string CounterId { get; set; }
            public string OperId { get; set; }
            public TransType TransType { get; set; } = TransType.消费;
            public decimal Amount { get; set; }
            public string OldTrace { get; set; }
            public string OldDate { get; set; }
            public string OldRef { get; set; }
            public string OldAuth { get; set; }
            public string OldBatch { get; set; }
            public string Memo { get; set; }
            public string Lrc { get; set; }

            protected string PadLeft(string s, int length)
            {
                if (string.IsNullOrEmpty(s))
                    return string.Empty.PadLeft(length, ' ');
                return s.PadLeft(length, ' ');
            }

            public override string ToString()
            {
                var sb = new StringBuilder(2048);
                sb.Append(PadLeft(CounterId, 8));
                sb.Append(PadLeft(OperId, 8));
                sb.Append(((int)TransType).ToString("D2"));
                sb.Append(((int)Amount).ToString("D12"));
                sb.Append(PadLeft(OldTrace, 6));
                sb.Append(PadLeft(OldDate, 8));
                sb.Append(PadLeft(OldRef, 12));
                sb.Append(PadLeft(OldAuth, 6));
                sb.Append(PadLeft(OldBatch, 6));
                sb.Append(PadLeft(Memo, 1024));
                sb.Append(PadLeft(Lrc, 3));
                return sb.ToString();
            }
        }

        protected static TransResDto Parse(byte[] data)
        {
            var p = 0;
            return new TransResDto
            {
                RespCode = GetString(data, ref p, 2),
                RespInfo = GetString(data, ref p, 40),
                CardNo = GetString(data, ref p, 20),
                Amount = GetString(data, ref p, 12),
                Trace = GetString(data, ref p, 6),
                Batch = GetString(data, ref p, 6),
                TransDate =DateTimeCore.Now.Year+GetString(data, ref p, 4),
                TransTime = GetString(data, ref p, 6),
                Ref = GetString(data, ref p, 12),
                Auth = GetString(data, ref p, 6),
                MId = GetString(data, ref p, 15),
                TId = GetString(data, ref p, 8),
                Memo = GetString(data, ref p, 1024),
                Lrc = GetString(data, ref p, 3)
            };
        }
        protected static string GetString(byte[] data, ref int start, int length)
        {
            var s = Encoding.Default.GetString(data, start, length);
            start += length;
            return s;
        }
        public string GetPrint()
        {
            var text = "";
            try
            {
                using (
                    var sr = new StreamReader(Path.Combine(Path.GetDirectoryName(_path), "cup", "receipt.txt"),
                        Encoding.GetEncoding("GB2312")))
                    text = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.POS.Error("取打印文本失败:" + ex.Message);
            }
            return text;
        }
    }

    public static class Dics
    {
        #region Error Dictionary

        private static readonly Dictionary<string, Dictionary<int, string>> ErrorDictionary = new Dictionary<string, Dictionary<int, string>>
        {
            {
                "UMS_Init", new Dictionary<int, string>
                {
                    {-101, "appType非法"}, {-102, "加载主配置main.ini失败"}, {-103, "初始化密码键盘失败"}, {-104, "银联卡模块初始化失败"}, {-105, "增值模块初始化失败"}, {-106, "预付卡模块初始化失败"}, {-107, "暂不支持的appType"}, {-108, "向EMV内核下载IC卡参数失败"}
                }
            },
            {
                "UMS_EnterCard", new Dictionary<int, string>
                {
                    {0, "插卡器启动成功，外挂非接模块无法打开"}, {1, "外挂非接模块启动成功，插卡器无法打开"}, {2, "外挂非接模块、插卡器均启动成功"}
                }
            },
            { "UMS_CheckCard", new Dictionary<int, string>()},
            { "UMS_ReadCard", new Dictionary<int, string>
                {
                    {-999, "此卡为芯片卡，请插入读卡器"}
                }
            },
            { "UMS_EjectCard", new Dictionary<int, string>()},
            { "UMS_CardSwallow", new Dictionary<int, string>()},
            { "UMS_StartPin", new Dictionary<int, string>()},
            { "UMS_GetOnePass", new Dictionary<int, string>()},
            { "UMS_GetPin", new Dictionary<int, string>()},
            { "UMS_SetReq", new Dictionary<int, string>()}
        };

        #endregion Error Dictionary

        #region Res Dictionary

        public static Dictionary<string, string> ResDictionary = new Dictionary<string, string>
        {
            {"00", "交易成功"}, {"01", "查发卡方"}, {"02", "查发卡方的特殊条件"}, {"03", "无效商户"}, {"04", "没收卡"}, {"05", "不予承兑"}, {"06", "出错"}, {"07", "特殊条件下没收卡"}, {"09", "请求正在处理中"}, {"12", "无效交易"}, {"13", "无效金额"}, {"14", "无效卡号"}, {"15", "无此发卡方"}, {"19", "请重做交易"}, {"20", "无效应答"}, {"21", "不作任何处理"}, {"22", "怀疑操作有误"}, {"23", "不可接受的交易费"}, {"25", "查不到原交易记录"}, {"30", "格式错误"}, {"31", "交换中心不支持的银行"}, {"33", "过期的卡（没收卡）"}, {"34", "有作弊嫌疑（没收卡）"}, {"35", "与安全保密部门联系（没收卡）"}, {"36", "受限制的卡（没收卡）"}, {"37", "呼受理方安全保密部门（没收卡）"}, {"38", "超过允许的密码次数"}, {"39", "无此信用卡帐户"}, {"40", "请求的功能尚不支持"}, {"41", "挂失卡（没收卡）"}, {"42", "无此帐户"}, {"43", "被窃卡（没收卡）"}, {"44", "无此投资帐户"}, {"45", "请使用IC卡"}, {"46", "ISO保留使用"}, {"47", "ISO保留使用"}, {"48", "ISO保留使用"}, {"49", "ISO保留使用"}, {"50", "ISO保留使用"}, {"51", "卡内余额不足"}, {"52", "无此支票帐户"}, {"53", "无此储蓄卡帐户"}, {"54", "过期的卡"}, {"55", "密码错误"}, {"56", "无此卡记录"}, {"57", "不允许持卡人进行的交易"}, {"58", "不支持的交易,请先签到"}, {"59", "有作弊嫌疑"}, {"60", "受卡方与安全保密部门联系"}, {"61", "超出取款金额限制"}, {"62", "受限制的卡"}, {"63", "违反安全保密规定"}, {"64", "原始金额不正确"}, {"65", "超出取款次数限制"}, {"66", "受卡方呼受理方安全保密部门"}, {"67", "捕捉（没收卡）"}, {"68", "收到的回答太迟"}, {"69", "ISO保留使用"}, {"70", "ISO保留使用"}, {"71", "ISO保留使用"}, {"72", "ISO保留使用"}, {"73", "ISO保留使用"}, {"74", "ISO保留使用"}, {"75", "密码输入次数超限"}, {"77", "POS批次与网络中心不一致"}, {"78", "网络中心需要向POS终端下载数据"}, {"79", "POS终端上传的脱机数据对帐不平"}, {"89", "私有保留使用"}, {"90", "日期切换正在处理"}, {"91", "发卡方系统故障"}, {"92", "金融机构无法达到"}, {"93", "交易违法、不能完成"}, {"94", "重复交易"}, {"95", "调节控制错"}, {"96", "系统故障请重试"}, {"97", "无此终端"}, {"98", "交换中心收不到发卡方应答"}, {"99", "PIN格式错请重新签到"}, {"A0", "校验错，请重新签到"}, {"A1", "二磁道长度不符合"}, {"A2", "三磁道长度不符合"}, {"A3", "校验密钥错"}, {"A4", "未签到，请重新签到"}, {"A5", "用户磁道数据异常"}, {"Z0", "未收到应答"}, {"Z1", "交易超时，请重试"}, {"Z2", "因数据包错误引发冲正"}, {"Z3", "因交易类型错误引发冲正"}, {"Z4", "因原交易不匹配引发冲正"}, {"Z5", "因签购单生成失败冲正,请重试"}, {"Y1", "二磁道信息错误"}, {"Y2", "冲正或接口转换交易类型错"}, {"Y3", "init配置文件失败"}, {"Y4", "未找到原交易"}, {"Y5", "与密码键盘通讯失败"}, {"Y6", "报文错误"}, {"Y7", "冲正文件错误，操作系统异常"}, {"Y8", "通讯链路异常，请检查连接"}, {"Y9", "结算文件生成错误"}, {"X1", "交易类型错误"}, {"X2", "接口文件格式错"}, {"X3", "初始化输出接口文件错"}, {"X4", "已结算"}, {"X5", "该交易已经撤消"}, {"X6", "原交易不是消费交易"}, {"X7", "交易合计错误"}, {"ZA", "校验错，请重新签到"}, {"ZB", "系统库错误"}, {"FB", "帐单已经缴纳"}, {"I0", "请求数据格式错"}, {"I1", "请求数据字符非法"}, {"I2", "不支持的交易类型"}, {"P0", "打包配置文件错"}, {"P2", "打包错误"}, {"P3", "解包错误"}, {"PF", "域数据非法"}, {"R0", "写冲正文件失败"}, {"R1", "删除冲正文件失败"}, {"R2", "读冲正文件失败"}, {"V0", "未找到原交易"}, {"T0", "记录流水失败"}, {"T1", "更新流水失败"}, {"1A", "初始化报文结构失败"}, {"1B", "赋值发送域失败"}, {"1C", "组包失败"}, {"1D", "加MAC失败"}, {"1E", "组织返回数据异常"}, {"1F", "解包失败"}, {"1G", "取接收报文域失败"}, {"1H", "校验MAC失败"}, {"1I", "写密钥失败"}, {"1J", "组织48域数据失败"}, {"1K", "未初始化"}, {"1L", "输入参数非法"}, {"1M", "磁道数据异常"}, {"1N", "暂时不支持的应用"}, {"1O", "磁道信息非法"}, {"1P", "卡号异常"}, {"1Q", "PAN异常"}, {"1R", "磁道信息异常"}, {"1S", "磁道信息异常"}, {"1T", "用户输入密码异常"}, {"1U", "缴费确认失败"}, {"1V", "缴费确认请求数据组织失败"}, {"1W", "交易类型错误"}, {"1X", "漏掉了必须要做的交易"}, {"1Y", "金额错误"}, {"1Z", "流水号不匹配"}, {"2A", "写冲正文件失败"}, {"2B", "终端号不匹配"}, {"2C", "读冲正文件失败"}, {"2D", "商户号不匹配"}, {"2E", "删除冲正文件失败"}, {"2F", "公钥查询获得数据异常"}, {"2G", "公参查询获得数据异常"}, {"2H", "向磁盘写公钥长度失败"}, {"2I", "向磁盘写公钥数据失败"}, {"2J", "向磁盘写公参长度失败"}, {"2K", "向磁盘写公参数据失败"}, {"2L", "联机前的EMV处理失败"}, {"2M", "芯片卡上电失败"}, {"2N", "射频卡上电失败"}, {"2O", "内核联机后处理失败"}, {"2P", "从EMV内核获取数据失败"}, {"2Q", "写脚本信息文件失败"}, {"2R", "读脚本信息失败"}, {"2S", "联机后EMV内核判定交易拒绝"}, {"2T", "非接模块消费交易失败"}, {"2U", "没有冲正文件"}, {"2V", "读冲正文件失败"}, {"2W", "写脱机交易流水失败"}, {"2X", "获取脱机流水文件大小失败"}, {"2Y", "脱机流水大小异常"}, {"2Z", "无法打开脱机流水文件"}, {"3A", "读取脱机流水文件失败"}, {"3B", "删除一笔脱机流水失败"}, {"3C", "该交易不支持非接"}, {"3D", "开启键盘失败"}, {"3E", "输入密码超过时间限制"}, {"3F", "按键超时"}, {"3G", "用户取消"}, {"3H", "获取密文失败"}
        };

        #endregion Res Dictionary

        public static string GetErrorMsgDetail(this string functName,int ret)
        {
            if (ErrorDictionary.ContainsKey(functName))
            {
                var dic = ErrorDictionary[functName];
                if (dic.ContainsKey(ret))
                {
                    return dic[ret];
                }
                else if(ret==-1)
                {
                    return "接口已经被释放，请重新操作";
                }
                return "未知错误码:" + ret;
            }
            return $"未知错误调用:{functName}" ;
        }
    }
}
