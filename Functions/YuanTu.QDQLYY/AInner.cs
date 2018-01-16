using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using YuanTu.Consts;

namespace YuanTu.QDQLYY
{
    public class AInner
    {

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
        public static string DeptInfo_Context => "科室信息";
        public class KSXX
        {
            public static string DeptList => DeptInfo_Context + A.Separator + nameof(DeptList);
            public static string DeptInfo => DeptInfo_Context + A.Separator + nameof(DeptInfo);
            public static string DocList => DeptInfo_Context + A.Separator + nameof(DocList);
            public static string DocInfo => DeptInfo_Context + A.Separator + nameof(DocInfo);
        }
        public static string CYQD_Context => "出院清单";
        public class CYQD
        {
            public static string FeeList => CYQD_Context + A.Separator + nameof(FeeList);
        }
    }
}
