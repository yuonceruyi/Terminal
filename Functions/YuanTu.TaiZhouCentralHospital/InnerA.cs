using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using YuanTu.Consts;

namespace YuanTu.TaiZhouCentralHospital
{
    public class InnerA
    {
        public class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
        }
        public class XC
        {
            public static string Time => A.XianChang_Context + A.Separator + nameof(Time);
        }
    }
}
