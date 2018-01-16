using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using YuanTu.Core.Log;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583.External
{
    public class ACT_A6_V2
    {
        protected static int handle;
        public static int Port { get; set; } = 1;
        public static int Baud { get;set; }= 9600;

        public static bool Connected { get; private set; }

        public static LoggerWrapper Log { get; set; } = Logger.POS;

        public static bool Init()
        {
            if (Connected)
                return true;

            if (!Connect(Port, Baud))
                return false;

            if (!Initialize())
                return false;

            return true;
        }

        public static bool Uninit()
        {
            if (!Connected)
                return true;

            if (!EnterCard(FCI.禁止))
                return false;

            if (!Disconnect())
                return false;

            return true;
        }

        public static bool MoveOut()
        {
            if (!Connect(Port, Baud))
                return false;

            if (!MoveCard(MOVE_TO.前端不持卡))
                return false;

            return Uninit();
        }

        #region Enums

        public enum CardPos : byte
        {
            未知 = 0x00,
            长卡 = 0x46,
            短卡 = 0x47,
            不持卡位 = 0x48,
            持卡位 = 0x49,
            停卡位 = 0x4A,
            IC位 = 0x4B,
            后端持卡位 = 0x4C,
            后端不持卡位 = 0x4D,
            无卡 = 0x4E
        }

        public enum FCI : byte
        {
            禁止 = 0x31,
            仅磁卡 = 0x32,
            允许 = 0x33
        }

        public enum ICCTYPE : byte
        {
            UNKNOWN = 0x0,
            MIFARE_S50 = 0x10,
            MIFARE_S70 = 0x11,
            MIFARE_UL = 0x12,
            TYPEA_CPU = 0x13,
            TYPEB_CPU = 0x14,
            T0_CPU = 0x20,
            T1_CPU = 0x21,
            AT24C01 = 0x30,
            AT24C02 = 0x31,
            AT24C04 = 0x32,
            AT24C08 = 0x33,
            AT24C16 = 0x34,
            AT24C32 = 0x35,
            AT24C64 = 0x36,
            SLE4442 = 0x40,
            SLE4428 = 0x41,
            AT88SC102 = 0x50,
            AT88SC1604 = 0x51,
            AT88SC1608 = 0x52,
            AT45DB041 = 0x53
        }

        public enum MOVE_TO : byte
        {
            非接 = 0x2E,
            IC = 0x2F,
            前端不持卡 = 0x30,
            前端持卡 = 0x31,
            后端持卡 = 0x32,
            后端不持卡 = 0x33
        }

        public enum RCI : byte
        {
            允许 = 0x30,
            禁止 = 0x31
        }

        public enum RESET : byte
        {
            仅复位 = 0x30,
            复位并弹卡 = 0x31,
            复位并吞卡 = 0x32
        }

        public enum DPOS : byte
        {
            FRONT_NH = 0x30,
            FRONT = 0x31,
            INTERNAL = 0x32,
            IC_POS = 0x33,
            REAR = 0x34,
            REAR_NH = 0x35
        }

        public enum ICC_PROTOCOL : byte
        {
            T0 = 0x31,
            T1 = 0x32
        }

        [Flags]
        public enum TRACKID : byte
        {
            ISO1 = 0x10,
            ISO2 = 0x20,
            ISO3 = 0x40,
            ISO23 = 0x60,
            ALL = 0x70
        }

        public enum TRACKST : byte
        {
            NORMAL = 0x60,
            NO_SS = 0xE1,
            NO_ES = 0xE2,
            PARITY_ERROR = 0xE3,
            LRC_ERROR = 0xE4,
            NO_DATA = 0xE5
        }

        public enum READ : byte
        {
            ASCII = 0x30,
            BINARY = 0x31
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TrackInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3*512)] public byte[] Contents;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] Lengths;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] Status;
        }

        #endregion Enums

        #region Wrapper

        public static bool Connect(int port, int baud)
        {
            if (Connected)
                return true;
            var ret = A6_Connect(port, baud, ref handle);
            if (ret != 0)
            {
                var log = "读卡器连接失败 端口：" + port + " 波特率：" + baud + " 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            Connected = true;
            return true;
        }

        public static bool Disconnect()
        {
            if (!Connected)
                return true;
            var ret = A6_Disconnect(handle);
            if (ret != 0)
            {
                var log = "读卡器断开连接失败 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            Connected = false;
            return true;
        }

        public static bool Initialize(RESET reset = RESET.仅复位)
        {
            if (!Connected)
                return false;
            var buffer = new byte[1024];
            var bufferLen = 1024;
            var ret = A6_Initialize(handle, (byte) reset, buffer, ref bufferLen);
            if (ret != 0)
            {
                var log = "读卡器初始化 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            return true;
        }

        public static bool MoveCard(MOVE_TO to)
        {
            if (!Connected)
                return false;
            var ret = A6_MoveCard(handle, (int) to);
            if (ret != 0)
            {
                var log = "读卡器退卡失败 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            return true;
        }

        public static bool EnterCard(FCI fci = FCI.允许, RCI rci = RCI.禁止)
        {
            var ret = A6_SetCardIn(handle, (byte) fci, (byte) rci);
            if (ret != 0)
            {
                var log = "读卡器设置进卡方式失败 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            return true;
        }

        public static bool SetDockedPos(DPOS pos = DPOS.IC_POS)
        {
            if (!Connected)
                return false;
            var ret = A6_SetDockedPos(handle, (byte) pos);
            if (ret != 0)
            {
                Log?.Debug("设置停卡位失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        public static bool CheckCardPos(out CardPos pos)
        {
            pos = CardPos.未知;
            if (!Connected)
                return false;
            var status = new byte[3];
            var ret = A6_GetCRCondition(handle, status);
            if (ret != 0)
            {
                var log = "检测卡类型失败 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            pos = (CardPos) status[0];
            return true;
        }

        public static bool DetectIccType(out ICCTYPE iccType)
        {
            iccType = ICCTYPE.UNKNOWN;
            if (!Connected)
                return false;
            byte type = 0;
            var ret = A6_DetectIccType(handle, ref type);
            if (ret != 0)
            {
                var log = "检测IC卡类型失败 错误代码：" + ret;
                Log?.Debug(log);
                return false;
            }
            iccType = (ICCTYPE) type;
            return true;
        }

        public static bool LedOn()
        {
            if (!Connected)
                return false;
            var ret = A6_LedOn(handle);
            if (ret != 0)
            {
                Log?.Debug("开启Led失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        public static bool LedOff()
        {
            if (!Connected)
                return false;
            var ret = A6_LedOff(handle);
            if (ret != 0)
            {
                Log?.Debug("关闭Led失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        /// <summary>
        ///     设置LED闪烁周期
        /// </summary>
        /// <param name="on">0.25s*on 亮灯时间</param>
        /// <param name="off">0.25s*off 灭灯时间</param>
        /// <returns></returns>
        public static bool LedBlink(byte on, byte off)
        {
            if (!Connected)
                return false;
            var ret = A6_LedBlink(handle, on, off);
            if (ret != 0)
            {
                Log?.Debug("Led闪烁设置失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        public static bool IccPowerOn()
        {
            if (!Connected)
                return false;
            var ret = A6_IccPowerOn(handle);
            if (ret != 0)
            {
                Log?.Debug("IC卡上电失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        public static bool IccPowerOff()
        {
            if (!Connected)
                return false;
            var ret = A6_IccPowerOff(handle);
            if (ret != 0)
            {
                Log?.Debug("IC卡下电失败 错误代码：" + ret);
                return false;
            }
            return true;
        }

        public static bool CpuColdReset(out byte[] apdu)
        {
            apdu = null;
            if (!Connected)
                return false;
            var data = new byte[256];
            var len = 256;
            var ret = A6_CpuColdReset(handle, data, ref len);
            if (ret != 0)
            {
                Log?.Debug("IC卡冷复位失败 错误代码：" + ret);
                return false;
            }
            apdu = new byte[len];
            Array.Copy(data, apdu, len);
            return true;
        }

        public static bool CpuTransmit(ICC_PROTOCOL protocol, byte[] send, out byte[] recv)
        {
            recv = new byte[1024];
            var recvLen = 1024;
            if (!Connected)
                return false;

            var ret = A6_CpuTransmit(handle, (byte) protocol, send, send.Length, recv, ref recvLen);
            if (ret != 0)
            {
                Log?.Debug("IC通信失败 错误代码：" + ret);
                return false;
            }
            recv = recv.Take(recvLen).ToArray();
            return true;
        }

        public static bool ReadTrack(int id, out string track)
        {
            track = string.Empty;
            uint t;
            switch (id)
            {
                case 1:
                    t = (uint) TRACKID.ISO1;
                    break;

                case 2:
                    t = (uint) TRACKID.ISO2;
                    break;

                case 3:
                    t = (uint) TRACKID.ISO3;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(id));
            }
            var trackInfo = new TrackInfo();
            var ret = A6_ReadTracks(handle, (byte) READ.ASCII, t, ref trackInfo);
            if (ret != 0)
            {
                Log?.Debug("读磁条 错误代码：" + ret);
                return false;
            }
            track = Encoding.ASCII.GetString(trackInfo.Contents, (id - 1)*512, trackInfo.Lengths[id - 1]);
            return true;
        }

        public static bool ReadTracks(int[] ids, out string[] tracks)
        {
            tracks = new string[ids.Length];
            uint t = 0;
            foreach (var id in ids)
            {
                switch (id)
                {
                    case 1:
                        t |= (uint) TRACKID.ISO1;
                        break;

                    case 2:
                        t |= (uint) TRACKID.ISO2;
                        break;

                    case 3:
                        t |= (uint) TRACKID.ISO3;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(id));
                }
            }
            var trackInfo = new TrackInfo();
            var ret = A6_ReadTracks(handle, (byte) READ.ASCII, t, ref trackInfo);
            if (ret != 0)
            {
                Log?.Debug("读磁条 错误代码：" + ret);
                return false;
            }
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                tracks[i] = Encoding.ASCII.GetString(trackInfo.Contents, (id - 1)*512, trackInfo.Lengths[id - 1]);
            }
            return true;
        }

        #endregion Wrapper

        #region DLL Import

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_Connect", CharSet = CharSet.Ansi)]
        public static extern int A6_Connect(
            int dwPort,
            int dwSpeed,
            ref int phReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_Disconnect", CharSet = CharSet.Ansi)]
        public static extern int A6_Disconnect(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_Initialize", CharSet = CharSet.Ansi)]
        public static extern int A6_Initialize(
            int hReader,
            byte bResetMode,
            byte[] pbVerBuff,
            ref int pcbVerLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_GetCRCondition", CharSet = CharSet.Ansi)]
        public static extern int A6_GetCRCondition(
            int hReader,
            byte[] pStatus
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SetCardIn", CharSet = CharSet.Ansi)]
        public static extern int A6_SetCardIn(
            int hReader,
            byte bFrontSet,
            byte bRearSet
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SetDockedPos", CharSet = CharSet.Ansi)]
        public static extern int A6_SetDockedPos(
            int hReader,
            byte bDockedPos
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_MoveCard", CharSet = CharSet.Ansi)]
        public static extern int A6_MoveCard(
            int hReader,
            int bMoveMethod
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_LedOn", CharSet = CharSet.Ansi)]
        public static extern int A6_LedOn(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_LedOff", CharSet = CharSet.Ansi)]
        public static extern int A6_LedOff(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_LedBlink", CharSet = CharSet.Ansi)]
        public static extern int A6_LedBlink(
            int hReader,
            byte bOnTime,
            byte bOffTime
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_DetectIccType", CharSet = CharSet.Ansi)]
        public static extern int A6_DetectIccType(
            int hReader,
            ref byte pbType
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_IccPowerOn", CharSet = CharSet.Ansi)]
        public static extern int A6_IccPowerOn(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_IccPowerOff", CharSet = CharSet.Ansi)]
        public static extern int A6_IccPowerOff(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_CpuColdReset", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuColdReset(
            int hReader,
            byte[] pbATRBuff,
            ref int pcbATRLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_CpuWarmReset", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuWarmReset(
            int hReader,
            byte[] pbATRBuff,
            ref int pcbATRLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_CpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuTransmit(
            int hReader,
            byte bProtocol,
            byte[] pbSendBuff,
            int cbSendLength,
            byte[] pbRecvBuff,
            ref int pcbRecvLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_ReadTracks", CharSet = CharSet.Ansi)]
        public static extern int A6_ReadTracks(
            int hReader,
            byte bMode,
            uint iTrackID,
            ref TrackInfo pTrackInfo
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_TypeACpuSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeACpuSelect
            (
            int hReader,
            byte[] pbATRBuff,
            ref int pcbATRLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_TypeBCpuSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeBCpuSelect(
            int hReader,
            byte[] pbATRBuff,
            ref int pcbATRLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_TypeABCpuDeselect", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuDeselect(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_TypeABCpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuTransmit(
            int hReader,
            byte[] pbSendBuff,
            ushort cbSendLength,
            byte[] pbRecvBuff,
            ref int pcbRecvLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_TypeACpuGetUID", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeACpuGetUID(
            int hReader,
            byte[] pbUIDBuff,
            ref int pcbUIDLength
            );

        //for M1 card
        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SxxSelect", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxSelect(
            int hReader
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SxxGetUID", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxGetUID(
            int hReader,
            byte[] pbUIDBuff,
            ref int pcbUIDLength
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SxxVerifyPassword", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxVerifyPassword(
            int hReader,
            byte sec,
            bool isKeyA,
            byte[] data
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SxxReadBlock", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxReadBlock(int hReader, byte sec, byte block, byte[] data);

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_SxxWriteBlock", CharSet = CharSet.Ansi)]
        public static extern int A6_SxxWriteBlock(int hReader, byte sec, byte block, byte[] data);

        #endregion DLL Import

        #region Original

        //public static void Init()
        //{
        //    var buff = new byte[1024];
        //    var bufflen = 1024;
        //    if (Connected)
        //        return;
        //    ret = A6_Connect(Port, Baud, ref handle);
        //    if (ret != 0)
        //    {
        //        var log = "读卡器连接失败 端口：" + Port + " 波特率：" + Baud + " 错误代码：" + ret;
        //        Logger.Device.Debug(log);
        //        throw new Exception(log);
        //    }

        //    ret = A6_Initialize(handle, RESET_ONLY, buff, ref bufflen);
        //    if (ret != 0)
        //    {
        //        var log = "读卡器初始化失败 错误代码：" + ret;
        //        Logger.Device.Debug(log);
        //        throw new Exception(log);
        //    }

        //    ret = A6_SetCardIn(handle, FCI_ALLOWED, RCI_PROHIBITED);
        //    if (ret != 0)
        //    {
        //        var log = "读卡器设置进卡方式失败 错误代码：" + ret;
        //        Logger.Device.Debug(log);
        //        throw new Exception(log);
        //    }
        //    Connected = true;
        //}

        public static void EnableCard(bool isEnable)
        {
            A6_SetCardIn(handle, isEnable ? FCI_ALLOWED : RCI_PROHIBITED, RCI_PROHIBITED);
        }

        //public static void Uninit()
        //{
        //    //ret = A6_SetCardIn(handle, FCI_PROHIBITED, RCI_PROHIBITED);
        //    //if (ret != 0)
        //    //{
        //    //	var log = "读卡器设置进卡方式失败 错误代码：" + ret;
        //    //	Logger.log.Debug(log);
        //    //}

        //    ret = A6_Disconnect(handle);
        //    if (ret != 0)
        //    {
        //        var log = "读卡器断开连接失败 错误代码：" + ret;
        //        Logger.Device.Debug(log);
        //    }
        //    Connected = false;
        //}

        //public static void MoveOut()
        //{
        //    ret = A6_MoveCard(handle, MOVE_TO_FRONT_NH);
        //    if (ret != 0)
        //    {
        //        var log = "读卡器退卡失败 错误代码：" + ret;
        //        Logger.Device.Debug(log);
        //        throw new Exception(log);
        //    }
        //}

        public static bool HaveCard()
        {
            var pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (CRSTATUS)));
            var ret = A6_GetCRCondition(handle, pt);
            if (ret != 0)
            {
                return false;
            }
            var st = (CRSTATUS) Marshal.PtrToStructure(pt, typeof (CRSTATUS));
            if (st.bLaneStatus == LS_CARD_IN_RF_POS
                || st.bLaneStatus == LS_CARD_IN_IC_POS)
            {
                return true;
            }
            return false;
        }

        public static void ReadTrack2(out string cardNo)
        {
            cardNo = "";
            var pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof (TRACKINFO)));
            A6_ReadTracks(handle, 0x30, 0x20, pt);
            var st = (TRACKINFO) Marshal.PtrToStructure(pt, typeof (TRACKINFO));
            var len = st.Lengths[1];
            if (len > 0)
            {
                var realNo = new char[len];
                Array.Copy(st.Contents, 512, realNo, 0, len);
                cardNo = new string(realNo);
            }

            //cardNo = new string(st.Contents[1]);
        }

        public static string SelectTypeACpu()
        {
            var buff = new byte[256];
            var bufflen = 256;
            ret = A6_TypeACpuSelect(handle, buff, ref bufflen);
            return Bytes2String(buff, bufflen);
        }

        public static string Transmit(string text)
        {
            var sendbuff = new byte[256];
            ushort sendlen = 0;
            var recvbuff = new byte[256];
            var recvlen = 256;

            for (var i = 0; i < text.Length; i += 2)
            {
                sendbuff[i/2] = Convert.ToByte(16*Char2Byte(text[i]) + Char2Byte(text[i + 1]));
                sendlen++;
            }

            ret = A6_TypeABCpuTransmit(handle, sendbuff, sendlen, recvbuff, ref recvlen);
            return Bytes2String(recvbuff, recvlen);
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

        public static bool DetectCard()
        {
            if (A6_SxxSelect(handle) != 0)
                return false;
            return true;
        }

        #endregion
        
        #region DLL Import

        public static int ret;

        public const byte RESET_ONLY = 0x30;
        public const byte RESET_AND_EJECT = 0x31;
        public const byte RESET_AND_CAPTURE = 0x32;
        public const byte FCI_PROHIBITED = 0x31;
        public const byte FCI_MAGCARD_ONLY = 0x32;
        public const byte FCI_ALLOWED = 0x33;
        public const byte RCI_ALLOWED = 0x30;
        public const byte RCI_PROHIBITED = 0x31;
        public const byte MOVE_TO_FRONT_NH = 0x30;
        public const byte MOVE_TO_FRONT = 0x31;
        public const byte MOVE_TO_RF_POS = 0x2E;
        public const byte MOVE_TO_IC_POS = 0x2F;
        public const byte MOVE_TO_REAR = 0x32;
        public const byte MOVE_TO_REAR_NH = 0x33;

        public const byte LS_LONG_CARD_IN = 0x46;
        public const byte LS_SHORT_CARD_IN = 0x47;
        public const byte LS_CARD_IN_FRONT_NH = 0x48; // without holding
        public const byte LS_CARD_IN_FRONT = 0x49; // whih holding
        public const byte LS_CARD_IN_RF_POS = 0x4A;
        public const byte LS_CARD_IN_IC_POS = 0x4B;
        public const byte LS_CARD_IN_REAR = 0x4C; // with holding
        public const byte LS_NO_CARD_IN = 0x4E;

       

        [StructLayout(LayoutKind.Sequential)]
        public struct CRSTATUS
        {
            [MarshalAs(UnmanagedType.U1)] public byte bLaneStatus;

            [MarshalAs(UnmanagedType.U1)] public byte bFCI;

            [MarshalAs(UnmanagedType.U1)] public byte bRCI;
        }

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_GetCRCondition", CharSet = CharSet.Ansi)]
        public static extern int A6_GetCRCondition(
            int hReader,
            IntPtr pStatus
            );

        [StructLayout(LayoutKind.Sequential)]
        public struct TRACKINFO
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1536)] public char[] Contents;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] Lengths;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Status;
        }

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_ReadTracks", CharSet = CharSet.Ansi)]
        public static extern int A6_ReadTracks(
            int hReader,
            byte bMode,
            int trackId,
            IntPtr trackInfo
            );

        [DllImport("A6CRTAPI.dll", EntryPoint = "A6_DetectIccType", CharSet = CharSet.Ansi)]
        public static extern int A6_DetectIccType(
            int hReader,
            byte[] pbType
            );
        #endregion DLL Import
    }
}