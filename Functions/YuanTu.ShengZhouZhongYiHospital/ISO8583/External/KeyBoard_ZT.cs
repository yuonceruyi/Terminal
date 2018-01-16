using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts;
using YuanTu.Core.Log;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583.External
{
    public class DelegateCollection
    {
        public delegate void KeyPressDelegate(string pin);

        public delegate void UpdateMoneyCount(int bill);
    }
    public static class KeyBoard_ZT
    {
        #region DLL Import

        private const string DllPathZtKeyboard = "External\\ZtEpp\\ZT_EPP_API.dll";
        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_OpenCom", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_OpenCom(short iPort, int lBaud);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_CloseCom", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_CloseCom();

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinInitialization", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinInitialization(short iInitMode);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinReadVersion", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinReadVersion(StringBuilder cpVersion, StringBuilder cpSN, StringBuilder cpRechang);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_SetDesPara", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_SetDesPara(short iPara, short iFCode);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinLoadMasterKey", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinLoadMasterKey(short iKMode, short iKeyNo, string
            lpKey, StringBuilder cpExChk);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinLoadWorkKey", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinLoadWorkKey(short iKMode, short iMKeyNo, short
            iKeyNo, string lpKey, StringBuilder cpExChk);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_ActivWorkPin", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_ActivWorkPin(short iMKeyNo, short iWKeyNo);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinLoadCardNo", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinLoadCardNo(string lpCardNo);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_OpenKeyVoic", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_OpenKeyVoic(short iValue);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinStartAdd", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinStartAdd(short iPinLen, short iDispMode, short iPINMode, short
            iPromMode, short iTimeOut);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinReportPressed", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinReportPressed(StringBuilder cpKey, short iTimeOut);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinReadPin", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinReadPin(short iKMode, StringBuilder cpPinBlock);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinCalMAC", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinCalMAC(short iKMode, short iMacMode, string lpValue,
            StringBuilder cpExValue);
        //public static extern short ZT_EPP_PinCalMAC(short iKMode, short iMacMode, byte[] lpValue,
        //    StringBuilder cpExValue);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinAdd", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinAdd(short iKMode, short iMode, string lpValue, StringBuilder cpExValue);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinUnAdd", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinUnAdd(short iKMode, short iMode, string lpValue, StringBuilder cpExValue);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_SetICType", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_SetICType(short iIC, short iICType);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_ICOnPower", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_ICOnPower(StringBuilder chOutData);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_ICDownPower", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_ICDownPower();

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_ICControl", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_ICControl(string lpValue, StringBuilder cpExValue);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_GetDLLVersion", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_GetDLLVersion(StringBuilder v1, StringBuilder v2);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_ReadICType()", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_ReadICType(short ic, StringBuilder type);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinGeneratecalMAC", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinGeneratecalMAC(StringBuilder v1, StringBuilder v2, StringBuilder v3);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_TerminalNum", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_TerminalNum(StringBuilder v1, StringBuilder v2);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_Des", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_Des(StringBuilder v1, StringBuilder v2, StringBuilder v3);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_DllSplitBcd", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_DllSplitBcd(StringBuilder v1, StringBuilder v2, short v3);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_Undes", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_Undes(string v1, string v2, StringBuilder v3);

        [DllImport(DllPathZtKeyboard, EntryPoint = "SZZT_DownloadKey", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short SZZT_DownloadKey(short nKeyNo, short nMKeyNo, short nKMode, string lpValue,
            StringBuilder lpExKCV);

        #endregion DLL Import

        public static int MainKeyIndex { get; set; } = 0x00;

        private static short mainKeyIndex => (short)MainKeyIndex;
        private static short pinKeyIndexWithMain => (short)(MainKeyIndex * 4 + pinKeyIndex + 0x40);
        private static short macKeyIndexWithMain => (short)(MainKeyIndex * 4 + macKeyIndex + 0x40);

        private const short pinKeyIndex = 0x00;
        private const short macKeyIndex = 0x01;
        private static int ret;

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

        public static short Port;
        public static int Baud;

        public static DelegateCollection.KeyPressDelegate keyPressDelegate;
        public static bool StopKeypress;

        public static string log = string.Empty;

        private static int Ret
        {
            set
            {
                ret = value > 0 ? value : -value;
                if (ret == 0)
                    return;

                string message;
                ErrorDictionary.TryGetValue(ret, out message);
                throw new Exception(message);
            }
            get { return ret; }
        }

        public static string passBin { get; private set; }

        public static bool InitKeyboard()
        {
            StopKeypress = false;
            try
            {
                Ret = ZT_EPP_OpenCom(Port, Baud);
                Logger.Device.Debug("open keyboard com=" + Port + ", rate=" + Baud);
                //if (ret != 0)
                //{
                //	 Logger.Device.Debug("open keyboard com erro,code=" + ret);
                //	Reporter.ReportState(MTUtils.MONITOR_PC_TYPE_KEY, MTUtils.MONITOR_LEVEL_FATAL, "初始化键盘失败，错误代码是:" + ret);
                //	return ret;
                //}

                Ret = ZT_EPP_PinInitialization(0);
                 Logger.Device.Debug("Initialize pin,code=" + ret);

                var ver = new StringBuilder();
                var sn = new StringBuilder();
                var charge = new StringBuilder();
                Ret = ZT_EPP_PinReadVersion(ver, sn, charge);
                log = "ver=" + ver + ",sn=" + sn + ",charge=" + charge;
                 Logger.Device.Debug("ver=" + ver + ",sn=" + sn + ",charge=" + charge);
                //if (ret != 0)
                //{
                //	Reporter.ReportState(MTUtils.MONITOR_PC_TYPE_KEY, MTUtils.MONITOR_LEVEL_FATAL, "初始化键盘失败，错误代码是:" + ret);
                //}
                Ret = ZT_EPP_SetDesPara(0, 0x30);
                Ret = ZT_EPP_SetDesPara(1, 0x30);
                Ret = ZT_EPP_SetDesPara(4, 0x10);
                Ret = ZT_EPP_SetDesPara(5, 0x05);
                return true;
            }
            catch (Exception ex)
            {
                 Logger.Device.Debug(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
            //if (ret != 0)
            //{
            //	Reporter.ReportState(MTUtils.MONITOR_PC_TYPE_KEY, MTUtils.MONITOR_LEVEL_FATAL, "初始化键盘失败，错误代码是:" + ret);
            //}
            //return ret;
        }

        public static void SetKeyboardDelegate(DelegateCollection.KeyPressDelegate delegateKey)
        {
            keyPressDelegate = delegateKey;
        }

        private static long GetSecondNow()
        {
            var ts = DateTimeCore.Now - new DateTime(1970, 1, 1);
            var startTime = (long)ts.TotalSeconds;
            return startTime;
        }

        public static string ReadBin()
        {
             Logger.Device.Debug("start read pin");

            var pinBlock = new StringBuilder();
            Ret = ZT_EPP_PinReadPin(2, pinBlock);
            if (ret == 0)
            {
                log = "读取密码pin:" + pinBlock;
                 Logger.Device.Debug("read pin success,pin block=" + pinBlock);
            }
            else
            {
                log = "读取密码错误，error:" + ret;
                 Logger.Device.Debug("read pin error, code=" + ret);
            }
            return pinBlock.ToString();
        }

        public static bool BeforeAddPin(string cardNo)
        {
            passBin = "";
            keyPressDelegate("workkey");

            Ret = ZT_EPP_ActivWorkPin(mainKeyIndex, pinKeyIndexWithMain);
            var res = new StringBuilder();
            Ret = ZT_EPP_PinUnAdd(2, 0, "0000000000000000", res);
            Ret = ZT_EPP_PinLoadCardNo(cardNo);
            Ret = ZT_EPP_OpenKeyVoic(3);
             Logger.Device.Debug("open key, code=" + ret);

             Logger.Device.Debug("active work pin, code=" + ret);
            short timeout = 30;
            short maxLen = 6;
            short mode = 1;

            Ret = ZT_EPP_PinStartAdd(maxLen, 1, mode, 0, timeout);
            bool isSuccess = false;
            long startTime = GetSecondNow();
            var pin = new StringBuilder(20);
            var chPin = new StringBuilder(20);
            int count = 0;
            while (true)
            {
                if (StopKeypress)
                {
                     Logger.Device.Debug("StopKeypress");
                    //ZT_EPP_OpenKeyVoic(0);
                    //keyPressDelegate("exit");
                    break;
                }
                if (GetSecondNow() - startTime > timeout)
                {
                    ZT_EPP_OpenKeyVoic(0);
                    keyPressDelegate("timeout");
                    break;
                }
                try
                {
                    Ret = ZT_EPP_PinReportPressed(pin, 500);
                }
                catch (Exception)
                {
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
                    keyPressDelegate("clear");
                }
                if (0x1B == pin[0])
                {
                     Logger.Device.Debug("cancel input");
                    keyPressDelegate("cancel");
                    break;
                }
                if (0x2A == pin[0])
                {
                    chPin.Append(pin[0]);
                    keyPressDelegate(chPin.ToString());
                    count++;
                    if (count == 6)
                    {
                        isSuccess = true;
                        break;
                    }
                }
            }
            ZT_EPP_OpenKeyVoic(0);
            if (isSuccess)
            {
                passBin = ReadBin();
                 Logger.Device.Debug("set password ok, password=******");
                keyPressDelegate("finish");
            }
            return true;
        }

        public static void CloseKeyboard()
        {
            //ZT_EPP_OpenKeyVoic(0);
             Logger.Device.Debug("Close Keyboard...");
            ZT_EPP_CloseCom();
             Logger.Device.Debug("Keyboard Closed");
        }

        public static bool PressKey()
        {
            Ret = ZT_EPP_OpenKeyVoic(3);
            short timeout = 30;

            bool isSuccess = false;
            long startTime = GetSecondNow();
            var pin = new StringBuilder(20);
            var chPin = new StringBuilder(20);
            int count = 0;
            while (true)
            {
                if (StopKeypress)
                {
                    //ZT_EPP_OpenKeyVoic(0);
                    // keyPressDelegate("exit");
                    break;
                }
                if (GetSecondNow() - startTime > timeout)
                {
                    ZT_EPP_OpenKeyVoic(0);
                    keyPressDelegate("timeout");
                    break;
                }
                try
                {
                    Ret = ZT_EPP_PinReportPressed(pin, 100);
                }
                catch (Exception)
                {
                    continue;
                }
                if (0x0D == pin[0] && count > 0)
                {
                    //chPin[count] = pin[0];
                    chPin.Append(pin[0]);
                    //StopKeypress = true;
                     Logger.Device.Debug("finish, input:" + chPin);
                    //keyPressDelegate("finish");
                    isSuccess = true;
                    break;
                }
                if (0x08 == pin[0])
                {
                    chPin.Clear();
                    count = 0;
                    keyPressDelegate("clear");
                }
                if (0x1B == pin[0])
                {
                    count = 0;
                     Logger.Device.Debug("cancel input");
                    keyPressDelegate("cancel");
                    break;
                }
                chPin.Append(pin[0]);
                keyPressDelegate(chPin.ToString());
                count++;
            }
            if (isSuccess)
            {
                 Logger.Device.Debug("set input ok");
            }
            return true;
        }

        public static int GetKeyPress()
        {
            int ret = 0;
            ret = ZT_EPP_OpenCom(Port, Baud);
             Logger.Device.Debug("GetKeyPress open keyboard com=" + Port + ", rate=" + Baud);
            if (ret != 0)
            {
                 Logger.Device.Debug("GetKeyPress open keyboard com erro,code=" + ret);
                return ret;
            }
            ZT_EPP_OpenKeyVoic(3);
            return ret;
        }

        public static string GetTerminalNum()
        {
            var set = new StringBuilder();
            var read = new StringBuilder();
            ZT_EPP_TerminalNum(set, read);
            return read.ToString();
        }

        public static string GetDllInfo()
        {
            var v1 = new StringBuilder(128);
            var v2 = new StringBuilder(32);
            try
            {
                ZT_EPP_GetDLLVersion(v1, v2);
            }
            catch (Exception e)
            {
                 Logger.Device.Debug(e.Message);
            }
            return "path=" + v1 + ",version=" + v2;
        }

        public static int DownloadMasterKey(string masterKey)
        {
            //ZT_EPP_PinInitialization(1);
            string key = masterKey;
            var chk = new StringBuilder(32);
            ret = ZT_EPP_PinLoadMasterKey(2, mainKeyIndex, key, chk);
            log = "下载主密钥，代码=" + ret;
             Logger.Device.Debug(log);
            return ret;
        }

        /// <summary>
        /// 数据加密
        /// </summary>
        /// <param name="kMode">
        /// 原下载的相应工作密钥的长度
        /// 1:PEA_DES 8字节的工作密钥DES运算;
        /// 2:PEA_TDES 16字节的工作密钥TDES运算;
        /// 3:PEA_TDES2 24字节的工作密钥
        /// 4:PEA_MDES 8字节的主密钥DES运算;
        /// 5:PEA_MTDES 16字节的主密钥TDES运算;</param>
        /// <param name="mode">
        /// ECB：代表模式0；(目前是以ECB方式)
        /// CBC：代表模式1。(部分支持,如C45或以后的)
        /// </param>
        /// <param name="data"></param>
        /// <returns></returns>
	    public static string PinAdd(int kMode, int mode, string data)
        {
            var res = new StringBuilder();
            Ret = ZT_EPP_PinAdd((short)kMode, (short)mode, data, res);//注释
            return res.ToString();
        }

        public static bool LoadWorkKey(string[] keyStrings)
        {
            return LoadWorkKey(keyStrings[0], keyStrings[1], keyStrings[2], keyStrings[3]);
        }

        public static bool LoadWorkKey(string pin, string pinchk, string mac, string macchk)
        {
            Ret = ZT_EPP_SetDesPara(0, 0x30);
            Ret = ZT_EPP_SetDesPara(1, 0x30);
            Ret = ZT_EPP_SetDesPara(4, 0x10);
            Ret = ZT_EPP_SetDesPara(5, 0x05);
            Ret = ZT_EPP_ActivWorkPin(mainKeyIndex, pinKeyIndexWithMain);
            var res = new StringBuilder();
            Ret = ZT_EPP_PinUnAdd(2, 0, "0000000000000000", res);
            var chkpin = new StringBuilder(32);
            Ret = ZT_EPP_PinLoadWorkKey(2, mainKeyIndex, pinKeyIndexWithMain, pin, chkpin);
             Logger.Device.Info($"PIN:{pin}\n {pinchk}\n {chkpin}\n 0x{mainKeyIndex:X} 0x{pinKeyIndexWithMain:X}");
            //if (!chkpin.ToString().StartsWith(pinchk))
            //    return false;

            //Ret = ZT_EPP_SetDesPara(1, 0x20);
            Ret = ZT_EPP_ActivWorkPin(mainKeyIndex, macKeyIndexWithMain);
            res = new StringBuilder();
            Ret = ZT_EPP_PinUnAdd(2, 0, "0000000000000000", res);
            var chkmac = new StringBuilder(32);
            Ret = ZT_EPP_PinLoadWorkKey(2, mainKeyIndex, macKeyIndexWithMain, mac, chkmac);
             Logger.Device.Info($"MAC:{mac}\n {macchk}\n {chkmac}\n 0x{mainKeyIndex:X} 0x{macKeyIndexWithMain:X}");

            //if (!chkmac.ToString().StartsWith(macchk))
            //    return false;

            return true;
        }

        public static byte[] CalcMAC(byte[] data)
        {
            try
            {
                // 8 字节对齐
                var g = data.Length / 8;
                if (data.Length % 8 != 0)
                {
                    g++;
                    var newData = new byte[g * 8];
                    Array.Copy(data, newData, data.Length);
                    data = newData;
                }
                // 求异或
                var buffer = new byte[8];
                Array.Copy(data, 0, buffer, 0, 8);
                for (var i = 1; i < g; i++)
                    for (var j = 0; j < 8; j++)
                        buffer[j] = (byte)(buffer[j] ^ data[i * 8 + j]);
                var sbuiler = new StringBuilder(1024);
                ZT_EPP_ActivWorkPin(0, (short)(mainKeyIndex * 4 + macKeyIndex + 0x40));
                ZT_EPP_PinAdd(1, 0, buffer.InnerBytes2Hex(), sbuiler);
                var macrest = sbuiler.ToString().Trim(new[] { ' ', '\0' });
                Logger.Device.Debug($"输入参数:{data.InnerBytes2Hex()}\r\nBuffer:{buffer.InnerBytes2Hex()}\r\n输出:{macrest}");
                return macrest.InnerHex2Bytes();
            }
            catch (Exception ex)
            {
                Console.WriteLine("MAC_ECB() Exception caught, exception = {0}", ex.Message);
                return new byte[8];
            }
        }
        public static byte[] EncrptyByMacKey(byte[] data)
        {
            var sbuiler = new StringBuilder(1024);
            ZT_EPP_ActivWorkPin(0, (short)(mainKeyIndex * 4 + macKeyIndex + 0x40));
            ZT_EPP_PinAdd(1, 0, data.InnerBytes2Hex(), sbuiler);
            var encRest = sbuiler.ToString().Trim(new[] { ' ', '\0' });
            Logger.Device.Debug($"输入参数EncrptyByMacKey:{data.InnerBytes2Hex()}\r\nBuffer:{data.InnerBytes2Hex()}\r\n输出:{encRest}");
            return encRest.InnerHex2Bytes();
        }
        public static byte[] EncrptyByPinKey(byte[] data)
        {
            var sbuiler = new StringBuilder(1024);
            ZT_EPP_ActivWorkPin(0, (short)(mainKeyIndex * 4 + pinKeyIndex + 0x40));
            ZT_EPP_PinAdd(2, 0, data.InnerBytes2Hex(), sbuiler);
            var encRest = sbuiler.ToString().Trim(new[] { ' ', '\0' });
            Logger.Device.Debug($"输入参数EncrptyByPinKey:{data.InnerBytes2Hex()}\r\nBuffer:{data.InnerBytes2Hex()}\r\n输出:{encRest}");
            return encRest.InnerHex2Bytes();
        }
        public static string InnerBytes2Hex(this byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var t in data)
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }
        public static byte[] InnerHex2Bytes(this string text)
        {
            var len = text.Length / 2;
            var data = new byte[len];
            for (var i = 0; i < len; i++)
                data[i] = (byte)(text[i * 2].InnerChar2Hex() * 0x10 + text[i * 2 + 1].InnerChar2Hex());
            return data;
        }
        public static int InnerChar2Hex(this char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;
            if (c == '=')
                return 0x0D;
            throw new ArgumentOutOfRangeException(nameof(c), $"无法转换:{c}");
        }
        public static string Bytes2Hex(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (byte t in data)
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }


        

    }
}