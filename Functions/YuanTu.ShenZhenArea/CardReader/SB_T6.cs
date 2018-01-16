using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Log;

namespace YuanTu.ShenZhenArea.CardReader
{
    public class SB_T6
    {
        #region 声明DLL的位置
        private const string DllPathCTDRD10U = "External\\DK_T6\\CTDRD10U.dll";
        private const string DllPathDcic32 = "External\\DK_T6\\dcic32.dll";
        private const string DllPathGoldDes = "External\\DK_T6\\GoldDes.dll";
        private const string DllPathHdssse32 = "External\\DK_T6\\hdssse32.dll";
        private const string DllPathSSSE32 = "External\\DK_T6\\SSSE32.dll";
        private const string DllPathSZSBCardProj1 = "External\\DK_T6\\SZSBCardProj1.ocx";
        private const string DllPathSzssc = "External\\DK_T6\\szssc.dll";
        private const string DllPathTY_SSSE32 = "External\\DK_T6\\TY_SSSE32.dll";
        #endregion

        //申明API位置.
        [DllImport(DllPathDcic32)]
        //初始化端口
        public static extern short IC_InitComm(int Port);

        [DllImport(DllPathDcic32)]
        //关闭端口
        public static extern short IC_ExitComm(int icdev);

        [DllImport(DllPathDcic32)]
        //卡下电(要重新审核密码才能继续操作)
        public static extern short IC_Down(int icdev);

