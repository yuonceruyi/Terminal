using System;
using System.Runtime.InteropServices;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Devices.PrinterCheck
{
    public class WindowsDriverPrinter
    {
        #region WindowsDriver

    
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetPrinter(IntPtr hPrinter, uint dwLevel, IntPtr pPrinter, uint dwBuf,
            ref uint dwNeeded);

        [StructLayout(LayoutKind.Sequential)]
        public struct PRINTER_DEFAULTS
        {
            public readonly IntPtr pDatatype;
            public readonly IntPtr pDevMode;
            public readonly int DesiredAccess;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PRINTER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pServerName;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pPrinterName;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pShareName;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pPortName;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pDriverName;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pComment;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pLocation;

            public readonly IntPtr pDevMode;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pSepFile;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pPrintProcessor;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pDatatype;

            [MarshalAs(UnmanagedType.LPTStr)] public readonly string pParameters;

            public readonly IntPtr pSecurityDescriptor;
            public readonly uint Attributes; // See note below!
            public readonly uint Priority;
            public readonly uint DefaultPriority;
            public readonly uint StartTime;
            public readonly uint UntilTime;
            public readonly uint Status;
            public readonly uint cJobs;
            public readonly uint AveragePPM;
        }

        [Flags]
        public enum PrinterStatus
        {
            PRINTER_STATUS_DOOR_OPEN = 0x00400000,
            PRINTER_STATUS_NOT_AVAILABLE = 0x00001000,
            PRINTER_STATUS_OFFLINE = 0x00000080,
            PRINTER_STATUS_PAPER_JAM = 0x00000008,
            PRINTER_STATUS_PAPER_OUT = 0x00000010,
            PRINTER_STATUS_PRINTING = 0x00000400,
            PRINTER_STATUS_USER_INTERVENTION = 0x00100000
        }

        #endregion WindowsDriver
    }
}