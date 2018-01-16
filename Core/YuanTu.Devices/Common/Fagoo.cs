using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.Devices.Common
{
    public class HitiCardPrintParameter
    {
        public byte ByOrientation;

        public byte ByCardThickness;

        public byte ByTransparentCard;

        public Bitmap FrontBgr;

        public Bitmap FrontK;

        public Bitmap FrontO;

        public Bitmap BackBgr;

        public Bitmap BackK;

        public Bitmap BackO;
    }

    public class HitiHeatingEnergy
    {
        public sbyte ChFrontDenYmc;

        public sbyte ChFrontDenK;

        public sbyte ChFrontDenO;

        public sbyte ChFrontDenResinK;

        public sbyte ChBackDenYmc;

        public sbyte ChBackDenK;

        public sbyte ChBackDenO;

        public sbyte ChBackDenResinK;

        public sbyte ChFrontSenYmc;

        public sbyte ChFrontSenK;

        public sbyte ChFrontSenO;

        public sbyte ChFrontSenResinK;

        public sbyte ChBackSenYmc;

        public sbyte ChBackSenK;

        public sbyte ChBackSenO;

        public sbyte ChBackSenResinK;
    }

    public struct MagTrackData2
    {
        public byte[] SzTrack1;

        public byte[] SzTrack2;

        public byte[] SzTrack3;

        public byte ByTrackFlag;

        public byte ByEncodeMode;

        public byte ByCoercivity;

        public byte ByT2Bpi;

        public byte ByRawLenT1;

        public byte ByRawLenT2;

        public byte ByRawLenT3;
    }

    public class PavoJobProperty
    {
        public uint DwCardType;

        public uint DwFlags;

        public uint DwDataFlag;

        public IntPtr HParentWnd;

        public short ShOrientation;

        public short ShCopies;

        public uint DwCustomIndex;

        public uint DwFieldFlag;

        public byte ByTransparentCard;

        public byte ByFlip;

        public byte ByCardThick;

        public byte ByDuplex;

        public byte ByRibbonType;

        public byte ByPrintColor;

        public byte ByDitherK;

        public byte ByRotate180;

        public byte ByPrintAs230E;
    }

    public class PavoApi
    {
        private const string DllPathFagoo = "External\\Fagoo\\PavoApi.dll";

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_ApplyJobSettingW(string szPrinter, IntPtr hDc, IntPtr lpInDevMode, ref PavoApi._PAVO_JOB_PROPERTY lpInJobProp);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_CheckPrinterStatusW(string szPrinter, ref uint lpdwStatus);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_DoCommandW(string szPrinter, uint dwCommand);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_FindComPortW(string szPrinter, ref uint lpdwMagPort, ref uint lpdwRfPort);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_FindSCardReaderW(string szPrinter, StringBuilder szReaderNameW);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_GetDeviceInfoW(string szPrinter, uint dwInfoType, byte[] lpInfoData, ref uint lpdwDataLen);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_MoveCardW(string szPrinter, uint dwPosition);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_MoveCard3W(string szPrinter, uint dwPosition);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_SetExtraDataToHDC(IntPtr hDc, uint dwType, uint x, uint y, ref PavoApi.BITMAP lpBmp);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_PrintOneCardW(string szPrinter, ref PavoApi._HITI_CARD_PRINT_PARAMETER lpJobPara, ref PavoApi._HITI_HEATING_ENERGY lpHeatEnergy, IntPtr lpReserved);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_ReadMagTrackDataW(string szPrinter, uint dwCom, ref PavoApi._MAG_TRACK_DATA2 lpTrackData);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_WriteMagTrackDataW(string szPrinter, uint dwCom, ref PavoApi._MAG_TRACK_DATA2 lpTrackData);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_SetPasswordW(string szPrinter, string szCurrentPasswd, string szNewPasswdW);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_SetSecurityModeW(string szPrinter, string szCurrentPasswd, int nSecurityMode);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_EnumUSBCardPrintersA(byte[] szPrinter, ref uint nBufferSize);

        [DllImport(DllPathFagoo, CharSet = CharSet.Unicode)]
        private static extern uint PAVO_EraseCardAreaW(string szPrinter, int nLeft, int nTop, int nRight, int bottom);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        public static uint PAVO_ApplyJobSetting(PrinterSettings printerSettings, ref PavoJobProperty jobProp)
        {
            string printerName = printerSettings.PrinterName;
            PavoApi._PAVO_JOB_PROPERTY pAvoJobProperty = default(PavoApi._PAVO_JOB_PROPERTY);
            pAvoJobProperty.DwSize = (uint)Marshal.SizeOf(typeof(PavoApi._PAVO_JOB_PROPERTY));
            pAvoJobProperty.DwCardType = jobProp.DwCardType;
            pAvoJobProperty.DwFlags = jobProp.DwFlags;
            pAvoJobProperty.DwDataFlag = jobProp.DwDataFlag;
            pAvoJobProperty.HParentWnd = jobProp.HParentWnd;
            pAvoJobProperty.HReserved1 = IntPtr.Zero;
            pAvoJobProperty.PReserved1 = null;
            pAvoJobProperty.PReserved2 = null;
            pAvoJobProperty.PReserved3 = null;
            pAvoJobProperty.PReserved4 = null;
            pAvoJobProperty.ShOrientation = jobProp.ShOrientation;
            pAvoJobProperty.ShCopies = jobProp.ShCopies;
            pAvoJobProperty.ShReserved1 = 0;
            pAvoJobProperty.ShReserved2 = 0;
            pAvoJobProperty.DwCustomIndex = jobProp.DwCustomIndex;
            pAvoJobProperty.DwFieldFlag = jobProp.DwFieldFlag;
            pAvoJobProperty.DwReserved3 = 0u;
            pAvoJobProperty.DwReserved4 = 0u;
            pAvoJobProperty.WReserved1 = 0;
            pAvoJobProperty.BReserved1 = 0;
            pAvoJobProperty.ByPrintAs230E = jobProp.ByPrintAs230E;
            pAvoJobProperty.ByTransparentCard = jobProp.ByTransparentCard;
            pAvoJobProperty.ByFlip = jobProp.ByFlip;
            pAvoJobProperty.ByReserved1 = 0;
            pAvoJobProperty.ByCardThick = jobProp.ByCardThick;
            pAvoJobProperty.ByDuplex = jobProp.ByDuplex;
            pAvoJobProperty.ByRibbonType = jobProp.ByRibbonType;
            pAvoJobProperty.ByPrintColor = jobProp.ByPrintColor;
            pAvoJobProperty.ByDitherK = jobProp.ByDitherK;
            pAvoJobProperty.ByReserved5 = 0;
            pAvoJobProperty.ByReserved6 = 0;
            pAvoJobProperty.ByReserved7 = 0;
            pAvoJobProperty.ByRotate180 = jobProp.ByRotate180;
            IntPtr hdevmode = printerSettings.GetHdevmode();
            IntPtr lpInDevMode = PavoApi.GlobalLock(hdevmode);
            uint result = PavoApi.PAVO_ApplyJobSettingW(printerName, IntPtr.Zero, lpInDevMode, ref pAvoJobProperty);
            printerSettings.SetHdevmode(hdevmode);
            PavoApi.GlobalFree(hdevmode);
            return result;
        }

        public static uint PAVO_CheckPrinterStatus(string szPrinter, ref uint dwStatus)
        {
            return PavoApi.PAVO_CheckPrinterStatusW(szPrinter, ref dwStatus);
        }

        public static uint PAVO_DoCommand(string szPrinter, uint dwCommand)
        {
            return PavoApi.PAVO_DoCommandW(szPrinter, dwCommand);
        }

        public static uint PAVO_FindComPort(string szPrinter, ref uint dwMagPort, ref uint dwRfPort)
        {
            uint num = 0u;
            uint num2 = 0u;
            uint result = PavoApi.PAVO_FindComPortW(szPrinter, ref num, ref num2);
            dwMagPort = num;
            dwRfPort = num2;
            return result;
        }

        public static uint PAVO_FindSCardReader(string szPrinter, StringBuilder szReaderName)
        {
            return PavoApi.PAVO_FindSCardReaderW(szPrinter, szReaderName);
        }

        public static uint PAVO_GetDeviceInfo(string szPrinter, uint dwInfoType, byte[] lpInfoData, ref uint dwDataLen)
        {
            return PavoApi.PAVO_GetDeviceInfoW(szPrinter, dwInfoType, lpInfoData, ref dwDataLen);
        }

        public static uint PAVO_MoveCard(string szPrinter, uint dwPosition)
        {
            return PavoApi.PAVO_MoveCardW(szPrinter, dwPosition);
        }

        public static uint PAVO_MoveCard3(string szPrinter, uint dwPosition)
        {
            return PavoApi.PAVO_MoveCard3W(szPrinter, dwPosition);
        }

        public static uint PAVO_SetExtraDataToHDC(IntPtr hDc, uint dwType, Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            PavoApi.BITMAP bItmap;
            bItmap.BmType = 21072;
            bItmap.BmWidth = bitmapData.Width;
            bItmap.BmHeight = bitmapData.Height;
            bItmap.BmWidthBytes = bitmapData.Stride;
            bItmap.BmPlanes = 1;
            bItmap.BmBitsPixel = 8;
            bItmap.BmBits = bitmapData.Scan0;
            uint result = PavoApi.PAVO_SetExtraDataToHDC(hDc, dwType, 0u, 0u, ref bItmap);
            bmp.UnlockBits(bitmapData);
            return result;
        }

        public static uint PAVO_PrintOneCard(string szPrinter, HitiCardPrintParameter jobPara, HitiHeatingEnergy heatEnergy)
        {
            PavoApi._HITI_HEATING_ENERGY hItiHeatingEnergy;
            hItiHeatingEnergy.ChFrontDenYmc = heatEnergy.ChFrontDenYmc;
            hItiHeatingEnergy.ChFrontDenK = heatEnergy.ChFrontDenK;
            hItiHeatingEnergy.ChFrontDenO = heatEnergy.ChFrontDenO;
            hItiHeatingEnergy.ChFrontDenResinK = heatEnergy.ChFrontDenResinK;
            hItiHeatingEnergy.ChBackDenYmc = heatEnergy.ChBackDenYmc;
            hItiHeatingEnergy.ChBackDenK = heatEnergy.ChBackDenK;
            hItiHeatingEnergy.ChBackDenO = heatEnergy.ChBackDenO;
            hItiHeatingEnergy.ChBackDenResinK = heatEnergy.ChBackDenResinK;
            hItiHeatingEnergy.ChFrontSenYmc = heatEnergy.ChFrontSenYmc;
            hItiHeatingEnergy.ChFrontSenK = heatEnergy.ChFrontSenK;
            hItiHeatingEnergy.ChFrontSenO = heatEnergy.ChFrontSenO;
            hItiHeatingEnergy.ChFrontSenResinK = heatEnergy.ChFrontSenResinK;
            hItiHeatingEnergy.ChBackSenYmc = heatEnergy.ChBackSenYmc;
            hItiHeatingEnergy.ChBackSenK = heatEnergy.ChBackSenK;
            hItiHeatingEnergy.ChBackSenO = heatEnergy.ChBackSenO;
            hItiHeatingEnergy.ChBackSenResinK = heatEnergy.ChBackSenResinK;
            IntPtr intPtr = IntPtr.Zero;
            BitmapData bitmapData = null;
            BitmapData bitmapData2 = null;
            BitmapData bitmapData3 = null;
            BitmapData bitmapData4 = null;
            BitmapData bitmapData5 = null;
            BitmapData bitmapData6 = null;
            PavoApi._HITI_CARD_PRINT_PARAMETER hItiCardPrintParameter;
            hItiCardPrintParameter.DwSize = (uint)Marshal.SizeOf(typeof(PavoApi._HITI_CARD_PRINT_PARAMETER));
            hItiCardPrintParameter.DwReserve1 = 0u;
            hItiCardPrintParameter.DwReserve2 = 0u;
            hItiCardPrintParameter.DwFlags = 0u;
            hItiCardPrintParameter.ByOrientation = jobPara.ByOrientation;
            hItiCardPrintParameter.ByCardThickness = jobPara.ByCardThickness;
            hItiCardPrintParameter.ByTransparentCard = jobPara.ByTransparentCard;
            hItiCardPrintParameter.ByReserve4 = 0;
            hItiCardPrintParameter.ByReserve5 = 0;
            hItiCardPrintParameter.ByReserve6 = 0;
            hItiCardPrintParameter.ByReserve7 = 0;
            hItiCardPrintParameter.ByReserve8 = 0;
            hItiCardPrintParameter.LpFrontBgr = IntPtr.Zero;
            hItiCardPrintParameter.LpFrontK = IntPtr.Zero;
            hItiCardPrintParameter.LpFrontO = IntPtr.Zero;
            hItiCardPrintParameter.LpBackBgr = IntPtr.Zero;
            hItiCardPrintParameter.LpBackK = IntPtr.Zero;
            hItiCardPrintParameter.LpBackO = IntPtr.Zero;
            hItiCardPrintParameter.LpReserve1 = IntPtr.Zero;
            hItiCardPrintParameter.LpReserve2 = IntPtr.Zero;
            if (jobPara.FrontBgr != null)
            {
                Rectangle rect = new Rectangle(0, 0, jobPara.FrontBgr.Width, jobPara.FrontBgr.Height);
                bitmapData = jobPara.FrontBgr.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                PavoApi.BITMAP bItmap;
                bItmap.BmType = 21072;
                bItmap.BmWidth = bitmapData.Width;
                bItmap.BmHeight = bitmapData.Height;
                bItmap.BmWidthBytes = bitmapData.Stride;
                bItmap.BmPlanes = 1;
                bItmap.BmBitsPixel = 24;
                bItmap.BmBits = bitmapData.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap, intPtr, false);
                hItiCardPrintParameter.LpFrontBgr = intPtr;
            }
            if (jobPara.FrontK != null)
            {
                Rectangle rect2 = new Rectangle(0, 0, jobPara.FrontK.Width, jobPara.FrontK.Height);
                bitmapData2 = jobPara.FrontK.LockBits(rect2, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                PavoApi.BITMAP bItmap2;
                bItmap2.BmType = 21072;
                bItmap2.BmWidth = bitmapData2.Width;
                bItmap2.BmHeight = bitmapData2.Height;
                bItmap2.BmWidthBytes = bitmapData2.Stride;
                bItmap2.BmPlanes = 1;
                bItmap2.BmBitsPixel = 8;
                bItmap2.BmBits = bitmapData2.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap2, intPtr, false);
                hItiCardPrintParameter.LpFrontK = intPtr;
            }
            if (jobPara.FrontO != null)
            {
                Rectangle rect3 = new Rectangle(0, 0, jobPara.FrontO.Width, jobPara.FrontO.Height);
                bitmapData3 = jobPara.FrontO.LockBits(rect3, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                PavoApi.BITMAP bItmap3;
                bItmap3.BmType = 21072;
                bItmap3.BmWidth = bitmapData3.Width;
                bItmap3.BmHeight = bitmapData3.Height;
                bItmap3.BmWidthBytes = bitmapData3.Stride;
                bItmap3.BmPlanes = 1;
                bItmap3.BmBitsPixel = 8;
                bItmap3.BmBits = bitmapData3.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap3, intPtr, false);
                hItiCardPrintParameter.LpFrontO = intPtr;
            }
            if (jobPara.BackBgr != null)
            {
                Rectangle rect4 = new Rectangle(0, 0, jobPara.BackBgr.Width, jobPara.BackBgr.Height);
                bitmapData4 = jobPara.BackBgr.LockBits(rect4, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                PavoApi.BITMAP bItmap4;
                bItmap4.BmType = 21072;
                bItmap4.BmWidth = bitmapData4.Width;
                bItmap4.BmHeight = bitmapData4.Height;
                bItmap4.BmWidthBytes = bitmapData4.Stride;
                bItmap4.BmPlanes = 1;
                bItmap4.BmBitsPixel = 24;
                bItmap4.BmBits = bitmapData4.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap4, intPtr, false);
                hItiCardPrintParameter.LpBackBgr = intPtr;
            }
            if (jobPara.BackK != null)
            {
                Rectangle rect5 = new Rectangle(0, 0, jobPara.BackK.Width, jobPara.BackK.Height);
                bitmapData5 = jobPara.BackK.LockBits(rect5, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                PavoApi.BITMAP bItmap5;
                bItmap5.BmType = 21072;
                bItmap5.BmWidth = bitmapData5.Width;
                bItmap5.BmHeight = bitmapData5.Height;
                bItmap5.BmWidthBytes = bitmapData5.Stride;
                bItmap5.BmPlanes = 1;
                bItmap5.BmBitsPixel = 8;
                bItmap5.BmBits = bitmapData5.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap5, intPtr, false);
                hItiCardPrintParameter.LpBackK = intPtr;
            }
            if (jobPara.BackO != null)
            {
                Rectangle rect6 = new Rectangle(0, 0, jobPara.BackO.Width, jobPara.BackO.Height);
                bitmapData6 = jobPara.BackO.LockBits(rect6, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                PavoApi.BITMAP bItmap6;
                bItmap6.BmType = 21072;
                bItmap6.BmWidth = bitmapData6.Width;
                bItmap6.BmHeight = bitmapData6.Height;
                bItmap6.BmWidthBytes = bitmapData6.Stride;
                bItmap6.BmPlanes = 1;
                bItmap6.BmBitsPixel = 8;
                bItmap6.BmBits = bitmapData6.Scan0;
                intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PavoApi.BITMAP)));
                Marshal.StructureToPtr(bItmap6, intPtr, false);
                hItiCardPrintParameter.LpBackO = intPtr;
            }
            uint result = PavoApi.PAVO_PrintOneCardW(szPrinter, ref hItiCardPrintParameter, ref hItiHeatingEnergy, IntPtr.Zero);
            if (jobPara.FrontBgr != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpFrontBgr);
                jobPara.FrontBgr.UnlockBits(bitmapData);
            }
            if (jobPara.FrontK != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpFrontK);
                jobPara.FrontK.UnlockBits(bitmapData2);
            }
            if (jobPara.FrontO != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpFrontO);
                jobPara.FrontO.UnlockBits(bitmapData3);
            }
            if (jobPara.BackBgr != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpBackBgr);
                jobPara.BackBgr.UnlockBits(bitmapData4);
            }
            if (jobPara.BackK != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpBackK);
                jobPara.BackK.UnlockBits(bitmapData5);
            }
            if (jobPara.BackO != null)
            {
                Marshal.FreeHGlobal(hItiCardPrintParameter.LpBackO);
                jobPara.BackO.UnlockBits(bitmapData6);
            }
            return result;
        }

        public static uint PAVO_ReadMagTrackData(string szPrinter, uint dwCom, ref MagTrackData2 lpTrackData)
        {
            PavoApi._MAG_TRACK_DATA2 mAgTrackData = default(PavoApi._MAG_TRACK_DATA2);
            mAgTrackData.ByTrackFlag = lpTrackData.ByTrackFlag;
            mAgTrackData.ByEncodeMode = 0;
            mAgTrackData.ByCoercivity = 0;
            mAgTrackData.ByT2Bpi = lpTrackData.ByT2Bpi;
            mAgTrackData.ByRawLenT1 = 0;
            mAgTrackData.ByRawLenT2 = 0;
            mAgTrackData.ByRawLenT3 = 0;
            uint num = PavoApi.PAVO_ReadMagTrackDataW(szPrinter, dwCom, ref mAgTrackData);
            if (num == 0u)
            {
                if ((mAgTrackData.ByTrackFlag & 1) == 1)
                {
                    Array.Copy(mAgTrackData.SzTrack1, lpTrackData.SzTrack1, 256);
                }
                if ((mAgTrackData.ByTrackFlag & 2) == 2)
                {
                    Array.Copy(mAgTrackData.SzTrack2, lpTrackData.SzTrack2, 256);
                }
                if ((mAgTrackData.ByTrackFlag & 4) == 4)
                {
                    Array.Copy(mAgTrackData.SzTrack3, lpTrackData.SzTrack3, 256);
                }
            }
            return num;
        }

        public static uint PAVO_WriteMagTrackData(string szPrinter, uint dwCom, MagTrackData2 trackData)
        {
            PavoApi._MAG_TRACK_DATA2 mAgTrackData = default(PavoApi._MAG_TRACK_DATA2);
            mAgTrackData.ByTrackFlag = trackData.ByTrackFlag;
            mAgTrackData.ByEncodeMode = trackData.ByEncodeMode;
            mAgTrackData.ByCoercivity = trackData.ByCoercivity;
            mAgTrackData.ByT2Bpi = trackData.ByT2Bpi;
            mAgTrackData.ByRawLenT1 = trackData.ByRawLenT1;
            mAgTrackData.ByRawLenT2 = trackData.ByRawLenT2;
            mAgTrackData.ByRawLenT3 = trackData.ByRawLenT3;
            mAgTrackData.SzTrack1 = new byte[256];
            mAgTrackData.SzTrack2 = new byte[256];
            mAgTrackData.SzTrack3 = new byte[256];
            if ((mAgTrackData.ByTrackFlag & 1) == 1)
            {
                Array.Copy(trackData.SzTrack1, mAgTrackData.SzTrack1, trackData.SzTrack1.Length);
            }
            if ((mAgTrackData.ByTrackFlag & 2) == 2)
            {
                Array.Copy(trackData.SzTrack2, mAgTrackData.SzTrack2, trackData.SzTrack2.Length);
            }
            if ((mAgTrackData.ByTrackFlag & 4) == 4)
            {
                Array.Copy(trackData.SzTrack3, mAgTrackData.SzTrack3, trackData.SzTrack3.Length);
            }
            return PavoApi.PAVO_WriteMagTrackDataW(szPrinter, dwCom, ref mAgTrackData);
        }

        public static uint PAVO_SetPassword(string szPrinter, string szCurrentPasswd, string szNewPasswd)
        {
            return PavoApi.PAVO_SetPasswordW(szPrinter, szCurrentPasswd, szNewPasswd);
        }

        public static uint PAVO_SetSecurityMode(string szPrinter, string szCurrentPasswd, int nSecurityMode)
        {
            return PavoApi.PAVO_SetSecurityModeW(szPrinter, szCurrentPasswd, nSecurityMode);
        }

        public static uint PAVO_EnumUSBCardPrinters(byte[] szPrinter, ref uint nBufferSize)
        {
            return PavoApi.PAVO_EnumUSBCardPrintersA(szPrinter, ref nBufferSize);
        }

        public static uint PAVO_EraseCardArea(string szPrinter, int nLeft, int nTop, int nRight, int nBottom)
        {
            return PavoApi.PAVO_EraseCardAreaW(szPrinter, nLeft, nTop, nRight, nBottom);
        }

        public const uint PavoRibbonTypeYmcko = 0u;

        public const uint PavoRibbonTypeK = 1u;

        public const uint PavoRibbonTypeKo = 3u;

        public const uint PavoRibbonTypeYmckok = 4u;

        public const uint PavoRibbonTypeHalfYmcko = 5u;

        public const uint PavoRibbonTypeYmckfo = 12u;

        public const uint PavoCardTypeBlankCard = 0u;

        public const uint PavoCardTypeSmartChip_6Pin = 1u;

        public const uint PavoCardTypeSmartChip_8Pin = 2u;

        public const uint PavoCardTypeMagStrip = 3u;

        public const uint PavoCardTypeChipMagStrip = 4u;

        public const uint PavoCardTypeAdhesiveCard = 5u;

        public const uint PavoFlagNotShowErrorMsgDlg = 1u;

        public const uint PavoFlagWaitMsgDone = 2u;

        public const uint PavoFlagNotShowCleanMsg = 256u;

        public const uint PavoFlagWatchJobPrinted = 1024u;

        public const uint PavoDataflagResinFront = 2u;

        public const uint PavoDataflagResinBack = 16u;

        public const uint FfCardType = 1u;

        public const uint FfFlags = 2u;

        public const uint FfDataFlag = 4u;

        public const uint FfParentHwnd = 8u;

        public const uint FfOrientation = 32u;

        public const uint FfCopies = 64u;

        public const uint FfCustomIndex = 128u;

        public const uint FfDuplex = 256u;

        public const uint FfRibbonType = 512u;

        public const uint FfPrintColor = 1024u;

        public const uint FfDitherK = 2048u;

        public const uint FfLamin = 4096u;

        public const uint FfAllFields = 4294967295u;

        public const uint PavoDuplexPrintFrontSide = 1u;

        public const uint PavoDuplexPrintBackSide = 2u;

        public const uint PavoDataResinFront = 2u;

        public const uint PavoDataResinBack = 5u;

        public const uint PavoDevinfoMfgSerial = 1u;

        public const uint PavoDevinfoModelName = 2u;

        public const uint PavoDevinfoFirmwareVersion = 3u;

        public const uint PavoDevinfoRibbonInfo = 4u;

        public const uint PavoDevinfoPrintCount = 5u;

        public const uint PavoDevinfoCardPosition = 6u;

        public const uint PavoCommandResetPrinter = 100u;

        public const uint PavoCommandCleanCardPath = 105u;

        public const uint PavoCommandResetPrintCount = 106u;

        public const uint PavoCommandFlipCard = 202u;

        public const uint MoveCardToIcEncoder = 1u;

        public const uint MoveCardToRfidEncoder = 3u;

        public const uint MoveCardToRejectBox = 4u;

        public const uint MoveCardToHopper = 5u;

        public const uint MoveCardToFlipper = 6u;

        public const uint MoveCardToPrintFromFlipper = 7u;

        public const uint MoveCardToStandbyPosition = 10u;

        public const int WmPavoPrinter = 21845;

        public const int MsgJobBegin = 1;

        public const int MsgPrintOnePage = 3;

        public const int MsgPrintOneCopy = 4;

        public const int MsgJobEnd = 6;

        public const int MsgDeviceStatus = 7;

        public const int MsgJobCanceled = 12;

        public const int MsgJobPrinted = 24;

        public const uint PavoDsBusy = 524288u;

        public const uint PavoDsOffline = 128u;

        public const uint PavoDsPrinting = 2u;

        public const uint PavoDsProcessingData = 5u;

        public const uint PavoDsSendingData = 6u;

        public const uint PavoDsCardMismatch = 65790u;

        public const uint PavoDsCmdSeqError = 197118u;

        public const uint PavoDsSramError = 196609u;

        public const uint PavoDsSdramError = 196865u;

        public const uint PavoDsAdcError = 197121u;

        public const uint PavoDsNvramError = 197377u;

        public const uint PavoDsSdramChecksumError = 197378u;

        public const uint PavoDsWriteFail = 31u;

        public const uint PavoDsReadFail = 47u;

        public const uint PavoDs0100CoverOpen = 256u;

        public const uint PavoDs0101FlipperCoverOpen = 257u;

        public const uint PavoDs0200IcMissing = 512u;

        public const uint PavoDs0201RibbonMissing = 513u;

        public const uint PavoDs0202RibonMismatch = 514u;

        public const uint PavoDs0203RibbonTypeError = 515u;

        public const uint PavoDs0300RibbonSearchFail = 768u;

        public const uint PavoDs0301RibbonOut = 769u;

        public const uint PavoDs0302PrintFail = 770u;

        public const uint PavoDs0303PrintFail = 771u;

        public const uint PavoDs0304RibbonOut = 772u;

        public const uint PavoDs0400CardOut = 1024u;

        public const uint PavoDs0500CardJam = 1280u;

        public const uint PavoDs0501CardJam = 1281u;

        public const uint PavoDs0502CardJam = 1282u;

        public const uint PavoDs0503CardJam = 1283u;

        public const uint PavoDs0504CardJam = 1284u;

        public const uint PavoDs0505CardJam = 1285u;

        public const uint PavoDs0506CardJam = 1286u;

        public const uint PavoDs0507CardJam = 1287u;

        public const uint PavoDs0600CardMismatch = 1536u;

        public const uint PavoDs0700CamError = 1792u;

        public const uint PavoDs0800FlipperError = 2048u;

        public const uint PavoDs0900NvramError = 2304u;

        public const uint PavoDs1000RibbonError = 4096u;

        public const uint PavoDs1100RbnTakeCalibFail = 4352u;

        public const uint PavoDs1101RbnSupplyCalibFail = 4353u;

        public const uint PavoDs1200AdcError = 4608u;

        public const uint PavoDs1300FwError = 4864u;

        public const uint PavoDs1400PowerSupplyError = 5120u;

        internal struct BITMAP
        {
            public int BmType;

            public int BmWidth;

            public int BmHeight;

            public int BmWidthBytes;

            public short BmPlanes;

            public short BmBitsPixel;

            public IntPtr BmBits;
        }

        internal struct _PAVO_JOB_PROPERTY
        {
            public uint DwSize;

            public uint DwCardType;

            public uint DwFlags;

            public uint DwDataFlag;

            public IntPtr HParentWnd;

            public IntPtr HReserved1;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PReserved1;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PReserved2;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PReserved3;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string PReserved4;

            public short ShOrientation;

            public short ShCopies;

            public short ShReserved1;

            public short ShReserved2;

            public uint DwCustomIndex;

            public uint DwFieldFlag;

            public uint DwReserved3;

            public uint DwReserved4;

            public ushort WReserved1;

            public byte BReserved1;

            public byte ByPrintAs230E;

            public byte ByTransparentCard;

            public byte ByFlip;

            public byte ByReserved1;

            public byte ByCardThick;

            public byte ByDuplex;

            public byte ByRibbonType;

            public byte ByPrintColor;

            public byte ByDitherK;

            public byte ByReserved5;

            public byte ByReserved6;

            public byte ByReserved7;

            public byte ByRotate180;
        }

        internal struct _HITI_CARD_PRINT_PARAMETER
        {
            public uint DwSize;

            public uint DwReserve1;

            public uint DwReserve2;

            public uint DwFlags;

            public byte ByOrientation;

            public byte ByCardThickness;

            public byte ByTransparentCard;

            public byte ByReserve4;

            public byte ByReserve5;

            public byte ByReserve6;

            public byte ByReserve7;

            public byte ByReserve8;

            public IntPtr LpFrontBgr;

            public IntPtr LpFrontK;

            public IntPtr LpFrontO;

            public IntPtr LpBackBgr;

            public IntPtr LpBackK;

            public IntPtr LpBackO;

            public IntPtr LpReserve1;

            public IntPtr LpReserve2;
        }

        internal struct _HITI_HEATING_ENERGY
        {
            public sbyte ChFrontDenYmc;

            public sbyte ChFrontDenK;

            public sbyte ChFrontDenO;

            public sbyte ChFrontDenResinK;

            public sbyte ChBackDenYmc;

            public sbyte ChBackDenK;

            public sbyte ChBackDenO;

            public sbyte ChBackDenResinK;

            public sbyte ChFrontSenYmc;

            public sbyte ChFrontSenK;

            public sbyte ChFrontSenO;

            public sbyte ChFrontSenResinK;

            public sbyte ChBackSenYmc;

            public sbyte ChBackSenK;

            public sbyte ChBackSenO;

            public sbyte ChBackSenResinK;
        }

        internal struct _MAG_TRACK_DATA2
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] SzTrack1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] SzTrack2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] SzTrack3;

            public byte ByTrackFlag;

            public byte ByEncodeMode;

            public byte ByCoercivity;

            public byte ByT2Bpi;

            public byte ByRawLenT1;

            public byte ByRawLenT2;

            public byte ByRawLenT3;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
            public byte[] ByReserve;
        }
    }
}
