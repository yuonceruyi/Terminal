using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using YuanTu.Core.Log;
using System.Runtime.InteropServices;

namespace YuanTu.Devices.CashBox
{
    public class HATM
    {
        private static AtmCallback _innerCallback;

        public static bool HATM_Inited { get; private set; }

        public static bool HATM_CIM_Inited { get; private set; }

        public static bool HATM_IDC_Inited { get; private set; }

        ~HATM()
        {
            if (HATM_Inited)
                Uninitialize();
        }

        public static void Initialize()
        {
            var t = new Thread(() =>
            {
                Logger.Device.Info("开始初始化");
                _innerCallback = InitCallback;
                ATMInit(_AtmCallback);
            })
            { IsBackground = true };
            t.Start();
        }

        public static bool Uninitialize()
        {
            Logger.Device.Info("开始反初始化");
            var ret = ATMUnInit();
            if (ret == 1)
            {
                Logger.Device.Info("反初始化完成");
                return true;
            }
            Logger.Device.Info("反初始化失败");
            return false;
        }

        public static bool SetCallback(AtmCallback callback)
        {
            Logger.Device.Info("开始设置回调");
            var ret = ATMSetCallback(callback);
            if (ret == 1)
            {
                Logger.Device.Info("设置回调完成");
                return true;
            }
            Logger.Device.Info("设置回调失败");
            return false;
        }

        public static bool Call(Transaction transaction, AtmCallback callback)
        {
            _innerCallback = callback;
            Logger.Device.Info("发送命令:" + transaction);
            var ret = ATMCall(transaction);
            if (ret == 1)
            {
                Logger.Device.Info("发送命令完成:" + transaction);
                return true;
            }
            Logger.Device.Info("发送命令失败:" + transaction);
            return false;
        }

        private static void _AtmCallback(CallbackCode callbackCode, int ret, string msg)
        {
            var lastError = Marshal.GetLastWin32Error();
            Logger.Device.Info("Callback:CallbackCode=" + callbackCode + " ret=" + ret + " msg=" + msg + " last=" + lastError);
            _innerCallback(callbackCode, ret, msg);
        }

        private static void InitCallback(CallbackCode callbackCode, int ret, string msg)
        {
            switch (callbackCode)
            {
                case CallbackCode.INIT_OK:
                    Logger.Main.Info("WFS 服务初始化成功");
                    HATM_Inited = true;
                    //Call(Transaction.StartCardService, InitCallback);
                    Call(Transaction.StartCashServiceStart, InitCallback);
                    break;

                case CallbackCode.INIT_FAIL:
                    Logger.Main.Error("WFS 服务初始化失败 ret=" + ret);
                    break;

                case CallbackCode.STARTCARDSERVICE_OK:
                    if (ret != 0)
                    {
                        Logger.Main.Info("IDC 服务初始化完成 ret=" + ret);
                        return;
                    }
                    Logger.Main.Info("IDC 服务初始化成功");
                    HATM_IDC_Inited = true;
                    Call(Transaction.StopCardService, InitCallback);
                    break;

                case CallbackCode.STARTCARDSERVICE_FAIL:
                    Logger.Main.Info("IDC 服务初始化失败 ret=" + ret);
                    break;

                case CallbackCode.STARTCASHSERVICE_OK:
                    if (ret != 0)
                    {
                        Logger.Main.Info("CIM 服务初始化完成 ret=" + ret);
                        return;
                    }
                    Logger.Main.Info("CIM 服务初始化成功");
                    HATM_CIM_Inited = true;
                    break;

                case CallbackCode.STARTCASHSERVICE_FAIL:
                    Logger.Main.Error("CIM 服务初始化失败 ret=" + ret);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(callbackCode), callbackCode, null);
            }
        }

        //TODO: Test CheckCard
        public static void CheckCard()
        {
            Call(Transaction.StartCardService, CardCallback);
        }

