using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using System.Threading;
using YuanTu.Core.Log;

namespace YuanTu.Devices.PrinterCheck.BrotherPrinterCheck
{
    public class BrotherPrinter
    {
        private const string _dllPath = "External\\Brother\\brUsbmon.dll";
        #region RequestStr

        private string PJLTIMEOUTSLEEP = "\x001B%-12345X@PJL\r\n@PJL DEFAULT TIMEOUTSLEEP={0}\r\n\x001B%-12345X";
        private const string OPENPRINTERFAILED = "Open Printer Failed";
        private const string CLOSEPRINTERFAILED = "Close Printer Failed";
        private const string WRITEPJLFAILED = "Write PJL Failed";
        private const string READFAILED = "Read From Printer Failed";
        private const string SEARCHFAILED = "SearchFailed";
        private const string ESC = "\r\n";
        private const string START = "START";
        private const string CANCEL = "CANCELED";
        private const string END = "END";
        private const string Printing = "10023";
        private const string PleaseWait = "10003";
        private const string PJLRemainToner = "\x001B%-12345X@PJL\r\n@PJL DINQUIRE REMAINTONER\r\n\x001B%-12345X";
        private const string RemainToner = "PJL DINQUIRE REMAINTONER";
        private const string PJLExpectedDrumLife = "\x001B%-12345X@PJL\r\n@PJL DINQUIRE NEXTCAREDRM\r\n\x001B%-12345X";
        private const string ExpectedDrumLife = "PJL DINQUIRE NEXTCAREDRM";
        private const string PJLPrintedPages = "\x001B%-12345X@PJL\r\n@PJL INFO PAGES\r\n\x001B%-12345X";
        private const string PrintedPages = "PJL INFO PAGES";
        private const string PJLPrinterStatus = "\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X";
        private const string PrinterStatus = "PJL INFO STATUS";
        private const string PJLPrinterStatusChangeNoticeOn = "\x001B%-12345X@PJL\r\n@PJL USTATUS DEVICE=ON\r\n\x001B%-12345X";
        private const string PJLPrinterStatusChangeNoticeOff = "\x001B%-12345X@PJL\r\n@PJL USTATUS DEVICE=OFF\r\n\x001B%-12345X";
        private const string PrinterStatusDevice = "PJL USTATUS DEVICE";
        private const string PJLPageStatusChangeNoticeOn = "\x001B%-12345X@PJL\r\n@PJL USTATUS PAGE=ON\r\n\x001B%-12345X";
        private const string PJLPageStatusChangeNoticeOff = "\x001B%-12345X@PJL\r\n@PJL USTATUS PAGE=OFF\r\n\x001B%-12345X";
        private const string PageStatus = "PJL USTATUS PAGE";
        private const string PJLJobStatusChangeNoticeOn = "\x001B%-12345X@PJL\r\n@PJL USTATUS JOB=ON\r\n\x001B%-12345X";
        private const string PJLJobStatusChangeNoticeOff = "\x001B%-12345X@PJL\r\n@PJL USTATUS JOB=OFF\r\n\x001B%-12345X";
        private const string JobStatus = "PJL USTATUS JOB";
        private const string PJLAutoSleepON = "\x001B%-12345X@PJL\r\n@PJL DEFAULT AUTOSLEEP=ON\r\n\x001B%-12345X";
        private const string PJLAutoSleepOFF = "\x001B%-12345X@PJL\r\n@PJL DEFAULT AUTOSLEEP=OFF\r\n\x001B%-12345X";
        private const string tray1 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=TRAY1\r\n\x001B%-12345X";
        private const string tray2 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=TRAY2\r\n\x001B%-12345X";
        private const string MPTray = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=MPTRAY\r\n\x001B%-12345X";
        private const string autoTray = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY =AUTO/REMAINING\r\n\x001B%-12345X";
        #endregion
        private string _printerName;
        private int port;

        public BrotherPrinter(string printerName)
        {
            this._printerName = printerName;
            this.port = -1;
        }

