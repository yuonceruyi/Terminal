using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Devices
{
    public static class UnsafeMethods
    {
        #region [Act_A630读卡器]
        private const string DllPathActA630 = "External\\Act_A630\\A6API.dll";

        public enum ResetAction : byte
        {
            /// <summary>
            /// 复位，不移动卡片。
            /// </summary>
            RESET_NOACTION = 0x30,
            /// <summary>
            /// 复位后，弹卡到前端不持卡位（如果机内有卡）。
            /// </summary>
            RESET_EJECT = 0x31,
            /// <summary>
            /// 复位后，从后端弹出卡（如果机内有卡）。
            /// </summary>
            RESET_RETRACT = 0x32,
            /// <summary>
            /// 复位后，移动卡到前端持卡位（如果机内有卡）。
            /// </summary>
            RESET_RETAIN = 0x33,
        }

        public enum CardStatus : byte
        {
            /// <summary>
            /// 卡在前端不持卡位
            /// </summary>
            CARD_ATGATEPOS = 0x30,
            /// <summary>
            /// 卡在前端持卡位
            /// </summary>
            CARD_ATFRONTEND = 0x31,
            /// <summary>
            /// 卡在射频位
            /// </summary>
            CARD_ATRFPOS = 0x32,
            /// <summary>
            /// 卡在IC位
            /// </summary>
            CARD_ATICPOS = 0x33,
            /// <summary>
            /// 卡在后端持卡位
            /// </summary>
            CARD_ATREAREND = 0x34,
            /// <summary>
            /// 机内无卡
            /// </summary>
            CARD_NOTPRESENT = 0x35,
            /// <summary>
            /// 卡不在标准位置
            /// </summary>
            CARD_NOTINSTDPOS = 0x36,
        }

        public enum MoveCardPos : byte
        {
            /// <summary>
            /// 移动卡到读卡器内 射频卡位
            /// </summary>
            MOVE_TORFPOS = 0x30,
            /// <summary>
            /// 移动卡到IC位
            /// </summary>
            MOVE_TOICPOS = 0x31,
            /// <summary>
            /// 移动卡到前端持卡位
            /// </summary>
            MOVE_TOFRONT = 0x32,
            /// <summary>
            /// 移动卡到后端持卡位
            /// </summary>
            MOVE_TOREAR = 0x33,
            /// <summary>
            /// 移动卡到前端不持卡位
            /// </summary>
            MOVE_TOGATEPOS = 0x34,
            /// <summary>
            /// 从后端弹出卡片
            /// </summary>
            MOVE_RETRACT = 0x35,
        }

        public enum LedAction : byte
        {
            /// <summary>
            /// 关闭
            /// </summary>
            LED_OFF		=0x30,
            /// <summary>
            /// 打开
            /// </summary>
            LED_LIGHTEN	=0x31,
            /// <summary>
            /// 闪烁
            /// </summary>
            LED_BLINK	=0x32
        }

        public enum Voltage : byte
        {
            VOLTAGE_1_8 = 0x30,
            VOLTAGE_3 = 0x31,
            VOLTAGE_5 = 0x32,

        }

        public enum Icc_Protocol : byte
        {
            T0=0x30,
            T1=0x31,
        }

        [DllImport(DllPathActA630, EntryPoint = "A6_Connect", CharSet = CharSet.Ansi)]
        public static extern int A6_Connect(int dwPort, int dwSpeed, ref int phReader);
        [DllImport(DllPathActA630, EntryPoint = "A6_Disconnect", CharSet = CharSet.Ansi)]
        public static extern int A6_Disconnect(int phReader);
        [DllImport(DllPathActA630, EntryPoint = "A6_Cancel", CharSet = CharSet.Ansi)]
        public static extern int A6_Cancel(int phReader);
        [DllImport(DllPathActA630, EntryPoint = "A6_Reset", CharSet = CharSet.Ansi)]
        public static extern int A6_Reset(int phReader, ResetAction action,byte[]verBuff,ref int verLength);
        [DllImport(DllPathActA630, EntryPoint = "A6_GetStatus", CharSet = CharSet.Ansi)]
        public static extern int A6_GetStatus(int phReader,ref CardStatus status);
        [DllImport(DllPathActA630, EntryPoint = "A6_PermitInsertion", CharSet = CharSet.Ansi)]
        public static extern int A6_PermitInsertion(int phReader, byte magOnly);
        [DllImport(DllPathActA630, EntryPoint = "A6_DenieInsertion", CharSet = CharSet.Ansi)]
        public static extern int A6_DenieInsertion(int phReader);
        [DllImport(DllPathActA630, EntryPoint = "A6_MoveCard", CharSet = CharSet.Ansi)]
        public static extern int A6_MoveCard(int phReader,MoveCardPos modePos);
        [DllImport(DllPathActA630, EntryPoint = "A6_LedControl", CharSet = CharSet.Ansi)]
        public static extern int A6_LedControl(int phReader, LedAction action);

        #region -[接触式CPU]
        [DllImport(DllPathActA630, EntryPoint = "A6_CpuActivate", CharSet = CharSet.Ansi)]
        public static extern int A6_CpuActivate(int phReader, Voltage voltage,out Icc_Protocol protocol,byte[]atrBuff,ref int atrLength);
        [DllImport(DllPathActA630, EntryPoint = "A6_IccPowerOn", CharSet = CharSet.Ansi)]
        public static extern int A6_IccPowerOn(int phReader);
        #endregion


        #region -[非接TypeA/TypeB]
        [DllImport(DllPathActA630, EntryPoint = "A6_TypeABCpuActivate", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuActivate(int phReader,byte[]atrBuff,ref int atrLength);
        [DllImport(DllPathActA630, EntryPoint = "A6_TypeABCpuTransmit", CharSet = CharSet.Ansi)]
        public static extern int A6_TypeABCpuTransmit(int phReader, byte[] sendBuff, ushort sendLength, bool sendMBit,byte[]recvBuff,ref int recvLength,out bool recvMBit);

        #endregion

        #endregion
    }
}
