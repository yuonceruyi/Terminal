using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.YiWuFuBao
{
   public static class AInner
    {
        public static class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
        }
        public static class JYJL
        {
            public static string Print => A.DiagReportQuery + A.Separator + nameof(Print);
        }

        public  const string ChuYuanBillpay_Context = "ChuYuanBillpay_Context";
        public static class ChuYuan
        {
            //public static string InCard => ChuYuanBillpay_Context + A.Separator + nameof(InCard);
            public static string SiCard => ChuYuanBillpay_Context + A.Separator + nameof(SiCard);
            public static string Confirm => ChuYuanBillpay_Context + A.Separator + nameof(Confirm);
            public static string Print => ChuYuanBillpay_Context + A.Separator + nameof(Print);
        }
    }
}
