using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.Devices
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TrackInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3 * 512)]
        public byte[] Contents;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] Lengths;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Status;
    }

    public static class UnSafeMethods
    {
        #region[Act_A6三合一读卡器]

        private const string DllPathActA6 = "External\\Act_A6\\A6CRTAPI.dll";

        #region DLL Import

        [DllImport(DllPathActA6, EntryPoint = "A6_Connect", CharSet = CharSet.Ansi)]
        public static extern int A6_Connect(int dwPort, int dwSpeed, ref int phReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_Disconnect", CharSet = CharSet.Ansi)]
        public static extern int A6_Disconnect(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_Initialize", CharSet = CharSet.Ansi)]
        public static extern int A6_Initialize(int hReader, byte bResetMode, byte[] pbVerBuff, ref int pcbVerLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_GetCRCondition", CharSet = CharSet.Ansi)]
        public static extern int A6_GetCRCondition(int hReader, byte[] pStatus);

        [DllImport(DllPathActA6, EntryPoint = "A6_SetCardIn", CharSet = CharSet.Ansi)]
        public static extern int A6_SetCardIn(int hReader, byte bFrontSet, byte bRearSet);

        [DllImport(DllPathActA6, EntryPoint = "A6_SetDockedPos", CharSet = CharSet.Ansi)]
        public static extern int A6_SetDockedPos(int hReader, byte bDockedPos);

        [DllImport(DllPathActA6, EntryPoint = "A6_MoveCard", CharSet = CharSet.Ansi)]
        public static extern int A6_MoveCard(int hReader, int bMoveMethod);

        [DllImport(DllPathActA6, EntryPoint = "A6_LedOn", CharSet = CharSet.Ansi)]
        public static extern int A6_LedOn(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_LedOff", CharSet = CharSet.Ansi)]
        public static extern int A6_LedOff(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_LedBlink", CharSet = CharSet.Ansi)]
        public static extern int A6_LedBlink(int hReader, byte bOnTime, byte bOffTime);

        [DllImport(DllPathActA6, EntryPoint = "A6_DetectIccType", CharSet = CharSet.Ansi)]
        public static extern int A6_DetectIccType(int hReader, ref byte pbType);

        [DllImport(DllPathActA6, EntryPoint = "A6_IccPowerOn", CharSet = CharSet.Ansi)]
        public static extern int A6_IccPowerOn(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_IccPowerOff", CharSet = CharSet.Ansi)]
        public static extern int A6_IccPowerOff(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_CpuColdReset", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuColdReset(int hReader, byte[] pbATRBuff, ref int pcbATRLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_CpuWarmReset", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuWarmReset(int hReader, byte[] pbATRBuff, ref int pcbATRLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_CpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuTransmit(int hReader, byte bProtocol, byte[] pbSendBuff, int cbSendLength,
            byte[] pbRecvBuff, ref int pcbRecvLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_ReadTracks", CharSet = CharSet.Ansi)]
        public static extern int A6_ReadTracks(int hReader, byte bMode, uint iTrackID, ref TrackInfo pTrackInfo);

        [DllImport(DllPathActA6, EntryPoint = "A6_TypeACpuSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeACpuSelect(int hReader, byte[] pbATRBuff, ref int pcbATRLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_TypeBCpuSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeBCpuSelect(int hReader, byte[] pbATRBuff, ref int pcbATRLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_TypeABCpuDeselect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuDeselect(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_TypeABCpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuTransmit(int hReader, byte[] pbSendBuff, ushort cbSendLength,
            byte[] pbRecvBuff, ref int pcbRecvLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_TypeACpuGetUID", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeACpuGetUID(int hReader, byte[] pbUIDBuff, ref int pcbUIDLength);

        //for M1 card
        [DllImport(DllPathActA6, EntryPoint = "A6_SxxSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxSelect(int hReader);

        [DllImport(DllPathActA6, EntryPoint = "A6_SxxGetUID", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxGetUID(int hReader, byte[] pbUIDBuff, ref int pcbUIDLength);

        [DllImport(DllPathActA6, EntryPoint = "A6_SxxVerifyPassword", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxVerifyPassword(int hReader, byte sec, bool isKeyA, byte[] data);

        [DllImport(DllPathActA6, EntryPoint = "A6_SxxReadBlock", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxReadBlock(int hReader, byte sec, byte block, byte[] data);

        [DllImport(DllPathActA6, EntryPoint = "A6_SxxWriteBlock", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxWriteBlock(int hReader, byte sec, byte block, byte[] data);

        #endregion DLL Import

        #endregion

        #region[新中新身份证读卡器]

        private const string DllPathXzx = "External\\Xzx\\SynIDCardAPI.dll";

        #region DLL Import

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public struct IdCardData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string Name; //姓名   
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)] public string Sex; //性别
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)] public string Nation; //名族
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)] public string Born; //出生日期
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 72)] public string Address; //住址
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)] public string IDCardNo; //身份证号
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string GrantDept; //发证机关
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)] public string UserLifeBegin; // 有效开始日期
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)] public string UserLifeEnd; // 有效截止日期

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 38)] public string reserved; // 保留
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)] public string PhotoFileName; // 照片路径
        }

        /************************端口类API *************************/

        [DllImport(DllPathXzx, EntryPoint = "Syn_GetCOMBaud", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetCOMBaud(int nPort, ref uint puiBaudRate);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetCOMBaud", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetCOMBaud(int nPort, uint uiCurrBaud, uint uiSetBaud);

        [DllImport(DllPathXzx, EntryPoint = "Syn_OpenPort", CharSet = CharSet.Ansi)]
        public static extern int Syn_OpenPort(int nPort);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ClosePort", CharSet = CharSet.Ansi)]
        public static extern int Syn_ClosePort(int nPort);

        /**************************SAM类函数 **************************/

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetMaxRFByte", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetMaxRFByte(int nPort, byte ucByte, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ResetSAM", CharSet = CharSet.Ansi)]
        public static extern int Syn_ResetSAM(int nPort, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_GetSAMStatus", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMStatus(int nPort, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_GetSAMID", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMID(int nPort, ref byte pucSAMID, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_GetSAMIDToStr", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetSAMIDToStr(int nPort, ref byte pcSAMID, int iIfOpen);

        /*************************身份证卡类函数 ***************************/

        [DllImport(DllPathXzx, EntryPoint = "Syn_StartFindIDCard", CharSet = CharSet.Ansi)]
        public static extern int Syn_StartFindIDCard(int nPort, ref byte pucIIN, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SelectIDCard", CharSet = CharSet.Ansi)]
        public static extern int Syn_SelectIDCard(int nPort, ref byte pucSN, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadBaseMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadBaseMsg(int nPort, ref byte pucCHMsg, ref uint puiCHMsgLen, ref byte pucPHMsg,
            ref uint puiPHMsgLen, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadBaseMsgToFile", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadBaseMsgToFile(int nPort, ref byte pcCHMsgFileName, ref uint puiCHMsgFileLen,
            ref byte pcPHMsgFileName, ref uint puiPHMsgFileLen, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadBaseFPMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadBaseFPMsg(int nPort, ref byte pucCHMsg, ref uint puiCHMsgLen, ref byte pucPHMsg,
            ref uint puiPHMsgLen, ref byte pucFPMsg, ref uint puiFPMsgLen, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadBaseFPMsgToFile", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadBaseFPMsgToFile(int nPort, ref byte pcCHMsgFileName, ref uint puiCHMsgFileLen,
            ref byte pcPHMsgFileName, ref uint puiPHMsgFileLen, ref byte pcFPMsgFileName, ref uint puiFPMsgFileLen,
            int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadNewAppMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadNewAppMsg(int nPort, ref byte pucAppMsg, ref uint puiAppMsgLen, int iIfOpen);

        [DllImport(DllPathXzx, EntryPoint = "Syn_GetBmp", CharSet = CharSet.Ansi)]
        public static extern int Syn_GetBmp(int nPort, ref byte Wlt_File);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadMsg(int nPortID, int iIfOpen, ref IdCardData pIDCardData);

        [DllImport(DllPathXzx, EntryPoint = "Syn_ReadFPMsg", CharSet = CharSet.Ansi)]
        public static extern int Syn_ReadFPMsg(int nPortID, int iIfOpen, ref IdCardData pIDCardData,
            ref byte cFPhotoname);

        [DllImport(DllPathXzx, EntryPoint = "Syn_FindReader", CharSet = CharSet.Ansi)]
        public static extern int Syn_FindReader();

        [DllImport(DllPathXzx, EntryPoint = "Syn_FindUSBReader", CharSet = CharSet.Ansi)]
        public static extern int Syn_FindUSBReader();

        /***********************设置附加功能函数 ************************/

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetPhotoPath", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetPhotoPath(int iOption, ref byte cPhotoPath);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetPhotoType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetPhotoType(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetPhotoName", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetPhotoName(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetSexType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetSexType(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetNationType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetNationType(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetBornType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetBornType(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetUserLifeBType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetUserLifeBType(int iType);

        [DllImport(DllPathXzx, EntryPoint = "Syn_SetUserLifeEType", CharSet = CharSet.Ansi)]
        public static extern int Syn_SetUserLifeEType(int iType, int iOption);

        #endregion

        #endregion

        #region[Act_F6发卡器]

        #region Common

        private const string DllPathActF6 = "External\\Act_F6\\A6_DLL.dll";

        [DllImport(DllPathActF6, EntryPoint = "A6_CommOpen", CharSet = CharSet.Ansi)]
        public static extern int CommOpen(string port);

        [DllImport(DllPathActF6, EntryPoint = "A6_CommClose", CharSet = CharSet.Ansi)]
        public static extern int CommClose(int handle);

        [DllImport(DllPathActF6, EntryPoint = "A6_Reset", CharSet = CharSet.Ansi)]
        public static extern int Reset(int handle, byte pm, byte[] code);

        [DllImport(DllPathActF6, EntryPoint = "A6_EnterCard", CharSet = CharSet.Ansi)]
        public static extern int EnterCard(int handle, byte type, int time);

        [DllImport(DllPathActF6, EntryPoint = "A6_Led1Control", CharSet = CharSet.Ansi)]
        public static extern int Led1Control(int handle, byte pm);

        //0X30 AT24C01
        //0X31 AT24C02
        //0X32 AT24C04
        //0X33 AT24C08
        //0X34 AT24C16
        //0X35 AT24C32
        //0X36 AT24C64
        //0X37 AT45DB041
        //0X38 AT102
        //0X39 AT1604
        //0X3A AT1608
        //0X3B SLE4442
        //0X3C SLE4428
        //0X3D CPU T 0
        //0X3E CPU T 1
        //0XFF 不能识别的卡类型
        [DllImport(DllPathActF6, EntryPoint = "A6_AutoTestICCard", CharSet = CharSet.Ansi)]
        public static extern int AutoTestICCard(int handle, ref byte type);

        [DllImport(DllPathActF6, EntryPoint = "A6_CheckCardPosition", CharSet = CharSet.Ansi)]
        public static extern int CheckCardPosition(int handle, ref byte pos);

        [DllImport(DllPathActF6, EntryPoint = "A6_Sle4428Reset", CharSet = CharSet.Ansi)]
        public static extern int Sle4428Reset(int handle);

        [DllImport(DllPathActF6, EntryPoint = "A6_Sle4428Read", CharSet = CharSet.Ansi)]
        public static extern int Sle4428Read(int handle, int address, byte len, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_Sle4428Write", CharSet = CharSet.Ansi)]
        public static extern int Sle4428Write(int handle, int address, byte len, byte[] data);

        [DllImport("A6_DLL.dll", EntryPoint = "A6_Sle4428VerifyPWD", CharSet = CharSet.Ansi)]
        public static extern int Sle4428VerifyPWD(int handle, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_IcCardPowerOn", CharSet = CharSet.Ansi)]
        public static extern int IcCardPowerOn(int handle);

        [DllImport(DllPathActF6, EntryPoint = "A6_IcCardPowerOff", CharSet = CharSet.Ansi)]
        public static extern int IcCardPowerOff(int handle);

        [DllImport(DllPathActF6, EntryPoint = "A6_MoveCard", CharSet = CharSet.Ansi)]
        public static extern int MoveCard(int handle, byte pm);

        [DllImport(DllPathActF6, EntryPoint = "A6_ReadMagcardDecode", CharSet = CharSet.Ansi)]
        public static extern int ReadMagcardDecode(int handle, byte track, ref int len, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_ReadMagcardUNDecode", CharSet = CharSet.Ansi)]
        public static extern int ReadMagcardUNDecode(int handle, byte track, ref int len, byte[] data);

        #endregion Common

        #region Mifare One

        [DllImport(DllPathActF6, EntryPoint = "A6_S50DetectCard", CharSet = CharSet.Ansi)]
        private static extern int S50DetectCard(int handle);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50GetCardID", CharSet = CharSet.Ansi)]
        public static extern int S50GetCardID(int handle, byte[] card);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50LoadSecKey", CharSet = CharSet.Ansi)]
        private static extern int S50LoadSecKey(int handle, byte addr, byte keyType, byte[] key);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50ReadBlock", CharSet = CharSet.Ansi)]
        private static extern int S50ReadBlock(int handle, byte addr, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50WriteBlock", CharSet = CharSet.Ansi)]
        private static extern int S50WriteBlock(int handle, byte addr, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50InitValue", CharSet = CharSet.Ansi)]
        private static extern int S50InitValue(int handle, byte addr, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50Increment", CharSet = CharSet.Ansi)]
        private static extern int S50Increment(int handle, byte addr, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50Decrement", CharSet = CharSet.Ansi)]
        private static extern int S50Decrement(int handle, byte addr, byte[] data);

        [DllImport(DllPathActF6, EntryPoint = "A6_S50Halt", CharSet = CharSet.Ansi)]
        private static extern int S50Halt(int handle);

        #endregion Mifare One

        #region[非接CPU]

        [DllImport(DllPathActF6, EntryPoint = "A6_unmeet_activation", CharSet = CharSet.Ansi)]
        public static extern int A6_unmeet_activation(int handle, byte[] data /*, ref int len*/);

        [DllImport(DllPathActF6, EntryPoint = "A6_unmeet_APDU", CharSet = CharSet.Ansi)]
        public static extern int A6_unmeet_APDU(int handle, byte cbSendLength, byte[] pbSendBuff, byte[] pbRecvBuff
            /*, ref int pcbRecvLength*/);


        #endregion

        #endregion

        #region[CashCode钱箱]

        private const string DllPathCashCode = "External\\CashCode\\CashDriver.dll";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
        public struct BillRecord
        {
            public float Denomination;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)] public string name;

            private readonly bool bRouted;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
        public struct PollResults
        {
            public byte z1;
            public byte z2;
        }

        [DllImport(DllPathCashCode, EntryPoint = "InitDevice", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool InitDevice(int port, int timeout);

        [DllImport(DllPathCashCode, EntryPoint = "CloseDevice", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CloseDevice();

        [DllImport(DllPathCashCode, EntryPoint = "CmdReset", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdReset(byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdPoll", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdPoll(byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdGetBillTable", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdGetBillTable(ref BillRecord record, byte Addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdIdentification", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdIdentification(byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdPack", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdPack(byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdSetSecurity", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdSetSecurity(int s1, byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "CmdBillType", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CmdBillType(int s1, int s2, byte addr);

        [DllImport(DllPathCashCode, EntryPoint = "GetPollResult", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        public static extern PollResults GetPollResult();

        #endregion DLL Import

        #region[ZBR斑马打印机]

        private const string DllPathZbrPrint = "External\\Zbr\\ZBRPrinter.dll";

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNGetSDKVer", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void ZBRPRNGetSDKVer(out int major, out int minor, out int engLevel);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRGetHandle", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRGetHandle(out IntPtr _handle, byte[] drvName, out int prn_type, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRGetHandle", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRGetHandle(out IntPtr _handle, [MarshalAs(UnmanagedType.LPStr)] string drvName,
            out int prn_type, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRCloseHandle", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRCloseHandle(IntPtr _handle, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNSetCardFeedingMode", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNSetCardFeedingMode(IntPtr _handle, int prn_type, int mode, out int err);

        // Card Movement

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNMovePrintReady", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNMovePrintReady(IntPtr _handle, int prn_type, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNFlipCard", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNFlipCard(IntPtr _handle, int prn_type, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNEjectCard", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNEjectCard(IntPtr _handle, int prn_type, out int err);

        // Magnetic Encoding

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead,
            byte[] trk1Buf, out int trk1BytesNeeded,
            byte[] trk2Buf, out int trk2BytesNeeded,
            byte[] trk3Buf, out int trk3BytesNeeded,
            out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead,
            char[] trk1Buf, out int trk1BytesNeeded,
            char[] trk2Buf, out int trk2BytesNeeded,
            char[] trk3Buf, out int trk3BytesNeeded,
            out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMag", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNReadMag(IntPtr _handle, int prn_type, int trksToRead,
            IntPtr trk1Buf, out int trk1BytesNeeded,
            IntPtr trk2Buf, out int trk2BytesNeeded,
            IntPtr trk3Buf, out int trk3BytesNeeded,
            out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMagByTrk", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int ZBRPRNReadMagByTrk(IntPtr _handle, int prn_type, int trksToRead, byte[] trkBuf,
            out int trkBytesNeeded, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMagByTrk", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNReadMagByTrk(IntPtr _handle, int prn_type, int trksToRead, char[] trkBuf,
            out int trkBytesNeeded, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNReadMagByTrk", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNReadMagByTrk(IntPtr _handle, int prn_type, int trkNumb, IntPtr trkBuf,
            out int trkBytesNeeded, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite, byte[] trk1Data,
            byte[] trk2Data, byte[] trk3Data, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite, char[] trk1Data,
            char[] trk2Data, char[] trk3Data, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNWriteMag", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int ZBRPRNWriteMag(IntPtr _handle, int prn_type, int trksToWrite, IntPtr trk1Data,
            IntPtr trk2Data, IntPtr trk3Data, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNWriteMagByTrk", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int ZBRPRNWriteMagByTrk(IntPtr _handle, int prn_type, int trkNumb, byte[] trkBuf,
            out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNWriteMagPassThru", CharSet = CharSet.Auto, SetLastError = true)
        ]
        public static extern int ZBRPRNWriteMagPassThru(IntPtr hDC, int prn_Type, int trksToWrite, byte[] trk1Data,
            byte[] trk2Data, byte[] trk3Data, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNSetMagEncodingStd", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNSetMagEncodingStd(IntPtr hDC, int prn_Type, int std, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNResetMagEncoder", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNResetMagEncoder(IntPtr hDC, int prn_Type, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNSetEncoderCoercivity", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRPRNSetEncoderCoercivity(IntPtr hDC, int prn_Type, int coercivity, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNGetPrinterStatus", CharSet = CharSet.Auto, SetLastError = true)
        ]
        public static extern int ZBRPRNGetPrinterStatus(out int status);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNGetSensorStatus", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNGetSensorStatus(IntPtr _handle, int prn_type, byte[] status, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNMoveCard", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNMoveCard(IntPtr _handle, int prn_type, int steps, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNStartSmartCard", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNStartSmartCard(IntPtr _handle, int prn_type, int cardType, out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNEndSmartCard", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNEndSmartCard(IntPtr _handle, int prn_type, int cardType, int moveType,
            out int err);

        [DllImport(DllPathZbrPrint, EntryPoint = "ZBRPRNResync", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int ZBRPRNResync(IntPtr hPrinter, int printerType, out int err);

        #endregion

        #region ZBR斑马打印机绘制

        private const string DllPathZbrGraphics = "External\\Zbr\\ZBRGraphics.dll";
        // Check Print Spooler ------------------------------------------------------------------------------

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIIsPrinterReady", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIIsPrinterReady(byte[] strPrinterName, out int err);

        // SDK DLL Version ----------------------------------------------------------------------------------

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIGetSDKVer", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern void ZBRGDIGetSDKVer(out int major, out int minor, out int engLevel);

        // Initialization -----------------------------------------------------------------------------------

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIInitGraphics", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIInitGraphics(byte[] strPrinterName, out IntPtr hDC, out int err);

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDICloseGraphics", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDICloseGraphics(IntPtr hDC, out int err);

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIClearGraphics", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIClearGraphics(out int err);

        // Print --------------------------------------------------------------------------------------------

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIPrintGraphics", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIPrintGraphics(IntPtr hDC, out int err);

        // Draw ---------------------------------------------------------------------------------------------

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIDrawText", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIDrawText(int x, int y, byte[] text, byte[] font, int fontSize, int fontStyle,
            int color, out int err);

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIDrawLine", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIDrawLine(int x1, int y1, int x2, int y2, int color, float thickness,
            out int err);

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIDrawImageRect", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIDrawImageRect(byte[] fileName, int x, int y, int sizeX, int sizeY, out int err);

        [DllImport(DllPathZbrGraphics, EntryPoint = "ZBRGDIDrawBarCode", CharSet = CharSet.Auto,
            SetLastError = true)]
        public static extern int ZBRGDIDrawBarCode(int x, int y, int rotation, int barcodeType, int widthRatio,
            int mutiplier, int height, int textUnder, byte[] data, out int err);

        #endregion Graphic DLLImports

        #region[证通金属键盘]

        private const string DllPathZtKeyboard = "External\\ZtEpp\\ZT_EPP_API.dll";

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_OpenCom", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_OpenCom(int iPort, int lBaud);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_CloseCom", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_CloseCom();

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinInitialization", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinInitialization(short iInitMode);

        [DllImport(DllPathZtKeyboard, EntryPoint = "ZT_EPP_PinReadVersion", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern short ZT_EPP_PinReadVersion(StringBuilder cpVersion, StringBuilder cpSN,
            StringBuilder cpRechang);

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

        #endregion

        #region[WinApi音频播放]

        [DllImport("winmm.dll", EntryPoint = "mciSendString")]
        public static extern long MciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength,
            int hwndCallback);

        #endregion

        #region[动态加载和卸载dll]

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpPathName);

        public static bool Unload(string lpPathName)
        {
            var module = GetModuleHandle(lpPathName);
            if (module == IntPtr.Zero)
                return false;
            while (FreeLibrary(module))
            {
            }
            return true;
        }

        #endregion

        #region 华大四合一读卡器

        private const string DllPathHuaDa = "External\\HuaDa\\SSSE32.dll";

        #region 公共函数

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_Open", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_Open(string dev_Name);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_Close", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_Close(int readerHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_PosBeep", CharSet = CharSet.Ansi)]
        public static extern int ICC_PosBeep(int readerHandle, byte time);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_ReadEEPROM", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_ReadEEPROM(int readerHandle, int offset, int length, byte[] buffer);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_WriteEEPROM", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_WriteEEPROM(int ReaderHandle, int offset, int length, byte[] buffer);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetDeviceVersion", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetDeviceVersion(int ReaderHandle, byte[] VSoftware, byte[] VHardware,
            byte[] VBoot, byte[] VDate);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetDeviceCSN", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetDeviceCSN(int ReaderHandle, StringBuilder dev_Ser);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetDeviceSN", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetDeviceSN(int ReaderHandle, StringBuilder dev_Ser);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_SetDeviceSN", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_SetDeviceSN(int ReaderHandle, string dev_Ser);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetKeyBoardVersion", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetKeyBoardVersion(int ReaderHandle, StringBuilder ver);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_DisCardType", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_DisCardType(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetMagCardMode", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetMagCardMode(int ReaderHandle, ref int Mode, ref int Track);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_SetMagCardMode", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_SetMagCardMode(int ReaderHandle, int Mode, int Track);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_ChangeSlot", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_ChangeSlot(int ReaderHandle, byte slot);

        [DllImport(DllPathHuaDa, EntryPoint = "StrToHex", CharSet = CharSet.Ansi)]
        public static extern int StrToHex(byte[] strIn, int inLen, byte[] strOut);

        [DllImport(DllPathHuaDa, EntryPoint = "HexToStr", CharSet = CharSet.Ansi)]
        public static extern int HexToStr(byte[] strIn, int inLen, byte[] strOut);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_DispSound", CharSet = CharSet.Ansi)]
        public static extern int ICC_DispSound(int ReaderHandle, byte type, byte nMode);

        #endregion

        #region 接触CPU卡操作函数

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_PowerOn", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_PowerOn(int ReaderHandle, byte ICC_Slot_No, byte[] Response);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_PowerOnHEX", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_PowerOnHEX(int ReaderHandle, byte ICC_Slot_No, byte[] Response);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_PowerOff", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_PowerOff(int ReaderHandle, byte ICC_Slot_No);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_GetStatus", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_GetStatus(int ReaderHandle, byte ICC_Slot_No);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_Application", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_Application(int ReaderHandle, byte ICC_Slot_No, int Lenth_of_Command_APDU,
            byte[] Command_APDU, byte[] Response_APDU);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_ApplicationHEX", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_ApplicationHEX(int ReaderHandle, byte ICC_Slot_No, int Lenth_of_Command_APDU,
            byte[] Command_APDU, byte[] Response_APDU);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_SetCpupara", CharSet = CharSet.Ansi)]
        public static extern int ICC_SetCpupara(int ReaderHandle, byte ICC_Slot_No, byte cpupro, byte cpuetu);

        #endregion

        #region 非接基本操作函数

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_SetTypeA", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_SetTypeA(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_SetTypeB", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_SetTypeB(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Select", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Select(int ReaderHandle, byte cardtype);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Request", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Request(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_anticoll", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_anticoll(int ReaderHandle, byte[] uid);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_RFControl", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_RFControl(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_FindCard", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_FindCard(int ReaderHandle);

        #endregion

        #region 非接CPU卡基本操作函数

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_PowerOnTypeA", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_PowerOnTypeA(int ReaderHandle, byte[] Response);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_PowerOnTypeB", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_PowerOnTypeB(int ReaderHandle, byte[] Response);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Application", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Application(int ReaderHandle, int Lenth_of_Command_APDU,
            byte[] Command_APDU, byte[] Response_APDU);

        #endregion

        #region M1卡基本操作函数



        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Authentication", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Authentication(int ReaderHandle, byte Mode, byte SecNr);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Authentication_Pass", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Authentication_Pass(int ReaderHandle, byte Mode, byte SecNr,
            byte[] PassWord);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Read", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Read(int ReaderHandle, byte Addr, byte[] Data);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_ReadHEX", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_ReadHEX(int ReaderHandle, byte Addr, byte[] DataHex);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Write", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Write(int ReaderHandle, byte Addr, byte[] Data);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_WriteHEX", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_WriteHEX(int ReaderHandle, byte Addr, byte[] DataHex);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_LoadKey", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_LoadKey(int ReaderHandle, byte Mode, byte SecNr, byte[] Key);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_LoadKeyHEX", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_LoadKeyHEX(int ReaderHandle, byte Mode, byte SecNr, byte[] KeyHex);

        #endregion M1卡基本操作函数

        #region 磁条卡基本操作函数

        [DllImport(DllPathHuaDa, EntryPoint = "Rcard", CharSet = CharSet.Ansi)]
        public static extern int Rcard(int ReaderHandle, byte ctime, int track, ref byte rlen, StringBuilder getdata);

        #endregion

        #region 身份证操作函数

        //private const string DllPathHuaDaID = "External\\HuaDa\\IDReader.dll";
        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_ReadIDMsg", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_ReadIDMsg(int ReaderHandle, StringBuilder pBmpFile, byte[] pName,
            byte[] pSex, byte[] pNation, byte[] pBirth, StringBuilder pAddress, StringBuilder pCertNo,
            StringBuilder pDepartment, StringBuilder pEffectData, StringBuilder pExpire, StringBuilder pErrMsg);

        //public static extern int PICC_Reader_ReadIDMsg(int ReaderHandle, StringBuilder pBmpFile, StringBuilder pName, StringBuilder pSex, StringBuilder pNation, StringBuilder pBirth, StringBuilder pAddress, StringBuilder pCertNo, StringBuilder pDepartment, StringBuilder pEffectData, StringBuilder pExpire, StringBuilder pErrMsg);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_ReadIDInfo", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_ReadIDInfo(int ReaderHandle, string pBmpFile, StringBuilder pName,
            StringBuilder pSex, StringBuilder pNation, StringBuilder pBirth, StringBuilder pAddress,
            StringBuilder pCertNo, StringBuilder pDepartment, StringBuilder pEffectData, StringBuilder pExpire,
            StringBuilder pErrMsg);

        #endregion

        #endregion

        #region[华大900 身份证读卡器]
        private const string DllPathHuaDa900 = "External\\HuaDa900\\HDstdapi.dll";
        public static readonly Dictionary<int, string> HuaDa900_ErrorDictionary = new Dictionary<int, string>
        {
            [0] = "执行成功",
            [-1] = "设备连接错",
            [-2] = "设备未建立连接(没有执行打开设备函数)",
            [-3] = "(动态库)加载失败",
            [-4] = "(发给动态库的)参数错",
            [-5] = "寻卡失败",
            [-6] = "选卡失败",
            [-7] = "读卡失败",
            [-8] = "读取追加信息失败",
            [-9] = "读取追加信息失败",
            [-10] = "管理通信失败",
            [-11] = "检验通信失败",
            [-12] = "管理通信模块不支持获取指纹",
            [-999] = "其他异常错误",
        };
        [DllImport(DllPathHuaDa900, EntryPoint = "HD_InitComm", CharSet = CharSet.Ansi)]
        public static extern int HD_InitComm(int port);
        [DllImport(DllPathHuaDa900, EntryPoint = "HD_CloseComm", CharSet = CharSet.Ansi)]
        public static extern int HD_CloseComm(int port);
        [DllImport(DllPathHuaDa900, EntryPoint = "HD_Authenticate", CharSet = CharSet.Ansi)]
        public static extern int HD_Authenticate();
        [DllImport(DllPathHuaDa900, EntryPoint = "HD_Read_BaseInfo", CharSet = CharSet.Ansi)]
        public static extern int HD_Read_BaseInfo(string pBmpFile, StringBuilder pBmpData, StringBuilder pName, StringBuilder pSex, StringBuilder pNation, StringBuilder pBirth, StringBuilder pAddress, StringBuilder pCertNo, StringBuilder pDepartment, StringBuilder pEffectData, StringBuilder pExpire);
        #endregion

        #region Act_F3发卡器

        private const string DllPathF3 = "External\\Act_F3\\F3API.dll";
        public static readonly Dictionary<int, string> F3_ErrorDictionary = new Dictionary<int, string>
        {
            {0x0000, "操作成功"}, {0x3000, "指定的COM端口不存在,或者被其它程序占用"}, {0x3002, "未检测到设备"}, {0x3010, "通信错误"}, {0x3011, "通信超时"}, {0x3021, "检测到一个内部错误,但原因不明"}, {0x4010, "命令消息或接收的响应消息的长度超过了1024个字符"}, {0x4020, "没有足够的内存来完成当前的操作"}, {0x4000, "接收返回数据的缓冲区太小"}, {0x4001, "提供的句柄无效"}, {0x4E00, "未定义的命令"}, {0x4E01, "提供的一个或多个参数无效或者为NULL值"}, {0x4E02, "命令不能在当前的状态下执行"}, {0x4E03, "不支持的命令"}, {0x4E05, "IC触点未释放"}, {0x4E10, "卡片堵塞"}, {0x4E12, "传感器异常"}, {0x4E13, "插入到卡机内的卡片过长"}, {0x4E14, "插入到卡机内的卡片过短"}, {0x4E40, "回收卡时卡片被拿走"}, {0x4E41, "IC电磁线圈错误"}, {0x4E43, "不能移动卡到IC触点位"}, {0x4E45, "卡片位置被人为改变"}, {0x4E50, "回收卡计数器溢出"}, {0x4E51, "马达异常"}, {0x4E60, "IC卡供电电源短路"}, {0x4E61, "IC卡激活错误"}, {0x4E65, "IC卡未激活"}, {0x4E66, "不支持的IC卡"}, {0x4E67, "从IC卡接收数据时出错"}, {0x4E68, "IC卡通信超时"}, {0x4E69, "CPU/SAM卡不符合EMV2000规范"}, {0x4EA1, "卡箱空"}, {0x4EA2, "回收箱满"}, {0x4EB0, "等待复位"}, {0x6F00, "命令执行失败"}, {0x6F01, "校验卡时提供的校验码不正确"}, {0x6F02, "卡片已被锁"}, {0x6B00, "操作地址溢出"}, {0x6700, "操作长度溢出"},
        };
        public enum INIT_MODE : byte
        {
            /// <summary>
            /// 复位并移动卡到出卡口位 
            /// </summary>
            INIT_RETURN_TO_FRONT = 0x30,
            /// <summary>
            /// 复位并回收卡 
            /// </summary>
            INIT_CAPTURE_TO_BOX = 0x31,
            /// <summary>
            /// 复位，不移动卡 
            /// </summary>
            INIT_WITHOUT_MOVEMENT = 0x33,

        }

        public enum LaneStatus : byte
        {
            /// <summary>
            /// 机内无卡 
            /// </summary>
            LS_NO_CARD_IN = 0x30,
            /// <summary>
            /// 卡在出卡口位 
            /// </summary>
            LS_CARD_AT_GATE_POS = 0x31,
            /// <summary>
            /// 机内有卡
            /// </summary>
            LS_CARD_IN = 0x32,
        }

        public enum CardBoxStatus
        {
            /// <summary>
            /// 卡箱空
            /// </summary>
            CBS_EMPTY,

            /// <summary>
            /// 卡箱卡少 
            /// </summary>
            CBS_INSUFFICIENT,

            /// <summary>
            /// 卡箱卡足
            /// </summary>
            CBS_ENOUGH
        }

        public enum CardPostition : byte
        {
            /// <summary>
            /// 移动卡到前端持卡位 
            /// </summary>
            MM_RETURN_TO_FRONT = 0x30,
            /// <summary>
            ///  移动卡到IC位 
            /// </summary>
            MM_RETURN_TO_IC_POS = 0x31,
            /// <summary>
            /// 移动卡到射频位 
            /// </summary>
            MM_RETURN_TO_RF_POS = 0x32,
            /// <summary>
            /// 回收卡 
            /// </summary>
            MM_CAPTURE_TO_BOX = 0x33,
            /// <summary>
            /// 从前端弹出卡片 
            /// </summary>
            MM_EJECT_TO_FRONT = 0x39,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRSTATUS
        {
            //[MarshalAs(UnmanagedType.U1)]
            public LaneStatus bLaneStatus;

            //[MarshalAs(UnmanagedType.U1)]
            public CardBoxStatus bCardBoxStatus;

            //[MarshalAs(UnmanagedType.U1)]
            public bool fCaptureBoxFull;
        }
        [DllImport(DllPathF3, EntryPoint = "F3_Connect", CharSet = CharSet.Ansi)]
        public static extern int F3_Connect(int dwPort, int dwSpeed, byte bCRAddr, ref int lphReader);
        [DllImport(DllPathF3, EntryPoint = "F3_Initialize", CharSet = CharSet.Ansi)]
        public static extern int F3_Initialize(int hReader, INIT_MODE mode, bool fEnableCounter, byte[] pszRevBuff, ref int pcbRevLength);

        [DllImport(DllPathF3, EntryPoint = "F3_Disconnect", CharSet = CharSet.Ansi)]
        public static extern int F3_Disconnect(int hReader);
        [DllImport(DllPathF3, EntryPoint = "F3_MoveCard", CharSet = CharSet.Ansi)]
        public static extern int F3_MoveCard(int hReader, CardPostition bMode);

        [DllImport(DllPathF3, EntryPoint = "F3_MfVerifyPassword", CharSet = CharSet.Ansi)]
        public static extern int F3_MfVerifyPassword(int hReader, byte bSectorNumber, bool fWithKeyA, byte[] bKeyBytes);

        [DllImport(DllPathF3, EntryPoint = "F3_MfReadSector", CharSet = CharSet.Ansi)]
        public static extern int F3_MfReadSector(int hReader, byte bSectorNumber, byte bBlockNumber, byte[] pbBuffer,
            ref uint pcbLength);

        [DllImport(DllPathF3, EntryPoint = "F3_MfWriteSector", CharSet = CharSet.Ansi)]
        public static extern int F3_MfWriteSector(int hReader, byte bSectorNumber, byte bBlockNumber, byte bBytesToWrite,
            byte[] pbBuffer);

        [DllImport(DllPathF3, EntryPoint = "F3_GetCRStatus", CharSet = CharSet.Ansi)]
        public static extern int F3_GetCRStatus(int hReader, IntPtr lpStatus);
        [DllImport(DllPathF3, EntryPoint = "F3_GetCRStatus", CharSet = CharSet.Ansi)]
        public static extern int F3_GetCRStatus(int hReader, ref CRSTATUS lpStatus);

        #region RF Card

        public enum RFCTYPE : byte
        {

            RFCTYPE_UNKNOWN = 0xFF,
            RFCTYPE_MIFARE_S50 = 0x10,
            RFCTYPE_MIFARE_S70 = 0x11,
            RFCTYPE_MIFARE_UL = 0x12,
            RFCTYPE_TYPEA_CPU = 0x20,
            RFCTYPE_TYPEB_CPU = 0x30,

        }

        public enum RFC_PROTOCOL : byte
        {
            RFC_PROTOCOL_NONE = 0x30,
            RFC_PROTOCOL_TYPE_A = 0x41,
            RFC_PROTOCOL_TYPE_B = 0x42,
        }

        [DllImport(DllPathF3, EntryPoint = "F3_DetectRfcType", CharSet = CharSet.Ansi)]
        public static extern int F3_DetectRfcType(int hReader, ref RFCTYPE cardType);
        [DllImport(DllPathF3, EntryPoint = "F3_RfcActivate", CharSet = CharSet.Ansi)]
        public static extern int F3_RfcActivate(int hReader, byte[] pbATRBuff, ref int pbATRLength, RFC_PROTOCOL firstProtocol, RFC_PROTOCOL secondProtocol);
        [DllImport(DllPathF3, EntryPoint = "F3_TypeACpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int F3_TypeACpuTransmit(int hReader, byte[] pbSendBuff, ushort cbSendLength, byte[] pbRecvBuff, ref int pcbRecvLength);
        [DllImport(DllPathF3, EntryPoint = "F3_TypeBCpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int F3_TypeBCpuTransmit(int hReader, byte[] pbSendBuff, int cbSendLength, byte[] pbRecvBuff, ref int pcbRecvLength);


        #endregion
        #endregion

        #region 爱丽丝KC100
        private const string DllPathKC100 = "External\\KC100\\hzfdmt_32.dll";

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baud"></param>
        /// <returns></returns>
        [DllImport(DllPathKC100, EntryPoint = "open_device", CharSet = CharSet.Ansi)]
        public static extern int Mt_Open(string port, int baud);

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(DllPathKC100, EntryPoint = "close_device", CharSet = CharSet.Ansi)]
        public static extern int Mt_Close(int handle);

        /// <summary>
        /// 检测卡状态
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="cardStatus"></param>
        /// <returns></returns>
        [DllImport(DllPathKC100, EntryPoint = "dev_cardstate", CharSet = CharSet.Ansi)]
        public static extern int Mt_CheckStatus(int handle, byte[] cardStatus);

        /// <summary>
        /// 射频复位
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [DllImport(DllPathKC100, EntryPoint = "rf_reset", CharSet = CharSet.Ansi)]
        public static extern int Mt_Reset(int handle);

        /// <summary>
        /// 寻卡
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="nMode">0: 全部卡片都响应该命令，除了在HALT 状态的那些卡片1: 在任何状态的全部卡片都将响应该命令</param>
        /// <param name="sSnr"></param>
        /// <returns></returns>
        [DllImport(DllPathKC100, EntryPoint = "rf_card", CharSet = CharSet.Ansi)]
        public static extern int Mt_card(int handle, byte nMode, byte[] sSnr);

        private const string DllPathFangDian = "External\\KC100\\FangDian.dll";

        [DllImport(DllPathFangDian, EntryPoint = "FDSetCardSize", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_SetCardSize(Int32 i32Width, Int32 i32Height);

        [DllImport(DllPathFangDian, EntryPoint = "FDOpenPrinter", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_OpenPrinter();

        [DllImport(DllPathFangDian, EntryPoint = "FDClosePrinter", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_ClosePrinter();

        [DllImport(DllPathFangDian, EntryPoint = "FDGetStatus", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_GetStatus(out UInt32 ui32Status, out UInt32 ui32Config, out UInt32 ui32Information, out UInt32 ui32Warning, out UInt32 ui32Error, out UInt32 ui32Ext1, out UInt32 ui32Ext2, out UInt32 ui32Ext3, out UInt32 ui32Ext4);

        [DllImport(DllPathFangDian, EntryPoint = "FDMoveCard", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_MoveCard(Int32 i32Position);

        [DllImport(DllPathFangDian, EntryPoint = "FDReadMag", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_ReadMag(ref byte Trace1, ref byte Trace2, ref byte Trace3, int iSize);

        [DllImport(DllPathFangDian, EntryPoint = "FDWriteMag", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_WriteMag(String strTrack1, String strTrack2, String strTrack3);

        [DllImport(DllPathFangDian, EntryPoint = "FDPrintText", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_PrintText(int x, int y, String strText, String strFont, Int32 i32FontSize, Int32 i32FontWeight, UInt32 ui32Color, bool bFront);

        [DllImport(DllPathFangDian, EntryPoint = "FDPrintImage", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_PrintImage(String strFileName, int x, int y, int width, int height, UInt32 ui32Rop, bool bFront);

        [DllImport(DllPathFangDian, EntryPoint = "FDSaveImage", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_SaveImage(String strFileName, bool bFront);

        [DllImport(DllPathFangDian, EntryPoint = "FDPrintFlush", CharSet = CharSet.Unicode)]
        public static extern UInt32 FangDian_PrintFlush();

        [DllImport(DllPathFangDian, EntryPoint = "FDGetCardsRemainning", CharSet = CharSet.Unicode)]
        public static extern Int32 FangDian_GetCardsRemainning();

        #region 常数
        public const UInt32 ERROR_SUCCESS = 0;
        public const UInt32 ERROR_EVOLIS = 0xE3000000;
        public const UInt32 TYPEOF_PRIMACY = 0x80000000;
        public const UInt32 TYPEOF_ZENIUS = 0x40000000;
        public const UInt32 TYPEOF_ELYPSO = 0x10000000;


        // ERROR
        public const UInt32 ERR_BLANK_TRACK = 0x00080000;   // ERROR "磁条编码故障，请检查送卡器的卡片位置"
        public const UInt32 ERR_COVER_OPEN = 0x02000000;   // ERROR "盖子被打开"
        public const UInt32 ERR_HEAD_TEMP = 0x20000000;   // ERROR "打印头温度过高"
        public const UInt32 ERR_MAGNETIC_DATA = 0x00040000;   // ERROR "磁条编码故障，无效数据格式"
        public const UInt32 ERR_MECHANICAL = 0x01000000;   // ERROR "机械故障"
        public const UInt32 ERR_READ_MAGNETIC = 0x00020000;   // ERROR "磁道读取故障"
        public const UInt32 ERR_REJECT_BOX_FULL = 0x00800000;   // ERROR "废料箱满"
        public const UInt32 ERR_RIBBON_ERROR = 0x04000000;   // ERROR "色带已剪切或与卡片粘合"
        public const UInt32 ERR_WRITE_MAGNETIC = 0x00010000;   // ERROR "磁条写后读故障"
        public const UInt32 DEF_FEEDER_EMPTY = 0x01000000;   // WARNING "送卡器问题，请检查送卡器的卡片位置并进行隔距调整"
        public const UInt32 INF_WRONG_ZONE_EXPIRED = 0x00000008;   // INFORMATION "色带无效或已达到打印次数限制"
        public const UInt32 ERR_RIBBON_ENDED = 0x00200000;   // ERROR "色带耗尽，请更换新色带"
        public const UInt32 ERR_LAMINATE_END = 0x00000010;   // EXT2 "薄膜用完。请更换薄膜"
        public const UInt32 ERR_LAMINATE = 0x00000040;   // EXT2 "薄膜已剪切或与卡片粘连"
        public const UInt32 ERR_LAMI_MECHANICAL = 0x00000020;   // EXT2 "覆膜模块中发生机械错误"
        public const UInt32 ERR_LAMI_TEMPERATURE = 0x00000080;   // EXT2 "覆膜模块出现温度错误"
        public const UInt32 ERR_LAMI_COVER_OPEN = 0x00000008;   // EXT2 "覆膜模块机盖在覆膜过程中打开,请将其关闭并重试"

        // READY
        public const UInt32 INF_CLEANING = 0x00020000;   // INFORMATION "打印机需要清洁"
        public const UInt32 INF_CLEAN_2ND_PASS = 0x00002000;   // INFORMATION "请插入您的粘附式清洁卡,如果您想要继续打印则‘取消’"
        public const UInt32 INF_CLEANING_RUNNING = 0x00000020;   // INFORMATION "正在清洁"
        public const UInt32 INF_ENCODING_RUNNING = 0x00000040;   // INFORMATION "正在编码"
        public const UInt32 INF_PRINTING_RUNNING = 0x00000080;   // INFORMATION "正在打印"
        public const UInt32 INF_LAMINATING_RUNNING = 0x00020000;   // INFORMATION "正在覆膜"
        public const UInt32 INF_LAMI_CLEANING_RUNNING = 0x00080000;   // EXT2 "覆膜模块正在清洁"
        public const UInt32 INF_SLEEP_MODE = 0x00200000;   // INFORMATION "打印机处于待机状态"

        // WARNING
        public const UInt32 INF_LOW_RIBBON = 0x00080000;   // INFORMATION "色带快耗尽了，请补充后继续"
        public const UInt32 INF_FEEDER_NEAR_EMPTY = 0x00000100;   // EXT1 "送卡器的卡片即将用尽，请重新装入卡片"
        public const UInt32 DEF_COOLING = 0x00200000;   // WARNING "打印机正在冷却"
        public const UInt32 DEF_COVER_OPEN = 0x00020000;   // WARNING "请关闭您的打印机盖"
        public const UInt32 DEF_HOPPER_FULL = 0x00100000;   // WARNING "输出托盒满,请从输出托盒取出所有打印卡片以继续打印操作"
        public const UInt32 DEF_NO_RIBBON = 0x00010000;   // WARNING "无色带"
        public const UInt32 DEF_PRINTER_LOCKED = 0x00040000;   // WARNING "与打印机的通信被锁定"
        public const UInt32 DEF_UNSUPPORTED_RIBBON = 0x00008000;   // WARNING "插入的色带不适用于该打印机型号"
        public const UInt32 DEF_WAIT_CARD = 0x02000000;   // WARNING "请手动插入您的卡片"
        public const UInt32 INF_CLEANING_ADVANCED = 0x00000800;   // INFORMATION "需要打印机高级清洁"
        public const UInt32 INF_CLEAN_LAST_OUTWARRANTY = 0x00004000;   // INFORMATION "必须定期清洁，请点击‘取消’并立即清洁"
        public const UInt32 INF_CLEANING_REQUIRED = 0x00000100;   // INFORMATION "必须定期清洁——您的管理员不允许发卡"
        public const UInt32 INF_UNKNOWN_RIBBON = 0x00100000;   // INFORMATION "无法识别色带请使用手动设置继续"
        public const UInt32 INF_WRONG_ZONE_ALERT = 0x00000010;   // INFORMATION "色带和打印机存在兼容性问题。只剩少于 50 次打印输出。请联系经销商。"
        public const UInt32 INF_WRONG_ZONE_RIBBON = 0x00000400;   // INFORMATION "色带和打印机存在兼容性问题。请联系经销商。"
        public const UInt32 DEF_RIBBON_ENDED = 0x00080000;   // WARNING "色带耗尽，请更换新色带。"
        public const UInt32 DEF_NO_LAMINATE = 0x00008000;   // EXT2 "覆膜模块中没有薄膜。请更换薄膜"
        public const UInt32 INF_LAMINATE_UNKNOWN = 0x00400000;   // EXT2 "未知薄膜。请联系经销商"
        public const UInt32 INF_LAMINATE_LOW = 0x00200000;   // EXT2 "薄膜即将用尽。请安排补充。"
        public const UInt32 DEF_LAMINATE_END = 0x00002000;   // EXT2 "薄膜用完。请更换薄膜。"
        public const UInt32 DEF_LAMINATE_UNSUPPORTED = 0x00000800;   // EXT2 "薄膜与覆膜模块不兼容。请联系经销商"
        public const UInt32 DEF_LAMI_COVER_OPEN = 0x00004000;   // EXT2 "覆膜模块的机盖打开。关闭覆膜模块机盖。"
        public const UInt32 INF_LAMI_TEMP_NOT_READY = 0x00008000;   // EXT1 "薄膜模块温度正在调整。请稍候.."
        public const UInt32 DEF_LAMI_HOPPER_FULL = 0x00001000;   // EXT2 "覆膜输出受阻。请取出卡片并重试。"
        #endregion
        #endregion
    }
}
