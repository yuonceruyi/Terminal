using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;

namespace YuanTu.Devices.MKeyBoard
{
    
    public class ZtMKeyboard : IMKeyboard
    {
        private bool _isConnected;
        private int _port;
        private int _baud;
        private short _ret;
        private static short MainKeyIndex = 0x00;
        private static short PinKeyIndex = 0x02;
        private static short MacKeyIndex = 0x00;
        private  bool _stopKeypress;
        public  Action<KeyText> keyPressDelegate;
        private string _passBin;

        private short iWorkKey = 0x00;

        public int MasterKeyId { get; set; }
        public int PinKeyId { get; set; }
        public int MacKeyId { get; set; }

        private short Ret
        {
            get { return _ret; }
            set
            {
                _ret = value;
                if (value != 0) { 
                    _error = GetError(value);
                    throw new Exception(_error);
                }
            }
        }

        private string _error;
        private readonly IConfigurationManager _configurationManager;

        public ZtMKeyboard(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
            MainKeyIndex = (short)_configurationManager.GetValueInt("Pos:MainKeyIndex", 0x00);
            PinKeyIndex = (short)_configurationManager.GetValueInt("Pos:PinKeyIndex", 0x02);
            MacKeyIndex = (short)_configurationManager.GetValueInt("Pos:MacKeyIndex", 0x00);
        }

        #region Implementation of IDevice

        /// <summary>
        ///     硬件名称
        /// </summary>
        public string DeviceName { get; } = "ZTKeyboard";

        /// <summary>
        ///     硬件唯一标识(整个系统中唯一)
        /// </summary>
        public string DeviceId => DeviceName;

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
            if (_isConnected)
            {
                return Result.Success();
            }

            _port = _configurationManager.GetValueInt("MKeyboard:Port");
            _baud = _configurationManager.GetValueInt("MKeyboard:Baud");
            Ret = UnSafeMethods.ZT_EPP_OpenCom(_port, _baud);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_OpenCom 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘初始化失败 {_error}");
            }

