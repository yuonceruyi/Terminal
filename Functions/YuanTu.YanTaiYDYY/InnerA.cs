using System;
using YuanTu.Consts;

namespace YuanTu.YanTaiYDYY
{
    public class InnerA
    {
        public static string JDChoneZhi_Context = "建档充值";
        public static string WaiYuanCard_Contenxt => "外院卡";
        public static string ScheduleQuery_Context => "门诊排班查询";
        public static string MaterialItemsQuery => "材料费查询";

        public static class JDCZ
        {
            public static string RechargeWay => JDChoneZhi_Context + A.Separator + nameof(RechargeWay);
            public static string Print => JDChoneZhi_Context + A.Separator + nameof(Print);
            public static string InputAmount => JDChoneZhi_Context + A.Separator + nameof(InputAmount);
        }
        public static class WYC
        {
            public static string WYCard => WaiYuanCard_Contenxt + A.Separator + nameof(WYCard);
            public static string WYPatientInfo => WaiYuanCard_Contenxt + A.Separator + nameof(WYPatientInfo);
        }

        public class MZPBCX
        {
            public static string ScheduleQuery => ScheduleQuery_Context + A.Separator + nameof(ScheduleQuery);
        }

        public class CLF
        {
            public static string Query => MaterialItemsQuery + A.Separator + nameof(Query);
            public static string ChargeItems => MaterialItemsQuery + A.Separator + nameof(ChargeItems);
        }
    }
}