        public static void StopCheckCard()
        {
            Call(Transaction.EjectCard, CardCallback);
        }
        public static Action<string> ValadateAction { get; set; }
        public static string CardNo { get; private set; }

        private static void CardCallback(CallbackCode callbackCode, int ret, string msg)
        {
            switch (callbackCode)
            {
                case CallbackCode.STARTCARDSERVICE_OK:
                    Call(Transaction.ReadCard, CardCallback);
                    break;

                case CallbackCode.STARTCARDSERVICE_FAIL:
                    Logger.Main.Error("读卡器初始化失败");
                    //MainForm.ShowMsg("读卡器初始化失败");
                    //MainForm.Home();
                    break;

                case CallbackCode.READRAWDATA_OK:
                    break;

                case CallbackCode.READRAWDATA_FAIL:
                    break;

                case CallbackCode.EJECTCARD_OK:
                    Call(Transaction.StopCardService, CardCallback);
                    break;

                case CallbackCode.EJECTCARD_FAIL:
                    Call(Transaction.StopCardService, CardCallback);
                    break;

                case CallbackCode.TRACKDATA:
                    try
                    {
                        CardNo = msg.Split(':')[1];
                        if (ValadateAction != null)
                            ValadateAction(CardNo);
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error("读卡失败:" + ex.Message + "\n" + ex.StackTrace);
                    }
                    Call(Transaction.EjectCard, CardCallback);
                    break;
            }
        }

        #region DLL Import

