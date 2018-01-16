using System;
using System.Runtime.InteropServices;


namespace YuanTu.PanYu.House.CardReader.HuaDa
{
    public  class CPU
    {
        public static int readerHandle => PanYu.House.CardReader.HuaDa.Common.readerHandle;
        private const string DllPathHuaDa = "External\\HuaDa\\SSSE32.dll";
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
        public static extern int ICC_Reader_Application(int ReaderHandle, byte ICC_Slot_No, int Lenth_of_Command_APDU, byte[] Command_APDU, byte[] Response_APDU);

        [DllImport(DllPathHuaDa, EntryPoint = "ICC_Reader_ApplicationHEX", CharSet = CharSet.Ansi)]
        public static extern int ICC_Reader_ApplicationHEX(int ReaderHandle, byte ICC_Slot_No, int Lenth_of_Command_APDU, byte[] Command_APDU, byte[] Response_APDU);

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
        public static extern int PICC_Reader_Application(int ReaderHandle, int Lenth_of_Command_APDU, byte[] Command_APDU, byte[] Response_APDU);
        #endregion

        #region 非接CPU卡
        /*typeACPU 卡操作顺序
        1 设置为TypeA卡片
        2 请求卡片
        3 防碰撞
        4 选择卡片
        5 上电
        6 APDU 命令*/

        /*typeB 卡操作顺序
        1 设置为TypeB卡片
        2 上电
        3 选卡
        4 APDU 命令*/

        /// <summary>
        /// 设置读TypeA CPU卡
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderSetTypeA()
        {
            return PICC_Reader_SetTypeA(readerHandle)==0;
        }

        /// <summary>
        /// 设置读TypeB CPU卡
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderSetTypeB()
        {
            return PICC_Reader_SetTypeB(readerHandle) == 0;
        }

        /// <summary>
        /// 请求卡片
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderRequest()
        {
          return  PICC_Reader_Request(readerHandle)==0;
        }

        /// <summary>
        /// 防碰撞、读序列号
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static bool PICCReaderAnticoll(out byte[] uid)
        {
            var Uid = new byte[10];
            var Ret = PICC_Reader_anticoll(readerHandle, Uid) == 0;
            uid = Uid;
            return Ret;
        }

        /// <summary>
        /// 选择卡片M1/TypeA/TypeB
        /// </summary>
        /// <param name="CardType"></param>
        /// <returns></returns>
        public static bool PICCReaderSelect(cardType CardType)
        {
            return PICC_Reader_Select(readerHandle, (byte)CardType) == 0;
        }

        /// <summary>
        /// 非接TypeA CPU上电复位
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderPowerOnTypeA()
        {
            var res = new byte[32];
            return PICC_Reader_PowerOnTypeA(readerHandle, res) > 0;
        }

        /// <summary>
        /// 非接TypeB CPU上电复位
        /// </summary>
        /// <returns></returns>
        public static bool PICCReaderPowerOnTypeB()
        {
            var res = new byte[32];
            return PICC_Reader_PowerOnTypeB(readerHandle, res) > 0;
        }

        /// <summary>
        /// TypeA/B 非接 CPU 卡执行 apdu 命令
        /// </summary>
        /// <param name="APDU"></param>
        /// <param name="RES"></param>
        /// <returns></returns>
        public static bool PICCReaderApplication(byte[] APDU,out byte[] RES)
        {
            var res = new byte[32];
            var Ret= PICC_Reader_Application(readerHandle, APDU.Length,APDU, res)>0;
            RES = res;
            return Ret;
        }


        /// <summary>
        /// TypeA/B 非接 CPU 卡执行 apdu 命令
        /// </summary>
        /// <param name="APDU"></param>
        /// <param name="RES"></param>
        /// <returns></returns>
        public static bool PICCReaderApplication(string APDU, out string RES)
        {
            int sendlen = APDU.Length / 2;
            var sendbuff = new byte[sendlen];
           
            var recvbuff = new byte[256];
            //var recvlen = 256;

            for (var i = 0; i < APDU.Length; i += 2)
            {
                sendbuff[i / 2] = Convert.ToByte(16 * Char2Byte(APDU[i]) + Char2Byte(APDU[i + 1]));
                //sendlen++;
            }
            var Ret = PICC_Reader_Application(readerHandle, sendlen, sendbuff, recvbuff) > 0;
            RES = Bytes2String(recvbuff, recvbuff.Length);
            return Ret;
        }
        private static byte Char2Byte(char c)
        {
            if (c >= '0' && c <= '9')
                return Convert.ToByte(c - '0');
            if (c >= 'a' && c <= 'f')
                return Convert.ToByte(c - 'a' + 10);
            if (c >= 'A' && c <= 'F')
                return Convert.ToByte(c - 'A' + 10);
            return Convert.ToByte(-1);
        }
        public static string Bytes2String(byte[] buff, int len)
        {
            var text = "";
            for (var i = 0; i < len; i++)
            {
                text += string.Format("{0:X2}", buff[i]);
            }
            return text;
        }
        #endregion

        #region 接触CPU卡
        /// <summary>
        /// 接触 CPU 卡上电复位
        /// </summary>
        /// <param name="SlotNo">卡座选择一般为大卡座</param>
        /// <returns></returns>
        public static bool ICCReaderPowerOn(slotNo SlotNo)
        {
            var res = new byte[256];
           return ICC_Reader_PowerOn(readerHandle,(byte)SlotNo, res)>0;
        }

        /// <summary>
        /// 接触 CPU 卡执行 apdu 命令
        /// </summary>
        /// <param name="SlotNo">卡座选择一般为大卡座</param>
        /// <param name="APDU"></param>
        /// <param name="ResAPDU"></param>
        /// <returns></returns>
        public static bool ICCReaderApplication(slotNo SlotNo,byte[] APDU,out byte[] ResAPDU)
        {
            var res = new byte[32];
            var Ret= ICC_Reader_Application(readerHandle, (byte)SlotNo, APDU.Length,APDU, res);
            ResAPDU = res;
            return Ret>0;
        }
        #endregion
    }
}
