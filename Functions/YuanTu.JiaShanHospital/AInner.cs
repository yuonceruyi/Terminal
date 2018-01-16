using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.JiaShanHospital
{
    public class AInner
    {
        public class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
        }

        public class XC
        {
            public static string PayChoice => A.XianChang_Context + A.Separator + nameof(PayChoice);
        }
        public class QH
        {
            public static string PayChoice => A.QuHao_Context + A.Separator + nameof(PayChoice);
        }
        public class JF
        {
            public static string PayChoice => A.JiaoFei_Context + A.Separator + nameof(PayChoice);
        }
    }
}