        [DllImport(_dllPath, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int brOpenLMPrinterPort([MarshalAs(UnmanagedType.LPStr)] string printer);

        [DllImport(_dllPath, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool brCloseLMPrinter(int port);

        [DllImport(_dllPath, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool brReadPort(int hport, byte[] buffer, int dwSize, ref uint readSize);

        [DllImport(_dllPath, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool brWritePort(int hport, byte[] buffer, int dwSize, ref uint readSize);

        public Result GetPrinterStatus()
        {
            try
            {
                if (!File.Exists(_dllPath))
                {
                    return Result.Fail($"缺少动态库:{_dllPath}");
                }
                this.OpenPrinter();
                if (this.port == -1)
                {
                    return Result.Fail("打开连接失败");
                }
                string fullString = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X", "PJL INFO STATUS");
                Logger.Device.Info("兄弟激光打印机:" + fullString);
                this.ClosePritner();
                int p = fullString.IndexOf("=", StringComparison.Ordinal);
                var statusString = p >= 0 ? fullString.Substring(p + 1, 5) : fullString;

                int status = int.Parse(statusString);
                var statusEnum = (BrotherStatusEnum)status;
                var result = new Result { ResultCode = status };
                if (statusEnum == BrotherStatusEnum.准备
                    || statusEnum == BrotherStatusEnum.睡眠
                    || statusEnum == BrotherStatusEnum.墨粉不足
                    || statusEnum == BrotherStatusEnum.正在打印
                )
                    result.IsSuccess = true;
                else
                    result.Message = statusEnum.ToString();
                return result;
            }
            catch (Exception e)
            {
                return Result.Fail(e.ToString(), e);
            }
        }

        private void OpenPrinter()
        {
            if (this.port != -1)
                return;
            this.port = brOpenLMPrinterPort(this._printerName);
            if (this.port != -1)
                return;
            Logger.Device.Info("兄弟激光打印机打开连接失败");
        }

        private void ClosePritner()
        {
            if (this.port == -1)
                return;
            brCloseLMPrinter(this.port);
            if (this.port == -1)
                Logger.Device.Info("兄弟激光打印机关闭连接失败");
            else
                this.port = -1;
        }

        private string QueryPrinter(string pjl, string except)
        {
            string empty = string.Empty;
            if (!this.WritePortEx(pjl))
                return "Write PJL Failed";
            int num = 0;
            while (++num <= 5)
            {
                Thread.Sleep(1000);
                if (this.ReadPortEx(ref empty) && empty.Contains(except))
                    return empty;
            }
            return "SearchFailed";
        }

        #region UnuseMethods

        public string GetRemainToner()
        {
            string str = string.Empty;
            this.OpenPrinter();
            if (this.port != -1)
            {
                str = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL DINQUIRE REMAINTONER\r\n\x001B%-12345X", "PJL DINQUIRE REMAINTONER");
                this.ClosePritner();
                int num = str.IndexOf("\r\n", StringComparison.Ordinal);
                if (num != -1)
                {
                    try
                    {
                        str = int.Parse(str.Substring(num + 2)).ToString();
                    }
                    catch
                    {
                        str = "SearchFailed";
                    }
                }
            }
            else if (this.port == -1)
                return "Open Printer Failed";
            return str;
        }

        public string GetExpectedDrumLife()
        {
            this.OpenPrinter();
            if (this.port == -1)
                return "Open Printer Failed";
            string str = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL DINQUIRE NEXTCAREDRM\r\n\x001B%-12345X", "PJL DINQUIRE NEXTCAREDRM");
            this.ClosePritner();
            int num = str.IndexOf("\r\n", StringComparison.Ordinal);
            if (num != -1)
            {
                string s = str.Substring(num + 2);
                try
                {
                    str = int.Parse(s).ToString();
                }
                catch
                {
                    str = "SearchFailed";
                }
            }
            return str;
        }

        public string GetPrintedPages()
        {
            this.OpenPrinter();
            if (this.port == -1)
                return "Open Printer Failed";
            string str = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL INFO PAGES\r\n\x001B%-12345X", "PJL INFO PAGES");
            this.ClosePritner();
            int num1 = str.IndexOf("\r\n", StringComparison.Ordinal);
            int num2 = str.LastIndexOf("\r\n", StringComparison.Ordinal);
            if (num1 != -1 && num2 != -1 && num2 > num1)
                str = str.Substring(num1 + 2, num2 - num1);
            return str;
        }

        public int GetPrintedPagesCount()
        {
            int num1 = 0;
            this.OpenPrinter();
            if (this.port == -1)
                return num1;
            string str = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL INFO PAGES\r\n\x001B%-12345X", "PJL INFO PAGES");
            this.ClosePritner();
            if (str.Equals(""))
                return num1;
            char[] charArray = str.ToCharArray();
            int num2 = 0;
            bool flag = false;
            for (int index = 0; index < charArray.Length; ++index)
            {
                if (flag)
                {
                    if (48 <= (int)charArray[index] && 57 >= (int)charArray[index])
                    {
                        num2 = num2 * 10 + ((int)charArray[index] - 48);
                    }
                    else
                    {
                        num1 += num2;
                        num2 = 0;
                        flag = false;
                    }
                    if (charArray.Length == index + 1)
                        num1 += num2;
                }
                if (32 == (int)charArray[index] || 9 == (int)charArray[index])
                    flag = true;
                else if (48 > (int)charArray[index] || 57 < (int)charArray[index])
                    flag = false;
            }
            return num1;
        }

        public string GetPrinterStatusDevice()
        {
            string empty = string.Empty;
            this.OpenPrinter();
            if (this.port == -1)
                return "Open Printer Failed";
            string str = this.QueryPrinter("\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X", "PJL INFO STATUS");
            this.ClosePritner();
            if (str.IndexOf("\r\n") != -1)
            {
                try
                {
                    str = str.Substring(17);
                }
                catch
                {
                    str = "Read From Printer Failed";
                }
            }
            return str;
        }

        private bool IsStart()
        {
            if (this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X"))
            {
                string empty = string.Empty;
                int num = 0;
                while (++num <= 5)
                {
                    if (this.ReadPortEx(ref empty) && (empty.Contains("10023") || empty.Contains("10003")))
                    {
                        this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL USTATUS PAGE=ON\r\n\x001B%-12345X");
                        return true;
                    }
                    Thread.Sleep(500);
                }
            }
            return false;
        }

        public bool StartGetPageProgress()
        {
            bool flag = false;
            this.OpenPrinter();
            if (this.port != -1)
            {
                flag = this.IsStart();
                if (!flag)
                    this.ClosePritner();
            }
            return flag;
        }

        private int GetPageNo(string readFromPrinter)
        {
            string empty = string.Empty;
            string[] separator = new string[1] { "\r\n" };
            string[] strArray = readFromPrinter.Split(separator, StringSplitOptions.None);
            int length = strArray.Length;
            if (length == 2)
                empty = strArray[1];
            else if (length > 2)
            {
                for (int index = 0; index < length - 1; ++index)
                {
                    if (strArray[index].Contains("PJL USTATUS PAGE"))
                    {
                        empty = strArray[index + 1];
                        break;
                    }
                }
            }
            try
            {
                return int.Parse(empty);
            }
            catch
            {
                return -1;
            }
        }

        public int GetPrintingPageNo()
        {
            string empty = string.Empty;
            while (true)
            {
                this.ReadPortEx(ref empty);
                if (!empty.Contains("PJL USTATUS PAGE"))
                {
                    if (empty.Contains("PJL INFO STATUS"))
                    {
                        if (!empty.Contains("10023") && !empty.Contains("10003"))
                            goto label_5;
                    }
                    else if (empty == string.Empty)
                        this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X");
                    Thread.Sleep(500);
                }
                else
                    break;
            }
            return this.GetPageNo(empty);
            label_5:
            this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL USTATUS PAGE=OFF\r\n\x001B%-12345X");
            this.ClosePritner();
            return -1;
        }

        public int GetJobStatus(ref string jobStatus, ref string jobName, ref string jobPages)
        {
            string empty = string.Empty;
            if (this.port == -1)
            {
                this.OpenPrinter();
                if (this.port != -1)
                {
                    this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL USTATUS JOB=ON\r\n\x001B%-12345X");
                    Thread.Sleep(5000);
                }
                else if (this.port == -1)
                    return -1;
            }
            while (true)
            {
                this.ReadPortEx(ref empty);
                if (!empty.Contains("PJL USTATUS JOB"))
                {
                    if (empty.Contains("PJL INFO STATUS"))
                    {
                        if (!empty.Contains("10023") && !empty.Contains("10003"))
                            goto label_13;
                    }
                    else if (empty == string.Empty)
                        this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL INFO STATUS\r\n\x001B%-12345X");
                    Thread.Sleep(500);
                }
                else
                    break;
            }
            int num;
            if (empty.Contains("START\r\n"))
            {
                jobStatus = "START";
                this.GetJobNameAndPages(empty, ref jobName, ref jobPages);
                num = 0;
                goto label_17;
            }
            else if (empty.Contains("END\r\n"))
            {
                jobStatus = "END";
                this.GetJobNameAndPages(empty, ref jobName, ref jobPages);
                num = 2;
                goto label_17;
            }
            else
            {
                jobStatus = string.Empty;
                jobName = string.Empty;
                jobPages = string.Empty;
                num = 1;
                goto label_17;
            }
            label_13:
            num = 3;
            label_17:
            if (num == 3 || num == 2 || num == 1)
            {
                this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL USTATUS JOB=OFF\r\n\x001B%-12345X");
                this.ClosePritner();
            }
            return num;
        }

        private void GetJobNameAndPages(string readFromPrinter, ref string jobName, ref string jobPages)
        {
            string[] separator = new string[1] { "\r\n" };
            foreach (string str in readFromPrinter.Split(separator, StringSplitOptions.None))
            {
                if (str.StartsWith("NAME="))
                    jobName = str;
                else if (str.StartsWith("PAGES="))
                    jobPages = str;
            }
        }

        public bool SetTimeOutSleep(uint timeout)
        {
            string pjl = string.Format(this.PJLTIMEOUTSLEEP, (object)timeout);
            this.OpenPrinter();
            if (this.port == -1)
                return false;
            if (!this.WritePortEx(pjl))
            {
                this.ClosePritner();
                return false;
            }
            this.ClosePritner();
            return true;
        }

        public bool AutoSleepOn()
        {
            bool flag = false;
            this.OpenPrinter();
            if (this.port == -1)
                return false;
            if (this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL DEFAULT AUTOSLEEP=ON\r\n\x001B%-12345X"))
                flag = true;
            this.ClosePritner();
            return flag;
        }

        public bool AutoSleepOff()
        {
            bool flag = false;
            this.OpenPrinter();
            if (this.port == -1)
                return false;
            if (this.WritePortEx("\x001B%-12345X@PJL\r\n@PJL DEFAULT AUTOSLEEP=OFF\r\n\x001B%-12345X"))
                flag = true;
            this.ClosePritner();
            return flag;
        }

        private bool WritePortEx(string pjl)
        {
            uint readSize = 0;
            Logger.Device.Info($"兄弟激光打印机写信息:{pjl}");
            byte[] bytes = Encoding.ASCII.GetBytes(pjl + "\r\n");
            bool flag = brWritePort(this.port, bytes, bytes.Length, ref readSize);
            if (!flag)
                Logger.Device.Info($"兄弟激光打印机写信息失败:{pjl}");
            return flag;
        }

        private bool ReadPortEx(ref string readFromPrinter)
        {
            bool flag = false;
            byte[] numArray = new byte[1000];
            uint readSize = 0;
            readFromPrinter = string.Empty;
            if (brReadPort(this.port, numArray, 1000, ref readSize))
            {
                readFromPrinter = Encoding.ASCII.GetString(numArray);
                int num = readFromPrinter.LastIndexOf("\r\n", StringComparison.Ordinal);
                if (num == -1)
                {
                    readFromPrinter = string.Empty;
                    Logger.Device.Info("兄弟激光打印机读信息为空");
                }
                else
                {
                    readFromPrinter = readFromPrinter.Substring(1, num - 1);
                    flag = true;
                    Logger.Device.Info($"兄弟激光打印机读信息:{readFromPrinter}");
                }
            }
            return flag;
        }

        public bool SetTray(TrayType trayType)
        {
            string trayType1 = string.Empty;
            switch (trayType)
            {
                case TrayType.AutoType:
                    trayType1 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY =AUTO/REMAINING\r\n\x001B%-12345X";
                    break;
                case TrayType.Type1:
                    trayType1 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=TRAY1\r\n\x001B%-12345X";
                    break;
                case TrayType.Type2:
                    trayType1 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=TRAY2\r\n\x001B%-12345X";
                    break;
                case TrayType.MpType:
                    trayType1 = "\x001B%-12345X@PJL\r\n@PJL DEFAULT SOURCETRAY=MPTRAY\r\n\x001B%-12345X";
                    break;
            }
            return this.SetPaperTray(trayType1);
        }

        private bool SetPaperTray(string trayType)
        {
            bool flag = false;
            this.OpenPrinter();
            if (this.port != -1)
            {
                flag = this.WritePortEx(trayType);
                this.ClosePritner();
            }
            return flag;
        }

        #endregion
    }

    public enum TrayType
    {
        AutoType,
        Type1,
        Type2,
        MpType,
    }

    public enum MessageType
    {
        Unknown,
        Information,
        Warning,
        Error,
        Success,
    }
}
