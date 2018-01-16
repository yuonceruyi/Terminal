using System.Runtime.InteropServices;
using System.Text;


namespace YuanTu.PanYu.House.CardReader.HuaDa
{
    public  class Common
    {
        private const string DllPathHuaDa = "External\\HuaDa\\SSSE32.dll";
        public static int readerHandle { get; set; }
        public static string dev_Name { get; set; } = "USB1";
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
        public static extern int ICC_Reader_GetDeviceVersion(int ReaderHandle, byte[] VSoftware, byte[] VHardware, byte[] VBoot, byte[] VDate);

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

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        public static bool ReaderOpen()
        {
            readerHandle = ICC_Reader_Open(dev_Name);
            return readerHandle > 0;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        public static bool ReaderClose()
        {
            return ICC_Reader_Close(readerHandle)==0;
        }

        /// <summary>
        /// 发出蜂鸣
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static bool ReaderPosBeep(byte time)
        {
            return ICC_PosBeep(readerHandle, time)>=0;
        }

        /// <summary>
        /// 获取磁卡输出模式
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public static bool ReaderGetMagCardMode(out int mode,out int track)
        {
            mode = 0;
            track = 0;
           return ICC_Reader_GetMagCardMode(readerHandle,ref mode,ref track)==0;
        }

        /// <summary>
        /// 设置磁卡输出模式
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        public static bool ReaderSetMagCardMode(magMode mode,  int track)
        {
           return ICC_Reader_SetMagCardMode(readerHandle,(int) mode,  track) == 0;
        }

        /// <summary>
        /// 语音播报
        /// </summary>
        /// <param name="VoiceType"></param>
        /// <param name="VoiceMode"></param>
        /// <returns></returns>
        public static bool ICCDispSound(voiceType VoiceType,voiceMode VoiceMode)
        {
           return ICC_DispSound(readerHandle,(byte)VoiceType, (byte)VoiceMode)==0;
        }


    }
}
