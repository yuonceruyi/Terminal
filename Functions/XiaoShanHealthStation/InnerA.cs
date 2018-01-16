using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.XiaoShanHealthStation
{
    public class InnerA
    {
        public class XC
        {
            public static string AmPm => A.XianChang_Context + A.Separator + nameof(AmPm);
        }

        public class YY
        {
            public static string AmPm => A.YuYue_Context + A.Separator + nameof(AmPm);
        }
    }
}
