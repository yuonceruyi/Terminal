using System.Runtime.InteropServices;

namespace YuanTu.Devices.PrinterCheck.CePrinterCheck
{
	internal static class IntercomModule
	{
        // 
        [DllImport("CeSmLm.dll", EntryPoint = "CePrnInitCeUsbSI", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnInitUsb(ref uint lpdwSysError);
		// 
        [DllImport("CeSmLm.dll", EntryPoint = "CePrnGetInterfaceNumUsb", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnGetInterfaceNumUsb([MarshalAs(UnmanagedType.LPStr)]string prnName, ref uint lpdwSysError);
		// 
        [DllImport("CeSmLm.dll", EntryPoint = "CePrnGetFullModelUsb", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnGetFullModelUsb(int prnDevNum, ref uint prnModel, ref uint lpdwSysError);
		// 
        [DllImport("CeSmLm.dll", EntryPoint = "SetUsbPrinterModelGSI", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint SetUsbPrinterModelUsb(int prnDevNum, uint prnModel);
		// 
        [DllImport("CeSmLm.dll", EntryPoint = "CePrnGetStsUsb", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnGetStsUsb(int prnDevNum, ref uint prnStatus, ref uint lpdwSysError);

        [DllImport("CeSmLm.dll", EntryPoint = "CePrnGetNumTotCutUsb", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnGetNumCutUsb(int prnDevNum, ref uint prnStatus, ref uint lpdwSysError);

        [DllImport("CeSmLm.dll", EntryPoint = "CePrnGetTotPaperRemainingUsb", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern uint CePrnGetTotPaperRemaingUsb(int prnDevNum, ref uint prnStatus, ref uint lpdwSysError);

    }
} //end of root namespace