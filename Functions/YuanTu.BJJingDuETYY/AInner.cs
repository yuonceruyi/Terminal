﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using YuanTu.Consts;

namespace YuanTu.BJJingDuETYY
{
    public class AInner
    {

        public class CK
        {
            public static string BarScan => A.ChaKa_Context + A.Separator + nameof(BarScan);
        }

        public static string QualificationQuery_Context => "医生资格查询";
        public class ZYZG
        {
            public static string Query => QualificationQuery_Context + A.Separator + nameof(Query);
            public static string Qualification => QualificationQuery_Context + A.Separator + nameof(Qualification);
        }

        public static string QueneSelect_Context => "排队叫号签到";
        public class PDJH
        {
            public static string Select => QualificationQuery_Context + A.Separator + nameof(Select);
        }
        public static class JYJL
        {
            public static string Print => A.DiagReportQuery + A.Separator + nameof(Print);
        }
    }
}