            Ret = UnSafeMethods.ZT_EPP_PinInitialization(0);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinInitialization 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘初始化失败 {_error}");
            }

            var ver = new StringBuilder();
            var sn = new StringBuilder();
            var charge = new StringBuilder();
            Ret = UnSafeMethods.ZT_EPP_PinReadVersion(ver, sn, charge);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinReadVersion 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘初始化失败 {_error}");
            }
            _isConnected = true;
            return Result.Success();
        }

        /// <summary>
        ///     2.初始化
        /// </summary>
        /// <returns></returns>
        public Result Initialize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     3.反初始化
        /// </summary>
        /// <returns></returns>
        public Result UnInitialize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     4.断开连接
        /// </summary>
        /// <returns></returns>
        public Result DisConnect()
        {
            Ret = UnSafeMethods.ZT_EPP_CloseCom();
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_CloseCom 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘关闭失败 {_error}");
            }
            _isConnected = false;
            return Result.Success();
        }

        #endregion Implementation of IDevice

        private string GetError(int ret)
        {
            string message;
            return !ErrorDictionary.TryGetValue(ret, out message) ? $"未知错误" : message;
        }

        private static readonly Dictionary<int, string> ErrorDictionary = new Dictionary<int, string>
        {
            {0, "命令成功执行 "},
            {1, "命令参数错 "},
            {2, "打开串口错 "},
            {3, "关闭串口错 "},
            {4, "SAM卡解密错 "},
            {5, "SAM卡认证错 "},
            {6, "输入非法字符 "},
            {7, "发送数据错 "},
            {8, "超时,无数据返回 "},
            {9, "不支持此PIN加密模式"},
            {10, "发送到串口错 "},
            {11, "WaitCom Error "},
            {12, "读串口错"},
            {13, "设置超时错"},
            {14, "关闭串口错"},
            {15, "超时检测错"},
            {80, "自检时，COU错"},
            {81, "SRAM错"},
            {82, "键盘有短路错"},
            {83, "串口电平错"},
            {84, "CPU卡出错"},
            {85, "电池可能损坏"},
            {86, "主密钥失效"},
            {87, "杂项错"},
        };

        public Result LoadWorkKey(string pin, string pinchk, string mac, string macchk)
        {
            Ret = UnSafeMethods.ZT_EPP_SetDesPara(0, 0x30);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_SetDesPara 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }

            Ret = UnSafeMethods.ZT_EPP_ActivWorkPin(MainKeyIndex,(short)( MainKeyIndex * 4 + PinKeyIndex + 0x40));
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_ActivWorkPin 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            var res = new StringBuilder();
            Ret = UnSafeMethods.ZT_EPP_PinUnAdd((short)(pin.Length / 16), 0, "0000000000000000", res);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinUnAdd 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            var chkpin = new StringBuilder(32);
            Ret = UnSafeMethods.ZT_EPP_PinLoadWorkKey(2, MainKeyIndex, (short)(MainKeyIndex * 4 + PinKeyIndex + 0x40), pin, chkpin);

            Logger.Device.Info($"MAC:{mac}\n,macchk:{macchk}\n,pinchk:{pinchk}\n,pin:{pin}\n,MainKeyIndex:{MainKeyIndex}\n,PinKeyIndex:{PinKeyIndex}\n,chkpin:{chkpin}\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinLoadWorkKey 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }

            if (pinchk.Substring(0, 8) != chkpin.ToString())
            {
                Logger.Device.Debug($"金属键盘 比较结果 失败 pinchk：{pinchk} chkpin：{chkpin}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }

            Ret = UnSafeMethods.ZT_EPP_SetDesPara(0, 0x30);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_SetDesPara 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            Ret = UnSafeMethods.ZT_EPP_ActivWorkPin(MainKeyIndex, (short)(MainKeyIndex * 4 + MacKeyIndex + 0x40));
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_ActivWorkPin 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            res = new StringBuilder();
            Ret = UnSafeMethods.ZT_EPP_PinUnAdd((short)(mac.Length / 16), 0, "0000000000000000", res);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinUnAdd 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            var chkmac = new StringBuilder(32);
            Ret = UnSafeMethods.ZT_EPP_PinLoadWorkKey(2, MainKeyIndex, (short)(MainKeyIndex * 4 + MacKeyIndex + 0x40), mac, chkmac);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinLoadWorkKey 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            if (macchk.Substring(0, 8) != chkmac.ToString())
            {
                Logger.Device.Debug($"金属键盘 比较结果 失败 macchk：{macchk} chkmac：{chkmac}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }
            return Result.Success();
        }

        public Result<string> CalcMac(string text,KMode kMode,MacMode macMode)
        {
            Ret =UnSafeMethods.ZT_EPP_ActivWorkPin(MainKeyIndex, (short)(MainKeyIndex * 4 + MacKeyIndex + 0x40));
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_ActivWorkPin 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘计算mac失败 {_error}");
            }
            var res = new StringBuilder();
            Ret =UnSafeMethods.ZT_EPP_PinUnAdd((short)kMode, 0, "0000000000000000", res);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinUnAdd 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘计算mac失败 {_error}");
            }
            var mac = new StringBuilder();
       
           
            Ret =UnSafeMethods.ZT_EPP_PinCalMAC( (short)kMode, (short)macMode, text, mac);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 ZT_EPP_PinCalMAC 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘计算mac失败 {_error}");
            }
            return Result<string>.Success(mac.ToString());
        }

        public Result<string> BeforeAddPin(string cardNo = "000000000000")
        {
            Ret = UnSafeMethods.ZT_EPP_ActivWorkPin(MainKeyIndex, (short)(MainKeyIndex * 4 + PinKeyIndex + 0x40));
            Ret = UnSafeMethods.ZT_EPP_PinUnAdd(2, 0, "0000000000000000", new StringBuilder());
            Ret = UnSafeMethods.ZT_EPP_PinLoadCardNo(cardNo);
            Ret = UnSafeMethods.ZT_EPP_OpenKeyVoic(3);

            short timeout = 30;
            short maxLen = 6;
            short mode = 1;

            Ret = UnSafeMethods.ZT_EPP_PinStartAdd(maxLen, 1, mode, 0, timeout);
            var isSuccess = false;
            var startTime = GetSecondNow();
            var pin = new StringBuilder(20);
            var chPin = new StringBuilder(20);
            var count = 0;
            while (true)
            {
                if (_stopKeypress)
                {
                    UnSafeMethods.ZT_EPP_OpenKeyVoic(0);
                    Logger.Device.Debug("exit keyboard");
                    //keyPressDelegate("exit");
                    break;
                }
                if (GetSecondNow() - startTime > timeout)
                {
                    UnSafeMethods.ZT_EPP_OpenKeyVoic(0);
                    keyPressDelegate(new KeyText() { KeyLength = 0, KeyOrder = KeyEnum.Timeout });
                    break;
                }
                try
                {
                    Ret = UnSafeMethods.ZT_EPP_PinReportPressed(pin, 500);
                }
                catch (Exception ex)
                {
                    Logger.Device.Debug("read Exception;  ex :" + ex.Message);
                    continue;
                }
                if (0x0D == pin[0] && count > 0)
                {
                    chPin.Append(pin[0]);
                    Logger.Device.Debug("finish, password:" + chPin);
                    //keyPressDelegate("finish");

                    isSuccess = true;
                    break;
                }
                if (0x08 == pin[0])
                {
                    chPin.Clear();
                    count = 0;
                    keyPressDelegate(new KeyText() { KeyLength = 0, KeyOrder = KeyEnum.Clear });
                }
                if (0x1B == pin[0])
                {
                    Logger.Device.Debug("cancel input");
                    keyPressDelegate(new KeyText() { KeyLength = 0, KeyOrder = KeyEnum.Cancel });
                    break;
                }
                if (0x2A == pin[0])
                {
                    chPin.Append(pin[0]);
                    keyPressDelegate(new KeyText() { KeyLength = chPin.Length, KeyOrder = KeyEnum.Number });
                    count++;
                    if (count == 6)
                    {
                        isSuccess = true;
                        break;
                    }
                }
            }
            if (isSuccess)
            {
                _passBin = ReadBin();
                Logger.Device.Debug("set password ok, password=******");
                keyPressDelegate(new KeyText() { KeyLength = 6, KeyOrder = KeyEnum.Confirm });
            }
            return Result<string>.Success(_passBin);
        }


        private  string ReadBin()
        {
            Logger.Device.Debug("start read pin");

            var pinBlock = new StringBuilder();
            Ret =UnSafeMethods.ZT_EPP_PinReadPin(0, pinBlock);
            if (Ret == 0)
            {
               
                Logger.Device.Debug("read pin success,pin block=" + pinBlock);
            }
            else
            {
               
                Logger.Device.Debug("read pin error, code=" + Ret);
            }
            return pinBlock.ToString();
        }
        private static long GetSecondNow()
        {
            var ts = DateTimeCore.Now - new DateTime(1970, 1, 1);
            var startTime = (long)ts.TotalSeconds;
            return startTime;
        }

        public Result SetKeyAction(Action<KeyText> keyAction)
        {
            keyPressDelegate = keyAction;
            return Result.Success();
        }
    }
}