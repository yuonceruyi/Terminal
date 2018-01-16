using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Devices.MKeyBoard.Utils;

namespace YuanTu.Devices.MKeyBoard
{
    internal class ZtPinpadMKeyboard : IMKeyboard
    {
        public string DeviceName { get; } = "ZTKeyboard";
        public string DeviceId { get; } = "ZTKeyboard_ZTPinpad";
        private readonly IConfigurationManager _configurationManager;

        public int MasterKeyId { get; set; } = 1;
        public int PinKeyId { get; set; } = 2;
        public int MacKeyId { get; set; } = 3;

        public ZtPinpadMKeyboard(IConfigurationManager configurationManager)
        {
            _configurationManager = configurationManager;
        }

        private bool _isConnected;

        public Result<DeviceStatus> GetDeviceStatus()
        {
            throw new NotImplementedException();
        }

        public Result Connect()
        {
            if (_isConnected)
            {
                return Result.Success();
            }

            var port = _configurationManager.GetValueInt("MKeyboard:Port");
            var baud = _configurationManager.GetValueInt("MKeyboard:Baud");
            Ret = Open(EPORT.eCOM, EPIN_TYPE.ePIN_UNKNOWN, $"COM{port}:{baud},N,8,1");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 Open 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘初始化失败 {_error}");
            }

            //Ret = Init();
            //if (Ret != 0)
            //{
            //    Logger.Device.Debug($"金属键盘 Init 失败 {Ret} {_error}");
            //    return Result.Fail($"金属键盘初始化失败 {_error}");
            //}