        [DllImport("hatm.dll", EntryPoint = "ATMInit", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ATMInit([MarshalAs(UnmanagedType.FunctionPtr)] AtmCallback callback);

        [DllImport("hatm.dll", EntryPoint = "ATMUnInit", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ATMUnInit();

        [DllImport("hatm.dll", EntryPoint = "ATMCall", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ATMCall(Transaction transaction);

        [DllImport("hatm.dll", EntryPoint = "ATMSetCallback", CallingConvention = CallingConvention.Cdecl)]
        private static extern int ATMSetCallback([MarshalAs(UnmanagedType.FunctionPtr)] AtmCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AtmCallback(CallbackCode callbackCode, int ret, string msg);

        #endregion DLL Import

        #region Enums

        public enum Transaction
        {
            StartCardService = 0x100,
            ReadCard = 0x101,
            EjectCard = 0x102,
            StopCardService = 0x103,
            StartCashServiceStart = 0x300,
            CashInStart = 0x301,
            CashIn = 0x302,
            CashInEnd = 0x303,
            OpenShutter = 0x304,
            CloseShutter = 0x305,
            CashRollback = 0x306,
            StopCashService = 0x307,
            Print = 0x400,
            StopPrint = 0x401
        }

        public enum CallbackCode
        {
            INIT_OK = 0x0001,
            INIT_FAIL = 0x0002,
            STARTCARDSERVICE_OK = 0x9001,
            STARTCARDSERVICE_FAIL = 0x9002,
            READRAWDATA_OK = 0x9003,
            READRAWDATA_FAIL = 0x9004,
            EJECTCARD_OK = 0x9005,
            EJECTCARD_FAIL = 0x9006,
            TRACKDATA = 0x9090,
            STARTCASHSERVICE_OK = 0x8001,
            STARTCASHSERVICE_FAIL = 0x8002,
            CASHINSTART_OK = 0x8003,
            CASHINSTART_FAIL = 0x8004,
            OPENSHUTTER_OK = 0x8005,
            OPENSHUTTER_FAIL = 0x8006,
            CLOSESHUTTER_OK = 0x8007,
            CLOSESHUTTER_FAIL = 0x8008,
            CASHIN_OK = 0x8009,
            CASHIN_FAIL = 0x8010,
            CASHINEND_OK = 0x8011,
            CASHINEND_FAIL = 0x8012,
            CASHINROLLBACK_OK = 0x8013,
            CASHINROLLBACK_FAIL = 0x8014,
            STOPCASHSERVICE_OK = 0x8015,
            STOPCASHSERVICE_FAIL = 0x8016,
            CASH_REFUSE = 0x8017,
            CASH_ITEMSTAKEN = 0x8018,
            CASH_ITEMSINSERTED = 0x8019
        }

        public enum WfsStatDev
        {
            ONLINE = 0,
            OFFLINE = 1,
            POWEROFF = 2,
            NODEVICE = 3,
            HWERROR = 4,
            USERERROR = 5,
            BUSY = 6
        }

        public enum WfsErr
        {
            SUCCESS = 0,
            ALREADY_STARTED = -1,
            API_VER_TOO_HIGH = -2,
            API_VER_TOO_LOW = -3,
            CANCELED = -4,
            CFG_INVALID_HKEY = -5,
            CFG_INVALID_NAME = -6,
            CFG_INVALID_SUBKEY = -7,
            CFG_INVALID_VALUE = -8,
            CFG_KEY_NOT_EMPTY = -9,
            CFG_NAME_TOO_LONG = -10,
            CFG_NO_MORE_ITEMS = -11,
            CFG_VALUE_TOO_LONG = -12,
            DEV_NOT_READY = -13,
            HARDWARE_ERROR = -14,
            INTERNAL_ERROR = -15,
            INVALID_ADDRESS = -16,
            INVALID_APP_HANDLE = -17,
            INVALID_BUFFER = -18,
            INVALID_CATEGORY = -19,
            INVALID_COMMAND = -20,
            INVALID_EVENT_CLASS = -21,
            INVALID_HSERVICE = -22,
            INVALID_HPROVIDER = -23,
            INVALID_HWND = -24,
            INVALID_HWNDREG = -25,
            INVALID_POINTER = -26,
            INVALID_REQ_ID = -27,
            INVALID_RESULT = -28,
            INVALID_SERVPROV = -29,
            INVALID_TIMER = -30,
            INVALID_TRACELEVEL = -31,
            LOCKED = -32,
            NO_BLOCKING_CALL = -33,
            NO_SERVPROV = -34,
            NO_SUCH_THREAD = -35,
            NO_TIMER = -36,
            NOT_LOCKED = -37,
            NOT_OK_TO_UNLOAD = -38,
            NOT_STARTED = -39,
            NOT_REGISTERED = -40,
            OP_IN_PROGRESS = -41,
            OUT_OF_MEMORY = -42,
            SERVICE_NOT_FOUND = -43,
            SPI_VER_TOO_HIGH = -44,
            SPI_VER_TOO_LOW = -45,
            SRVC_VER_TOO_HIGH = -46,
            SRVC_VER_TOO_LOW = -47,
            TIMEOUT = -48,
            UNSUPP_CATEGORY = -49,
            UNSUPP_COMMAND = -50,
            VERSION_ERROR_IN_SRVC = -51,
            INVALID_DATA = -52,
            SOFTWARE_ERROR = -53,
            CONNECTION_LOST = -54,
            USER_ERROR = -55,
            UNSUPP_DATA = -56
        }

        public enum WfsCim
        {
            SRVE_SAFEDOOROPEN = 1301,
            SRVE_SAFEDOORCLOSED = 1302,
            USRE_CASHUNITTHRESHOLD = 1303,
            SRVE_CASHUNITINFOCHANGED = 1304,
            SRVE_TELLERINFOCHANGED = 1305,
            EXEE_CASHUNITERROR = 1306,
            SRVE_ITEMSTAKEN = 1307,
            SRVE_COUNTS_CHANGED = 1308,
            EXEE_INPUTREFUSE = 1309,
            SRVE_ITEMSPRESENTED = 1310,
            SRVE_ITEMSINSERTED = 1311,
            EXEE_NOTEERROR = 1312,
            EXEE_SUBCASHIN = 1313,
            SRVE_MEDIADETECTED = 1314,
            EXEE_INPUT_P6 = 1315
        }

        #endregion Enums
    }
}
