﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using YuanTu.Consts;

namespace YuanTu.ShengZhouZhongYiHospital
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
        public static string DoctortQuery => "医生查询";
        public class YSCX
        {
            public static string Query => DoctortQuery + A.Separator + nameof(Query);
            public static string Doctor => DoctortQuery + A.Separator + nameof(Doctor);
            public static string DoctorInfo => DoctortQuery + A.Separator + nameof(DoctorInfo);
        }
    }
}
