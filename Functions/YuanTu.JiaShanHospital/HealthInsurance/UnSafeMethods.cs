using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace YuanTu.JiaShanHospital.HealthInsurance
{
    public class UnSafeMethods
    {
        #region[网新恩普社保接口]

        //private const string DllPathSiInterface = "External\\TongXiang\\BargaingApplyV3_01038.dll";
        private const string DllPathSiInterface = "BargaingApplyV3_01038.dll";

        #region DLL Import

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingInit", CharSet = CharSet.Ansi,
        CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingInit(string Data1, StringBuilder retMsg, string Data2);

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingClose", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingClose(string Data, StringBuilder retMsg, string Data2);

        [DllImport(DllPathSiInterface, EntryPoint = "f_UserBargaingApply", CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int f_UserBargaingApply(int Code, double No, string Data, StringBuilder retMsg, string Data2);


        #endregion DLL Import

        #endregion

        #region 读医保卡接口
        private const string DllPathSiReader= "ICCInter_JX_SB.dll";
        //private const string DllPathSiReader = "External\\TongXiang\\ICCINTER_JX_SB.DLL";
        [DllImport(DllPathSiReader, EntryPoint = "IC_ReadCardInfo_NoPin", CharSet = CharSet.Ansi)]
        public static extern int IC_ReadCardInfo_NoPin(StringBuilder sb);
        #endregion


        public static void DoNothing()
        {
           
        }
    }
}