            var version = new StringBuilder(128);
            Ret = GetHardwareVersion(version);
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 GetHardwareVersion 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘初始化失败 {_error}");
            }
            _isConnected = true;
            return Result.Success();
        }

        public Result Initialize()
        {
            throw new NotImplementedException();
        }

        public Result UnInitialize()
        {
            throw new NotImplementedException();
        }

        public Result DisConnect()
        {
            Ret = Close();
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 Close 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘关闭失败 {_error}");
            }
            _isConnected = false;
            return Result.Success();
        }

        public Result LoadMasterKey(string master, string masterchk)
        {
            byte checkLen = 8;
            byte[] checkBin = new byte[16 + 1];

            Ret = SetCaps(ECAPS.eCAP_KCVL, checkLen);
            Logger.Device.Debug($"Set KCV Length [{checkLen}] nRet is [{Ret}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 SetCaps 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘加载主密钥失败 {_error}");
            }
            Logger.Device.Debug($"LoadKey MASTER KEY {MasterKeyId} ...");
            var masterKey = master.Hex2Bytes();
            Ret = LoadKey(MasterKeyId, EKEYATTR.ATTR_MK, masterKey, masterKey.Length, KEY_INVALID, checkBin, EKCVMODE.KCVZERO, EKEYMODE.KEY_SET);
            var chkmaster = checkBin.Bytes2Hex(0, checkLen);
            Logger.Device.Debug($"LoadKey MASTER KEY {MasterKeyId} nRet is [{Ret}]  KCV is [{chkmaster}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 LoadKey PIN 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘加载主密钥失败 {_error}");
            }

            if (!chkmaster.StartsWith(masterchk))
            {
                Logger.Device.Debug($"金属键盘 比较结果 失败 pinchk：{masterchk} chkpin：{chkmaster}");
                return Result.Fail($"金属键盘下载主秘钥失败 {_error}");
            }

            return Result.Success();
        }

        public Result LoadWorkKey(string pin, string pinchk, string mac, string macchk)
        {
            byte checkLen = 8;
            byte[] checkBin = new byte[16 + 1];

            Ret = SetCaps(ECAPS.eCAP_KCVL, checkLen);
            Logger.Device.Debug($"Set KCV Length [{checkLen}] nRet is [{Ret}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 SetCaps 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘加载工作密钥失败 {_error}");
            }

            Logger.Device.Debug($"LoadKey PIN KEY {PinKeyId} MASTER KEY {MasterKeyId} ...");
            var pinKey = pin.Hex2Bytes();
            Ret = LoadKey(PinKeyId, EKEYATTR.ATTR_PK, pinKey, pinKey.Length, MasterKeyId, checkBin, EKCVMODE.KCVZERO, EKEYMODE.KEY_SET);
            var chkpin = checkBin.Bytes2Hex(0, checkLen);
            Logger.Device.Debug($"LoadKey PIN KEY {PinKeyId} nRet is [{Ret}]  KCV is [{chkpin}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 LoadKey PIN 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘加载工作密钥失败 {_error}");
            }

            if (!chkpin.StartsWith(pinchk))
            {
                Logger.Device.Debug($"金属键盘 比较结果 失败 pinchk：{pinchk} chkpin：{chkpin}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }

            Logger.Device.Debug($"LoadKey MAC KEY {MacKeyId} MASTER KEY {MasterKeyId} ...");
            var macKey = mac.Hex2Bytes();
            Ret = LoadKey(MacKeyId, EKEYATTR.ATTR_PK, macKey, macKey.Length, MasterKeyId, checkBin, EKCVMODE.KCVZERO, EKEYMODE.KEY_SET);
            var chkmac = checkBin.Bytes2Hex(0, checkLen);
            Logger.Device.Debug($"LoadKey MAC KEY {MacKeyId} nRet is [{Ret}]  KCV is [{chkmac}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 LoadKey MAC 失败 {Ret} {_error}");
                return Result.Fail($"金属键盘加载工作密钥失败 {_error}");
            }
            if (!chkmac.StartsWith(macchk))
            {
                Logger.Device.Debug($"金属键盘 比较结果 失败 macchk：{macchk} chkmac：{chkmac}");
                return Result.Fail($"金属键盘下载工作秘钥失败 {_error}");
            }

            return Result.Success();
        }

        public Result<string> CalcMac(string text, KMode kMode, MacMode macMode)
        {
            byte[] macBin = new byte[8 + 1]; //MAC hex string , form CalcMAC
            var data = text.Hex2Bytes();
            var dataLen = data.Length;
            EMAC mode;
            switch (macMode)
            {
                case MacMode.x9算法:
                    mode = EMAC.MAC_X919;
                    break;

                case MacMode.PBOC:
                    mode = EMAC.MAC_PBOC;
                    break;

                case MacMode.银联算法:
                    mode = EMAC.MAC_BANKSYS;
                    break;

                case MacMode.CBC算法:
                    mode = EMAC.MAC_CBC;
                    break;

                default:
                    mode = EMAC.MAC_X919;
                    break;
            }
            Ret = CalcMAC(MacKeyId, mode, data, ref dataLen, macBin, null, MasterKeyId);
            var macHex = macBin.Bytes2Hex(0, dataLen);
            Logger.Device.Debug($"MAC nRet is [{Ret}] MAC is [{macHex}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 CalcMac MAC 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘MAC计算失败 {_error}");
            }
            return Result<string>.Success(macHex);
        }

        public Result<string> BeforeAddPin(string cardNo = "000000000000")
        {
            int pinLen = 0; // input pin length , for GetPinBlock
            byte inputLen = 0;
            byte[] pinInput = new byte[16 + 1];
            byte[] pinBlockBin = new byte[8 + 1];//pinblock hex string , from GetPinBlock
            int i = 0;

            //open pin input , pen sound , max len is 6 , min len is 4 , auto ended if the input len is equal the max len
            Ret = StartPinInput(ESOUND.SOUND_OPEN, 6, 4, true);
            Logger.Device.Debug($"StartPinInput nRet is [{Ret}]");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 StartPinInput 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘获取密码失败 {_error}");
            }

            Logger.Device.Debug("Please input PIN.<<");

            var stateRet = 0;//for end the pin inpt , press the CANCEL buntton or ENTER buntton with len bewtten min len and max len

            while (0 <= stateRet)//the pin input will end when input ENTER or the StartPinInput auto end it
            {
                ReadText(pinInput, ref inputLen, 500);

                for (i = 0; i < inputLen; i++)
                {
                    if (0x1B == pinInput[i])
                    {
                        Logger.Device.Debug("You pressed the [CANCEL] button.");
                        Logger.Device.Debug("The input is going to close.");
                        stateRet = -1;//break the input,end the pin input with no pinblock
                        _keyPressDelegate.Invoke(new KeyText(0, KeyEnum.Cancel));
                        break;
                    }
                    else if (0x08 == pinInput[i])
                    {
                        Logger.Device.Debug("You pressed the [CLEAR] or [BACKSPACE] button.");
                        //close the input and reopen the pin input
                        Ret = OpenKeyboardAndSound(ESOUND.SOUND_CLOSE, ENTRYMODE.ENTRY_MODE_CLOSE);
                        Logger.Device.Debug($"Open Keyboard And Sound SOUND_CLOSE  ENTRY_MODE_CLOSE nRet is [{Ret}]\n");
                        Ret = StartPinInput(ESOUND.SOUND_OPEN, 6, 4, true);
                        Logger.Device.Debug($"StartPinInput  nRet is [{Ret}]\n");
                        if (Ret != 0)
                        {
                            Logger.Device.Debug($"金属键盘 StartPinInput 失败 {Ret} {_error}");
                            return Result<string>.Fail($"金属键盘获取密码失败 {_error}");
                        }
                        _keyPressDelegate.Invoke(new KeyText(0, KeyEnum.Clear));

                        pinLen = 0;
                    }
                    else if (0x0D == pinInput[i])//the pin input will end when input ENTER or the StartPinInput auto end it
                    {
                        Logger.Device.Debug("You pressed the [ENTER] button.");
                        if (pinLen >= 4)
                        {
                            Logger.Device.Debug("The pin input is end.");
                            stateRet = -1;//break the input,end the pin input and calculate pinblock
                            _keyPressDelegate.Invoke(new KeyText(0, KeyEnum.Confirm));
                            break;
                        }
                        Logger.Device.Debug("The input pin is insufficient please continue the pin input.");
                    }
                    else if (0x20 == pinInput[i])
                    {
                        Logger.Device.Debug("You pressed the [BLACK] button.");
                    }
                    else if (0x2E == pinInput[i])
                    {
                        Logger.Device.Debug("You pressed the [.] button.");
                    }
                    else if (0x7F == pinInput[i])
                    {
                        Logger.Device.Debug("You pressed the [00] button.");
                    }
                    else if (0x80 == pinInput[i])
                    {
                        Logger.Device.Debug("Time out.");
                        _keyPressDelegate.Invoke(new KeyText(0, KeyEnum.Timeout));
                        stateRet = -1;
                        break;
                    }
                    else
                    {
                        Logger.Device.Debug($"You pressed the [{(char)pinInput[i]}] button");
                        pinLen++;
                        _keyPressDelegate.Invoke(new KeyText(pinLen, KeyEnum.Number));
                        if (6 == pinLen)
                            break;
                    }
                }
            }
            //close the input
            Ret = OpenKeyboardAndSound(ESOUND.SOUND_CLOSE, ENTRYMODE.ENTRY_MODE_CLOSE);
            Logger.Device.Debug($"Open Keyboard And Sound SOUND_CLOSE  ENTRY_MODE_CLOSE nRet is [{Ret}]\n");

            if (pinInput[i] == 0x1B) //CANCEL
                return Result<string>.Fail("已取消输入");
            if (pinInput[i] == 0x80) //Time out
                return Result<string>.Fail("操作超时");
            int outlen = pinBlockBin.Length;
            Ret = GetPinBlock(PinKeyId, (byte)pinLen, EPINFORMAT.FORMAT_ISO0, cardNo, pinBlockBin, ref outlen, 0x0F, MasterKeyId);
            var pinBolck = pinBlockBin.Bytes2Hex();
            Logger.Device.Debug($"GetPinBlock nRet is [{Ret}] PINBLOCK is [{pinBolck}]\n");
            if (Ret != 0)
            {
                Logger.Device.Debug($"金属键盘 GetPinBlock 失败 {Ret} {_error}");
                return Result<string>.Fail($"金属键盘获取密码失败 {_error}");
            }
            return Result<string>.Success(pinBolck);
        }

        public Result SetKeyAction(Action<KeyText> keyAction)
        {
            _keyPressDelegate = keyAction;
            return Result.Success();
        }

        private Action<KeyText> _keyPressDelegate;

        #region ErrorCodes

        private int _ret;

        private int Ret
        {
            get { return _ret; }
            set
            {
                _ret = value;
                if (value != 0)
                {
                    _error = GetError(value);
                    //throw new Exception(_error);
                }
            }
        }

        private string _error;
        private const int BASE_COMMON = 1000; //common for all
        private const int BASE_PINPAD = 2000; //Pinpad(2000 ~ 2099)
        private const int WARN_PINPAD = 2100; //Pinpad warn(2100 ~ 2199)

        private static Dictionary<int, string> ErrorDictionary = new Dictionary<int, string>()
        {
            [0] = "EC_SUCCESS",
            [BASE_COMMON + 1] = "EC_OPEN_FAILED",
            [BASE_COMMON + 2] = "EC_INVALID_PORT",
            [BASE_COMMON + 3] = "EC_INVALID_PARA",
            [BASE_COMMON + 4] = "EC_INVALID_DATA",
            [BASE_COMMON + 5] = "EC_SEND_FAILED",
            [BASE_COMMON + 6] = "EC_RECEIVED_FAILED",
            [BASE_COMMON + 7] = "EC_USER_CANCEL",
            [BASE_COMMON + 8] = "EC_DATA_TOOLONG",
            [BASE_COMMON + 9] = "EC_NAK_RECEIVED",
            [BASE_COMMON + 10] = "EC_READ_TIMEOUT",
            [BASE_COMMON + 11] = "EC_WRITE_TIMEOUT",
            [BASE_COMMON + 12] = "EC_WAITEVENT_FAILED",
            [BASE_COMMON + 13] = "EC_SET_FAILED",
            [BASE_COMMON + 14] = "EC_STEP_ERROR",
            [BASE_COMMON + 15] = "EC_POINTER_NULL",
            [BASE_COMMON + 16] = "EC_FULL_NOW",
            [BASE_COMMON + 17] = "EC_NET_ERROR",
            [BASE_COMMON + 18] = "EC_INVALID_FILE",
            [BASE_COMMON + 19] = "EC_TEST_MODE",
            [BASE_COMMON + 20] = "EC_EXIT",
            [BASE_COMMON + 21] = "EC_ALLOC_FAILED",
            [BASE_COMMON + 22] = "EC_TYPE_UNMATCH",
            [BASE_COMMON + 23] = "EC_RETURN_FAILED",
            [BASE_COMMON + 24] = "EC_SERIOUS_ERROR",
            [BASE_COMMON + 25] = "EC_UNSUPPORT",
            [BASE_COMMON + 26] = "EC_COMMAND_UNMATCH",
            [BASE_COMMON + 27] = "EC_SEQ_UNMATCH",
            [BASE_PINPAD + 1] = "PIN_INVALID_COMMAND_PARA",
            [BASE_PINPAD + 2] = "PIN_MAC_XOR_ERROR",
            [BASE_PINPAD + 3] = "PIN_MAC_CRC_ERROR",
            [BASE_PINPAD + 4] = "PIN_MAC_KEYCOMMAND_ERROR",
            [BASE_PINPAD + 5] = "PIN_INNER_ERROR",
            [BASE_PINPAD + 6] = "PIN_INVALID_DATA",
            [BASE_PINPAD + 7] = "PIN_DATA_TOOLONG",
            [BASE_PINPAD + 8] = "PIN_COMMAND_UNSUPPORT",
            [BASE_PINPAD + 9] = "PIN_ALGORITHM_UNSUPPORT",
            [BASE_PINPAD + 10] = "PIN_SERIAL_NUM_ERROR",
            [BASE_PINPAD + 11] = "PIN_INVALID_RSA_SN",
            [BASE_PINPAD + 12] = "PIN_EPP_NOT_INITIALIZED",
            [BASE_PINPAD + 13] = "PIN_SELFTEST_ERROR",
            [BASE_PINPAD + 14] = "PIN_PRESS_KEY_TIMEOUT",
            [BASE_PINPAD + 15] = "PIN_KEY_UNRELEASED",
            [BASE_PINPAD + 16] = "PIN_NOPSW_OR_ERROR",
            [BASE_PINPAD + 17] = "PIN_INVALID_PIN_LENGTH",
            [BASE_PINPAD + 18] = "PIN_GET_PINBLOCK_ERROR",
            [BASE_PINPAD + 19] = "PIN_RANDOM_DATA_ERROR",
            [BASE_PINPAD + 20] = "PIN_INVALID_ENTRY_MODE",
            [BASE_PINPAD + 21] = "PIN_INVALID_WRITE_MODE",
            [BASE_PINPAD + 22] = "PIN_INVALID_KEYID",
            [BASE_PINPAD + 23] = "PIN_KEY_USEVIOLATION",
            [BASE_PINPAD + 24] = "PIN_KEY_NOT_LOADED",
            [BASE_PINPAD + 25] = "PIN_KEY_LOCKED",
            [BASE_PINPAD + 26] = "PIN_INVALID_MASTER_KEY",
            [BASE_PINPAD + 27] = "PIN_IMK_NOT_EXIST",
            [BASE_PINPAD + 28] = "PIN_TMK_NOT_EXIST",
            [BASE_PINPAD + 29] = "PIN_KEY_NOT_EXIST",
            [BASE_PINPAD + 30] = "PIN_SAME_KEY_VALUE",
            [BASE_PINPAD + 31] = "PIN_INVALID_KEY_VALUE",
            [BASE_PINPAD + 32] = "PIN_INVALID_KEY_LENGTH",
            [BASE_PINPAD + 33] = "PIN_INVALID_IV_ATTRIBUTES",
            [BASE_PINPAD + 34] = "PIN_INVALID_KEY_ATTRIBUTES",
            [BASE_PINPAD + 35] = "PIN_INVALID_OFFSET_LENGTH",
            [BASE_PINPAD + 36] = "PIN_INVALID_LENGTH_OR_SUM",
            [BASE_PINPAD + 37] = "PIN_ENCRYPT_SUSPENDED",
            [BASE_PINPAD + 38] = "PIN_AUTHENTICATE_LOCKED_HOURS",
            [BASE_PINPAD + 39] = "PIN_COMMAND_LOCKED",
            [BASE_PINPAD + 40] = "PIN_INVALID_USERBLOCK_ADDRESS",
            [BASE_PINPAD + 41] = "PIN_INVALID_MODULUS_LENGTH",
            [BASE_PINPAD + 42] = "PIN_INVALID_EXPONENT_LENGTH",
            [BASE_PINPAD + 43] = "PIN_INVALID_PKCS_STRUCTURE",
            [BASE_PINPAD + 44] = "PIN_INVALID_PKCS_PADDING",
            [BASE_PINPAD + 45] = "PIN_INVALID_SIGNATURE_LENGTH",
            [BASE_PINPAD + 46] = "PIN_INVALID_SIGNATURE_SHA",
            [BASE_PINPAD + 47] = "PIN_SIG_VERIFICATION_FAILED",
            [BASE_PINPAD + 48] = "PIN_KCV_VERIFICATION_FAILED",
            [BASE_PINPAD + 49] = "PIN_PIN_VERIFICATION_FAILED",
            [BASE_PINPAD + 50] = "PIN_VERIFICATION_FAILED",
            [BASE_PINPAD + 51] = "PIN_NOT_AUTHENTE",
            [BASE_PINPAD + 52] = "PIN_INVALID_AUTHENTICATION_MODE",
            [BASE_PINPAD + 53] = "PIN_CERTIFICATE_NOT_EXIST",
            [BASE_PINPAD + 54] = "PIN_RECV_SPECIAL_KEY",
            [BASE_PINPAD + 55] = "PIN_INVALID_CERTIFICATE_FORMAT",
            [BASE_PINPAD + 56] = "PIN_INVALID_CERTIFICATE_VERSION",
            [BASE_PINPAD + 57] = "PIN_INVALID_CERTIFICATE_ISSUER",
            [BASE_PINPAD + 58] = "PIN_INVALID_CERTIFICATE_VALIDITY",
            [BASE_PINPAD + 59] = "PIN_INVALID_CERTIFICATE_SUBJECT",
            [BASE_PINPAD + 60] = "PIN_INVALID_CERTIFICATE_ALGOR",
            [BASE_PINPAD + 61] = "PIN_NO_CARD",
            [BASE_PINPAD + 62] = "PIN_CARD_APDU_ERROR",
            [BASE_PINPAD + 63] = "PIN_EMV_NOT_INITIALIZED",
            [BASE_PINPAD + 64] = "PIN_EMV_NOT_READY",
            [BASE_PINPAD + 65] = "PIN_EMV_NEED_REINITIALIZE",
            [BASE_PINPAD + 66] = "PIN_EMV_TIMEOUT",
            [BASE_PINPAD + 67] = "PIN_PSW_NOT_INITIALIZED",
            [BASE_PINPAD + 68] = "PIN_EPP_NOT_INSTALLED",
            [BASE_PINPAD + 69] = "PIN_INVALID_PADDING",
            [BASE_PINPAD + 70] = "PIN_PHYSICALLY_UNINSTALLED",
            [BASE_PINPAD + 71] = "PIN_LOGICALLY_UNINSTALLED",
            [BASE_PINPAD + 72] = "PIN_INPUT_KEY_TIMEOUT",
            [BASE_PINPAD + 73] = "PIN_INVALID_PASSWORD_LENGTH",
            [BASE_PINPAD + 74] = "PIN_INVALID_PASSWORD",
            [BASE_PINPAD + 75] = "PIN_INPUT_PASSWORD_LOCKED",
            [BASE_PINPAD + 76] = "PIN_SYSTEM_TIME_NOT_SET",
            [BASE_PINPAD + 77] = "PIN_SYSTEM_TIME_ALREADY_SET",
            [BASE_PINPAD + 78] = "PIN_MRAM_HARDWARE_ERROR",
            [BASE_PINPAD + 79] = "PIN_DEVICE_TAMPERED",
            [BASE_PINPAD + 80] = "PIN_SM2_ENCRYPT_FAILURE",
            [BASE_PINPAD + 81] = "PIN_SM2_DECRYPT_FAILURE",
            [BASE_PINPAD + 82] = "PIN_SM2_SIGNATURE_FAILURE",
            [BASE_PINPAD + 83] = "PIN_SM2_VERSIG_FAILURE",
            [BASE_PINPAD + 84] = "PIN_SM2_KEYEXC_FAILURE",
            [BASE_PINPAD + 85] = "PIN_SM2_VER_KEYEXC_FAILURE",
            [BASE_PINPAD + 86] = "PIN_CHIP_TIMEOUT",
            [BASE_PINPAD + 87] = "PIN_INVALID_SM4_KEYVAL",
            [BASE_PINPAD + 88] = "PIN_INVALID_INSTALLATION_MODE",
            [BASE_PINPAD + 89] = "PIN_CHIP_INNER_ERROR",
            [WARN_PINPAD + 1] = "PIN_EMV_ALREADY_INITIALIZED",
            [WARN_PINPAD + 2] = "PIN_POWER_ERROR",
            [WARN_PINPAD + 3] = "PIN_CERTIFICATE_ALREADY",
            [WARN_PINPAD + 4] = "PIN_EPP_ALREADY_INITIALIZED",
        };

        private static string GetError(int errorCode)
        {
            string message;
            if (ErrorDictionary.TryGetValue(errorCode, out message))
                return message;
            return $"Unknown Error:{errorCode}";
        }

        #endregion ErrorCodes

        #region Enums

        //the enums from IPinpad.h
        public enum EPORT
        {
            eCOM = 0x0     // Serial("COM1:9600,N,8,1")
            ,
            eUSB_FTDI = 0x1     // only for FTDI chip with baudrate, such as EPP("FT232R USB UART:9600,N,8,1")
            ,
            eUSB = 0x2     // Windows(such as "VID_23AB&PID_0002","VID_23AB&PID_0002&REV_0900","USB\\VID_23AB&PID_0002")
            // Linux  ("lp0")
            ,
            eHID = 0x3     // Windows(such as "VID_23AB&PID_1003","VID_23AB&PID_1003&REV_0100","HID\\VID_23AB&PID_1003")
            // Linux  ("hiddev0")
            ,
            ePC_SC = 0x20     // only for windows PC/SC()
            ,
            eLPT = 0x40     // LPT("LPT1")
            ,
            eTCPIP = 0x80     // TCP(such as "127.0.0.1:36860")
            ,
            eCOMBINE = 0x100     // It is combined with master device, and must behind master device instantiated
        }

        public enum EPIN_TYPE
        {
            ePIN_UNKNOWN = 0x0   //unknown type(DLL will try to detect pinpad's type,when finish do that you cann't change other command pinpad)
            ,
            ePIN_EPP = 0x10000   //EPP command type(Bxx Cxx Exx Vxx)
            ,
            ePIN_VISA = 0x20000   //VISA command type(Dxx)
            ,
            ePIN_PCI = 0x40000   //PCI command type(Hxx)
            ,
            ePIN_WOSA = 0x80000   //WOSA command type(Fxx)

            ,
            ePIN_VISA_3X = ePIN_VISA + 0x2  //VISA_3X
            ,
            ePIN_EPP_BR = ePIN_EPP + 0x4  //EPP_BR(B7/C60)
            , ePIN_WOSA_3X = ePIN_WOSA + 0x8  //WOSA_3X
        }

        public enum ECRYPT
        {
            CRYPT_DESECB = 0x0001  //DES ECB
            ,
            CRYPT_DESCBC = 0x0002  //DES CBC
            ,
            CRYPT_DESCFB = 0x0004  //DES CFB
            ,
            CRYPT_RSA = 0x0008  //RSA
            ,
            CRYPT_ECMA = 0x0010  //ECMA
            ,
            CRYPT_DESMAC = 0x0020  //DES MAC
            ,
            CRYPT_TRIDESECB = 0x0040  //TDES ECB
            ,
            CRYPT_TRIDESCBC = 0x0080  //TDES CBC
            ,
            CRYPT_TRIDESCFB = 0x0100  //TDES CFB
            ,
            CRYPT_TRIDESMAC = 0x0200  //TDES MAC
            ,
            CRYPT_MAAMAC = 0x0400  //MAA MAC
            ,
            CRYPT_SM4ECB = 0x1000 //SM4 ECB
            ,
            CRYPT_SM4MAC = 0x2000 //SM4 CBC
            ,
            CRYPT_SM4CBC = 0x4000 //SM4 MAC
            ,
            CRYPT_OFB = 0x10000 //AES OFB
            ,
            CRYPT_CFB = 0x20000 //AES CFB
            ,
            CRYPT_PCBC = 0x40000 //AES PCBC
            , CRYPT_CTR = 0x80000 //AES CTR
        }  //Crypt mode

        public enum EPIN_EXTEND
        {
            eEX_NONE = 0x00  //No extend
            , eEX_STRING_NAME = 0x01  //Key name is string, set this to use XFS_XXX function
            , eEX_ENLARGE_KEY = 0x02  //Auto enlarge key usage or key attribute
            , eEX_PC_KB = 0x04  //use as PC keyboard

            , eEX_SAVE2EPP = 0x08  //mapped table save to EPP(it can't use singleness and some EPP unsupport)

            , eEX_1_8 = eEX_STRING_NAME | eEX_SAVE2EPP
            , eEX_2_8 = eEX_ENLARGE_KEY | eEX_SAVE2EPP
            , eEX_4_8 = eEX_PC_KB | eEX_SAVE2EPP

            , eEX_1_2 = eEX_STRING_NAME | eEX_ENLARGE_KEY
            , eEX_1_4 = eEX_STRING_NAME | eEX_PC_KB
            , eEX_1_2_8 = eEX_STRING_NAME | eEX_ENLARGE_KEY | eEX_SAVE2EPP
            , eEX_1_4_8 = eEX_STRING_NAME | eEX_PC_KB | eEX_SAVE2EPP
        }

        public enum EPINFORMAT
        {
            FORMAT_ARITHMETIC = 0x0000      //arithmetic choose
            ,
            FORMAT_IBM3624 = 0x0001      //IBM3624
            ,
            FORMAT_ANSI = 0x0002      //ANSI 9.8
            ,
            FORMAT_ISO0 = 0x0004      //ISO9564 0
            ,
            FORMAT_ISO1 = 0x0008      //ISO9564 1
            ,
            FORMAT_ECI2 = 0x0010      //ECI2
            ,
            FORMAT_ECI3 = 0x0020      //ECI3
            ,
            FORMAT_VISA = 0x0040      //VISA/VISA2
            ,
            FORMAT_DIEBOLD = 0x0080      //DIEBOLD
            ,
            FORMAT_DIEBOLDCO = 0x0100      //DIEBOLDCO
            ,
            FORMAT_VISA3 = 0x0200      //VISA3
            ,
            FORMAT_BANKSYS = 0x0400      //Bank system
            ,
            FORMAT_EMV = 0x0800      //EMV
            ,
            FORMAT_ISO3 = 0x2000      //ISO9564 3
            ,
            FORMAT_AP = 0x4000      //AP
        }

        public enum EMAC
        {
            MAC_X9 = 0x00      //X9.9
            ,
            MAC_X919 = 0x01      //X9.19
            ,
            MAC_PSAM = 0x02      //PSAM
            ,
            MAC_PBOC = 0x03      //PBOC
            ,
            MAC_CBC = 0x04      //CBC(ISO 16609)
            ,
            MAC_BANKSYS = 0x05      //Bank system
            ,
            AES_CMAC = 0x06      //AES-CMAC-PRF-128
            ,
            AES_XCBC = 0x07      //AES-XCBC-PRF-128
            ,
            SM4MAC_PBOC = 0x08      //PBOC
            ,
            SM4MAC_BANKSYS = 0x09      //Bank system
        }

        public enum EKEYATTR
        {
            ATTR_SPECIAL = 0x0000  //special key(UID | UAK | KBPK | IMK)
            ,
            ATTR_SM = 0x0008  //
            ,
            ATTR_DK = 0x0001  //DATA KEY(WFS_PIN_USECRYPT)
            ,
            ATTR_PK = 0x0002  //PIN KEY(WFS_PIN_USEFUNCTION)
            ,
            ATTR_AK = 0x0004  //MAC KEY(WFS_PIN_USEMACING)
            ,
            ATTR_MK = 0x0020  //MASTER KEY / MK only for MK(WFS_PIN_USEKEYENCKEY)
            ,
            ATTR_IV = 0x0080  //IV KEY(WFS_PIN_USESVENCKEY)

            ,
            ATTR_NODUPLICATE = 0x0040  //All the key value must diffent(WFS_PIN_USENODUPLICATE)
            ,
            ATTR_ANSTR31 = 0x0400  //ANSTR31 MASTER KEY(WFS_PIN_USEANSTR31MASTER)
            ,
            ATTR_PINLOCAL = 0x10000  //pin local offset(WFS_PIN_USEPINLOCAL)

            ,
            ATTR_RSAPUBLIC = 0x20000  //RSA public(WFS_PIN_USERSAPUBLIC)
            ,
            ATTR_RSAPRIVATE = 0x40000  //RSA private(WFS_PIN_USERSAPRIVATE)
            ,
            ATTR_RSA_VERIFY = 0x8000000  //RSA public verify(WFS_PIN_USERSAPUBLICVERIFY)
            ,
            ATTR_RSA_SIGN = 0x10000000  //RSA private sign(WFS_PIN_USERSAPRIVATESIGN)

            ,
            ATTR_CHIPINFO = 0x100000  //WFS_PIN_USECHIPINFO
            ,
            ATTR_CHIPPIN = 0x200000  //WFS_PIN_USECHIPPIN
            ,
            ATTR_CHIPPS = 0x400000  //WFS_PIN_USECHIPPS
            ,
            ATTR_CHIPMAC = 0x800000  //WFS_PIN_USECHIPMAC
            ,
            ATTR_CHIPLT = 0x1000000  //WFS_PIN_USECHIPLT
            ,
            ATTR_CHIPMACLZ = 0x2000000  //WFS_PIN_USECHIPMACLZ
            ,
            ATTR_CHIPMACAZ = 0x4000000  //WFS_PIN_USECHIPMACAZ

            ,
            ATTR_MPK = ATTR_MK | ATTR_PK  //MASTER KEY only for PIN KEY
            ,
            ATTR_MDK = ATTR_MK | ATTR_DK  //MASTER KEY only for DATA KEY
            ,
            ATTR_MAK = ATTR_MK | ATTR_AK  //MASTER KEY only for MAC KEY
            ,
            ATTR_MIV = ATTR_MK | ATTR_IV  //MASTER KEY only for IV

            ,
            ATTR_WK = ATTR_PK | ATTR_DK | ATTR_AK
            ,
            ATTR_MWK = ATTR_MK | ATTR_PK | ATTR_DK | ATTR_AK
        }

        public enum EKEYMODE
        {
            KEY_SET = 0x30  // It's equivalent to "combine" at some pinpad
            ,
            KEY_XOR = 0x31
            ,
            KEY_XOR2 = 0x32
            ,
            KEY_XOR3 = 0x33
        }

        public enum EKCVMODE
        {
            KCVNONE = 0x0       //no KCV
            ,
            KCVSELF = 0x1       //key encrypt itself(first 8 char)
            ,
            KCVZERO = 0x2       //key encrypt 00000000
        }

        public enum ENTRYMODE
        {
            ENTRY_MODE_CLOSE = 0x0
            ,
            ENTRY_MODE_TEXT = 0x1
            ,
            ENTRY_MODE_PIN = 0x2
            ,
            ENTRY_MODE_KEY = 0x3
        }

        public enum ESOUND
        {
            SOUND_CLOSE = 0x0
            ,
            SOUND_OPEN = 0x1
            ,
            SOUND_KEEP = 0x2
        }

        public enum EINSTALL_AUTH
        {
            AUTH_FORBID = 0x30      //forbid(disable) RemoveInstallAuth, don't use this RemoveInstallAuth
            ,
            AUTH_REMOVE = 0x31      //remove RemoveInstallAuth
            ,
            AUTH_INSTALL = 0x32      //install RemoveInstallAuth
        }

        public enum EPSW_ID
        {
            PSW_ID1 = 0x1       //password id 1
            ,
            PSW_ID2 = 0x2       //password id 2
            ,
            PSW_ID3 = 0x3       //password id 3
            ,
            PSW_ID4 = 0x4       //password id 4 (Password1 for RemoveInstallAuth)
            ,
            PSW_ID5 = 0x5       //password id 5 (Password2 for RemoveInstallAuth)
        }

        public enum EPSW
        {
            PSW_OLD = 0x30      // use the PIN buffer
            , PSW_NEW = 0x31      // use the KEY buffer
        }

        public enum ECAPS
        {
            eCAP_RSA_HASHALG = 0x0001 //_pin_caps.dwRSAHashAlg, see the EHASH
            , eCAP_KCVL = 0x0002 //_pin_caps.ucKCVL
            , eCAP_UKO = 0x0003 //_pin_caps.wUserKeyOffset, don't change it unless you need to compatibility some project
            , eCAP_SPLIT_CBS_MAC = 0x0004 //_pin_caps.bSplitBankSysMAC, default is TRUE
            , eCAP_RSA_SIG_ALG = 0x0005 //_pin_caps.dwRSASignatureAlgorithm, see the ESIG_ALGORITHM

            , eCAP_DPASPA = 0x1000 //Control DPA/SPA, 0 is Disable, else is enable
            , eCAP_CMDSEQ = 0x2000 //Command sequence add one every time, 0 is FALSE, else is TRUE, default is FALSE
            , eCAP_MAPPINGPATH = 0x3000 //The path of enlarge key mapping table, maybe only path or include file name(*.dat)
        }

        public enum INIT_MODE
        {
            ALL = 0,
            HW = 1,//only hardware/firmware init
            SOFT = 2,//only software init
        }

        #endregion Enums

        #region DLLImport

        public const int KEY_INVALID = 0XFFFF;
        public const byte[] CONST_BYTE = null;
        public const string DLL_PATH = "External\\ZtPinpad\\PinpadC.dll";//modify DLL DLL_PATH
        public const string PORT_INFO = "COM1:115200,N,8,1";//modify PINPAD COM INFO

        [DllImport(DLL_PATH, EntryPoint = "AutoEnlargeKeyC", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern void AutoEnlargeKeyC(bool bEnable);

        [DllImport(DLL_PATH, EntryPoint = "Open", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Open(EPORT eDevice, EPIN_TYPE eType, string lpDescription, EPIN_EXTEND eExtend = EPIN_EXTEND.eEX_NONE);

        [DllImport(DLL_PATH, EntryPoint = "Init", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Init(INIT_MODE nMode = 0);

        [DllImport(DLL_PATH, EntryPoint = "GetHardwareVersion", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetHardwareVersion(StringBuilder pcVersion);

        [DllImport(DLL_PATH, EntryPoint = "SetCaps", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetCaps(ECAPS eCapsSwitch, int dwValue);

        [DllImport(DLL_PATH, EntryPoint = "Soft_Hex2Bin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Soft_Hex2Bin(byte[] pBin, int dwBufLen, string pHex, int dwLen); //2 visible hex char to 1 binary char

        [DllImport(DLL_PATH, EntryPoint = "Soft_Bin2Hex", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Soft_Bin2Hex(byte[] pHex, int dwBufLen, byte[] pBin, int dwLen); //1 binary char to 2 visible hex char

        [DllImport(DLL_PATH, EntryPoint = "OpenKeyboardAndSound", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int OpenKeyboardAndSound(ESOUND eSound, ENTRYMODE eMode, int dwDisableKey = 0, int dwDisableFDK = 0);

        [DllImport(DLL_PATH, EntryPoint = "ReadText", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ReadText(byte[] lpText, ref byte dwOutLen, int dwTimeOut = 2000);

        [DllImport(DLL_PATH, EntryPoint = "LoadKey", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int LoadKey(int wKeyId, EKEYATTR dwKeyAttr, byte[] lpKey, int iKeyLen, int wEnKey = KEY_INVALID,
            byte[] lpKCVRet = CONST_BYTE, EKCVMODE eKCV = EKCVMODE.KCVNONE, EKEYMODE eMode = EKEYMODE.KEY_SET);

        [DllImport(DLL_PATH, EntryPoint = "StartPinInput", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int StartPinInput(ESOUND eSound, byte MaxLen = 6, byte MinLen = 4, bool bAutoEnd = true, byte TimeOut = 30);

        [DllImport(DLL_PATH, EntryPoint = "GetPinBlock", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetPinBlock(int wKeyId, byte PinLen, EPINFORMAT ePinFormat, string lpCardNo,
            byte[] pPinBlock, ref int wOutLen, byte Padding = 0x0F, int wEnKeyId = KEY_INVALID);

        [DllImport(DLL_PATH, EntryPoint = "CalcMAC", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int CalcMAC(int wKeyId, EMAC eMac, byte[] lpDataIn, ref int dwInOutLen, byte[] lpOutData,
            string lpIVdata = null, int wMK1IV = KEY_INVALID);

        [DllImport(DLL_PATH, EntryPoint = "Crypt", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Crypt(int wKeyId, ECRYPT eMode, byte[] lpDataIn, ref int dwInOutLen, byte[] lpOutData, bool bEncrypt = true,
            string lpIVdata = null, int wMK1IV = KEY_INVALID);

        [DllImport(DLL_PATH, EntryPoint = "Close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Close();

        #endregion DLLImport
    }
}

#region Utils

namespace YuanTu.Devices.MKeyBoard.Utils
{
    public static class Extentions
    {
        public static int Char2Hex(this char c)
        {
            if ((c >= '0') && (c <= '9'))
                return c - '0';
            if ((c >= 'A') && (c <= 'F'))
                return c - 'A' + 10;
            if ((c >= 'a') && (c <= 'f'))
                return c - 'a' + 10;
            if (c == '=')
                return 0x0D;
            throw new ArgumentOutOfRangeException(nameof(c), $"无法转换:{c}");
        }

        public static byte[] Hex2Bytes(this string text)
        {
            var len = text.Length / 2;
            var data = new byte[len];
            for (var i = 0; i < len; i++)
                data[i] = (byte)(text[i * 2].Char2Hex() * 0x10 + text[i * 2 + 1].Char2Hex());
            return data;
        }

        public static string Bytes2Hex(this byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var t in data)
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }

        public static string Bytes2Hex(this byte[] data, int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (count <= 0 || startIndex + count >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            var sb = new StringBuilder();
            foreach (var t in data.Skip(startIndex).Take(count))
                sb.Append(t.ToString("X2"));
            return sb.ToString();
        }
    }
}

#endregion Utils