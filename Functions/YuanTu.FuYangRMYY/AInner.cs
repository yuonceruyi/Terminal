using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.FuYangRMYY
{
    public class AInner
    {
        public static string QueneSelect_Context => "排队叫号签到";
        public class PDJH
        {
            public static string Select => QueneSelect_Context + A.Separator + nameof(Select);
        }
    }
}
