using System.Runtime.InteropServices;

namespace YuanTu.PanYu.House.CardReader.HuaDa
{
    public class M1
    {
        public static int readerHandle => PanYu.House.CardReader.HuaDa.Common.readerHandle;
        private const string DllPathHuaDa = "External\\HuaDa\\SSSE32.dll";

        #region M1卡基本操作函数

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Request", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Request(int ReaderHandle);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_anticoll", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_anticoll(int ReaderHandle, byte[] uid);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Select", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Select(int ReaderHandle, byte cardtype);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Authentication", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Authentication(int ReaderHandle, byte Mode, byte SecNr);

        [DllImport(DllPathHuaDa, EntryPoint = "PICC_Reader_Authentication_Pass", CharSet = CharSet.Ansi)]
        public static extern int PICC_Reader_Authentication_Pass(int ReaderHandle, byte Mode, byte SecNr, byte[] PassWord);

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
        /// <summary>
        /// 请求卡片
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderRequest()
        {
            return PICC_Reader_Request(readerHandle) == 0;
        }
        /// <summary>
        /// 防碰撞、读序列号
        /// </summary>
        /// <param name="uid">序列号</param>
        /// <returns></returns>
        public static bool PICCReaderAnticoll(out byte[] uid)
        {
            var Uid = new byte[10];
            var Ret = PICC_Reader_anticoll(readerHandle, Uid) == 0;
            uid = Uid;
            return Ret;
        }

        /// <summary>
        /// 选择卡片类型
        /// </summary>
        /// <param name="CardType"></param>
        /// <returns></returns>
        public static bool PICCReaderSelect(cardType CardType)
        {
            return PICC_Reader_Select(readerHandle, (byte)CardType) == 0;
        }

        /// <summary>
        /// M1卡认证密钥,自动调用存储于设备里面的Key进行认证
        /// </summary>
        /// <param name="Mode"></param>
        /// <param name="Sec"></param>
        /// <returns></returns>
        public static bool PICCReaderAuthentication(mode Mode, byte Sec)
        {
            return PICC_Reader_Authentication(readerHandle, (byte)Mode, Sec) == 0;
        }

        /// <summary>
        /// M1卡认证密钥,该函数将使用参数PassWord传入的Key进行认证
        /// </summary>
        /// <param name="Mode">选择keyA或keyB</param>
        /// <param name="Sec">扇区</param>
        /// <param name="PassWord">key value</param>
        /// <returns></returns>
        public static bool PICCReaderAuthenticationPass(mode2 Mode, byte Sec, byte[] PassWord)
        {
            return PICC_Reader_Authentication_Pass(readerHandle, (byte)Mode, Sec, PassWord) == 0;
        }

       /// <summary>
       /// 读M1卡片块数据
       /// </summary>
       /// <param name="M1Addr"></param>
       /// <param name="data"></param>
       /// <returns></returns>
        public static bool PICCReaderRead(byte Addr, out byte[] data)
        {
            var Data = new byte[20];
            var Ret = PICC_Reader_Read(readerHandle, Addr, Data) == 0;
            data = Data;
            return Ret;
        }

        /// <summary>
        /// 读M1卡片块数据Hex
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool PICCReaderReadHex(byte Addr, out byte[] data)
        {
            var Data = new byte[20];
            var Ret = PICC_Reader_ReadHEX(readerHandle, Addr, Data) == 0;
            data = Data;
            return Ret;
        }

        /// <summary>
        /// 写M1卡片块数据
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool PICCReaderWrite(byte Addr, byte[] Data)
        {
            return PICC_Reader_Write(readerHandle, Addr, Data) == 0;
        }
        /// <summary>
        /// 写M1卡片块数据Hex
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static bool PICCReaderWriteHex(byte Addr, byte[] Data)
        {
            return PICC_Reader_WriteHEX(readerHandle, Addr, Data) == 0;
        }

        /// <summary>
        /// 装载密钥至设备里
        /// </summary>
        /// <param name="Mode">keyA或keyB</param>
        /// <param name="Sec">扇区</param>
        /// <param name="Key">key value</param>
        /// <returns></returns>
        public static bool PICCReaderLoadKey(mode Mode, byte Sec, byte[] Key)
        {
            return PICC_Reader_LoadKey(readerHandle, (byte)Mode, Sec, Key) == 0;
        }
        /// <summary>
        /// 装载密钥至设备里Hex
        /// </summary>
        /// <param name="Mode">keyA或keyB</param>
        /// <param name="Sec">扇区</param>
        /// <param name="KeyHex">key value</param>
        /// <returns></returns>
        public static bool PICCReaderLoadKeyHex(mode Mode, byte Sec, byte[] KeyHex)
        {
            return PICC_Reader_LoadKeyHEX(readerHandle, (byte)Mode, Sec, KeyHex) == 0;
        }
    }
}