        [DllImport(DllPathDcic32, EntryPoint = "IC_InitType", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //初始化卡型
        public static extern short IC_InitType(int icdev, int cardType);

        [DllImport(DllPathDcic32)]
        //判断连接是否成功,<0 ,连接不成功.0可以读写,1连接成功,但是没插卡.
        public static extern short IC_Status(int icdev);

        [DllImport(DllPathDcic32)]
        //检查原始密码(小于0为不正确)
        public static extern short IC_CheckPass_4442hex(int icdev, string Password);

        [DllImport(DllPathDcic32)]
        //更改密码(0为更改成功)
        public static extern short IC_ChangePass_4442hex(int icdev, string Password);

        [DllImport(DllPathDcic32)]
        //在固定的位置写入固定长度的数据
        public static extern short IC_Write(int icdev, int offset, int Length, string Databuffer);

        [DllImport(DllPathDcic32, EntryPoint = "IC_Read", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //在固定的位置读出固定长度的数据
        // public static extern short IC_Read(int icdev, int offset, int l, byte[] Databuffer);
        public static extern short IC_Read(int icdev, int offset, int l, StringBuilder data1);

        [DllImport(DllPathDcic32)]
        //发出声音
        public static extern short IC_DevBeep(int icdev, int intime);

        [DllImport(DllPathDcic32, EntryPoint = "IC_CpuApdu", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //CPU卡信息交换协议
        public static extern short IC_CpuApdu(int icdev, byte slen, byte[] sbuff, ref byte rlen, byte[] rbuff);

        [DllImport(DllPathDcic32, EntryPoint = "IC_CpuReset", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //CPU卡复位上电
        public static extern short IC_CpuReset(int icdev, byte[] rlen, byte[] rbuff);

        [DllImport(DllPathDcic32, EntryPoint = "IC_SetCpuPara", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //CPU卡参数设置
        public static extern short IC_SetCpuPara(int icdev, byte cputype, byte cpupro, byte cpuetu);

        [DllImport(DllPathDcic32, EntryPoint = "IC_CpuApduSourceEXT", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short IC_CpuApduSourceEXT(int icdev, short slen, byte[] sbuff, ref short rlen, byte[] rbuff);

        //  [DllImport(DllPathDcic32, EntryPoint = "IC_CpuApdu_Hex", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //CPU卡信息交换协议
        [DllImport(DllPathDcic32)]
        public static extern short IC_CpuApdu_Hex(int icdev, short slen, string sbuff, ref short rlen, StringBuilder rbuff);

        [DllImport(DllPathDcic32)]
        public static extern short IC_CpuReset_Hex(int icdev, ref byte rlen, ref byte rbuff);

        private static int icdev = -1;
        private static short st = 0;
        public static int Port = 100;//USB
        public static bool Connected { get; private set; }
        public static event Action<string> Log = Logger.Device.Info;

        public class 读社保卡指令
        {
            public const string 选取主文件 = "00A40000023F00";
            public const string 选取社保应用文件 = "00A404000F7378312E73682EC9E7BBE1B1A3D5CF";
            public const string 选取EF05文件 = "00A4020002EF05";//读卡号
            public const string 选取EF06文件 = "00A4020002EF06";//读其他个人信息
            public const string 读卡片识别码 = "00B2010012";
            public const string 读卡类别 = "00B2020003";
            public const string 读规范版本 = "00B2030006";
            public const string 读初始化机构编号 = "00B204000E";
            public const string 读发卡日期 = "00B2050006";
            public const string 读卡有效期 = "00B2060006";
            public const string 读卡号 = "00B207000B";
            public const string 读社会保障号码 = "00B2080014";
            public const string 读姓名 = "00B2090020";
            public const string 读性别 = "00B20A0003";
            public const string 读民族 = "00B20B0003";
            public const string 读出生地 = "00B20C0005";
            public const string 读出生日期 = "00B20D0006";
        }

        public static bool IC_InitComm()
        {
            if (Connected)
                return true;
            icdev = IC_InitComm(Port);
            if (icdev < 0)
            {
                var log = $"打开端口失败，返回值{icdev}";
                Log?.Invoke(log);
                return false;
            }
            Connected = true;
            st = IC_DevBeep(icdev, 10);
            return true;
        }

        public static bool IC_CpuReset()
        {
            byte rlen = 1;
            byte[] recbuff = new byte[32];
            st = IC_CpuReset_Hex(icdev, ref rlen, ref recbuff[0]);
            if (st != 0)
            {
                var log = $"复位失败，返回值{st}";
                Log?.Invoke(log);
                return false;
            }
            return true;
        }

        public static bool IC_SetCpuPara()
        {
            st = IC_SetCpuPara(icdev, 12, 0, 30);
            if (st != 0)
            {
                var log = $"CPU参数设置失败，返回值{st}";
                Log?.Invoke(log);
                return false;
            }
            return true;
        }

        public static bool IC_InitType()
        {
            st = IC_InitType(icdev, 12);
            if (st != 0)
            {
                var log = $"初始化卡类型失败，返回值{st}";
                Log?.Invoke(log);
                //return false;
            }
            return true;
        }

        public static bool IC_CpuApdu_HexOrder(string order, out string data)
        {
            var sbuff = order;
            var slen = (short)(sbuff.Length / 2);
            var rbuff = new StringBuilder();
            short rlen = 0;
            st = IC_CpuApdu_Hex(icdev, slen, sbuff, ref rlen, rbuff);
            if (st != 0)
            {
                var log = $"指令{order}执行失败，返回值{st}";
                Log?.Invoke(log);
                data = null;
                return false;
            }
            data = HexStr2Str(rbuff.ToString().Substring(4, rbuff.Length - 8)).TrimEnd('\0').Trim();
            return true;
        }

        public static string HexStr2Str(string StrIn)
        {
            var lst = new List<byte>();
            for (int i = 0; i < StrIn.Length; i += 2)
            {
                lst.Add((byte)Int32.Parse(StrIn.Substring(i, 2), NumberStyles.HexNumber));
            }
            var res = Encoding.GetEncoding("GB18030").GetString(lst.ToArray());
            return res;
        }

        #region 读医疗证号

        [DllImport(DllPathSzssc)]
        static extern short rdInfo_SSCID(IntPtr Info);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct TSSCInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 70)]
            public byte[] SSCID;
        }

        [DllImport(DllPathSzssc)]
        static extern short IniPort(int Port);

        private static int rtnVal;
        public static bool Initialize { get; private set; }

        public static bool IniPort()
        {
            rtnVal = IniPort(Port);
            if (rtnVal != 0)
            {
                var log = $"读医疗证号初始化端口失败：返回值:{rtnVal}";
                Log?.Invoke(log);
                return false;
            }
            Initialize = true;
            return true;
        }
        /// <summary>
        /// 读医疗证号【加密串】
        /// </summary>
        /// <param name="ylzh"></param>
        /// <returns></returns>
        public static bool rdInfo_SSCID(out string ylzh)
        {
            if (!IniPort())
            {
                ylzh = null;
                return false;
            }

            var info = new TSSCInfo();
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TSSCInfo)));
            Marshal.StructureToPtr(info, ptr, false);
            rtnVal = rdInfo_SSCID(ptr);
            if (rtnVal != 0)
            {
                Logger.Device.Debug($"读医疗证号失败：返回值:{rtnVal}\n");
                ylzh = null;
                return false;
            }
            info = (TSSCInfo)Marshal.PtrToStructure(ptr, typeof(TSSCInfo));
            ylzh = Encoding.Default.GetString(info.SSCID).TrimEnd('\u0000');
            Marshal.FreeHGlobal(ptr);
            return true;
        }

        #endregion 读医疗证号

    }
}
