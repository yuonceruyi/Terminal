using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.Devices.PrinterCheck.MsPrinterCheck
{
    public class MsPrinter
    {
        private const string DllPathMs = "External\\MsPrinter\\Msprintdyn.dll";
        public static string PrinterSatusMsg { get; set; }
        //private const string DllPathMs = "Msprintdyn.dll";

        [DllImport(DllPathMs, EntryPoint = "SetPrintNameRM", CharSet = CharSet.Ansi)]
        public static extern void SetPrintNameRM(StringBuilder cName);

        [DllImport(DllPathMs, EntryPoint = "SetBaudrateRM", CharSet = CharSet.Ansi)]
        public static extern void SetBaudrateRM(int int_Rate);

        [DllImport(DllPathMs, EntryPoint = "SetCOMPortRM", CharSet = CharSet.Ansi)]
        public static extern void SetCOMPortRM(int int_Port);

        [DllImport(DllPathMs, EntryPoint = "GetPrintStatusRM", CharSet = CharSet.Ansi)]
        public static extern int GetPrintStatusRM();

        [DllImport(DllPathMs, EntryPoint = "SetPrintNameUSB", CharSet = CharSet.Ansi)]
        public static extern void SetPrintNameUSB(StringBuilder cName);

        [DllImport(DllPathMs, EntryPoint = "GetPrintStatusUSB", CharSet = CharSet.Ansi)]
        public static extern int GetPrintStatusUSB();

        [DllImport(DllPathMs, EntryPoint = "CleanRMBuffer", CharSet = CharSet.Ansi)]
        public static extern int CleanRMBuffer();

        [DllImport(DllPathMs, EntryPoint = "CleanUSBBuffer", CharSet = CharSet.Ansi)]
        public static extern int CleanUSBBuffer();

        [DllImport(DllPathMs, EntryPoint = "SetPrintModel", CharSet = CharSet.Ansi)]
        public static extern void SetPrintModel(int int_PrintModel);

        public static string GetStatusMsg(int nStatus)
        {
            var message = "位置错误";
            switch (nStatus)
            {
                case 0:
                    message = "正常";
                    break;

                case 1:
                    message = "打印机未连接或未上电";
                    break;

                case 2:
                    message = "打印机和查状态控件不匹配";
                    break;

                case 3:
                    message = "主板系统错误";
                    break;

                case 4:
                    message = "打印头打开";
                    break;

                case 5:
                    message = "黑标错误或热敏片过热";
                    break;

                case 6:
                    message = "切刀错误";
                    break;

                case 7:
                    message = "纸尽";
                    break;

                case 8:
                    message = "堵纸";
                    break;

                case 9:
                    message = "打印机忙";
                    break;

                case 10:
                    message = "纸将尽";
                    break;

                case 11:
                    message = "出口纸检测";
                    break;

                case 12:
                    message = "容纸器错误";
                    break;

                case 100:
                    message = "未连接或未上电 ( 无返回值或打印机无回应 ) ";
                    break;
            }
            return message;
        }

      
    }
}