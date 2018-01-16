using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Log;

namespace YuanTu.HuNanHangTianHospital.Common
{
    public class KY
    {
        public const string DllPathHY = "syjdll.dll";

        [DllImport(DllPathHY, EntryPoint = "SetUserDirectory", CharSet = CharSet.Ansi)]
        public static extern int SetUserDirectory(string sWorkDirectory);

        [DllImport(DllPathHY, EntryPoint = "trans_proc", CharSet = CharSet.Ansi)]
        public static extern int Trans_proc(byte[] loca_req, byte[] loca_res);

        [DllImport(DllPathHY, EntryPoint = "GetErrorCode", CharSet = CharSet.Ansi)]
        public static extern int GetErrorCode(string sWorkDirectory);

        private static bool SetUserDirectory()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            int code = SetUserDirectory(path);
            return code == 0;
        }

        private static bool InitSystem()
        {
            var bytes = new byte[1024];
            return Run("20", ref bytes) == 0;
        }

        private static bool CloseHardware()
        {
            var bytes = new byte[1024];
            var resCode = Run("98", ref bytes);
            Logger.Main.Info("卡友退卡结束 返回码"+ resCode);
            return resCode == 0;
        }

        public static bool InitHardware()
        {
            var bytes = new byte[1024];
            var resCode = Run("99", ref bytes);
            return resCode == 0;
        }

        public static bool Revoke(string tranceNo, string money, ref byte[] resBytes)
        {
            var temp = "0";
            var amount = money.Length == 12 ? money : money.PadLeft(12, '0');
            StringBuilder type = new StringBuilder();
            type.Append("02");
            type.Append(amount);
            type.Append(temp.PadLeft(144, ' '));
            type.Append($"000000{tranceNo}");
            var resCode = Run(type.ToString(), ref resBytes);
            return resCode == 0;
        }

        public static void MoveOutCard()
        {
            CloseHardware();
        }

        private static bool Sign()
        {
            var bytes = new byte[1024];
            var resCode = Run("12", ref bytes);
            return resCode == 0;
        }

        public static int Trade(string money, ref byte[] resBytes)
        {
            if (!SetUserDirectory()  || !InitHardware())
            {
                return -1;
            }
            var amount = money.Length == 12 ? money : money.PadLeft(12, '0');
            var tradePassword = string.Format("01{0}0", amount);
            var tradeCode = Run(tradePassword, ref resBytes);//磁卡消费
            return tradeCode;
        }

        private static int Run(string tradType, ref byte[] resBytes)
        {
            var reqBytes = new byte[1024];
            var sBytes = Encoding.ASCII.GetBytes(tradType);
            Array.Copy(sBytes, reqBytes, sBytes.Length);
            var ret = Trans_proc(reqBytes, resBytes);
            return ret;
        }

    }

    public enum TradeResCodeType
    {
        交易成功 = 0,
        取消交易 = -12,
        余额不足 = 51,
        密码错误 = 55,
        初始化失败 = -1,
        未插卡或者卡已损坏 = -18
    }
}
