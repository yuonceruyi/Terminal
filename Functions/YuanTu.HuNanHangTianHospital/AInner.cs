using System;
using YuanTu.Consts;

namespace YuanTu.HuNanHangTianHospital
{
    public class AInner
    {
        public class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
            //public static string Print => A.ChaKa_Context + A.Separator + nameof(Print);
        }

        public class XC
        {
            public static string PayChoice =>A.XianChang_Context + A.Separator + nameof(PayChoice);
        }
        public class QH
        {
            public static string PayChoice => A.QuHao_Context + A.Separator + nameof(PayChoice);
        }
        public class JF
        {
            public static string PayChoice =>A.JiaoFei_Context  + A.Separator + nameof(PayChoice);
        }
    }
}
