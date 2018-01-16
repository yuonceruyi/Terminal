using YuanTu.Consts;
using YuanTu.Default.Tablet;

namespace YuanTu.VirtualHospital
{
    public static class InnerA
    {
        public static string JDChoneZhi_Context = "建档充值";
        public static string WaiYuanCard_Contenxt => "外院卡";
        public static string ChuYuan_Context => "自助出院";
        public static string Loan_Context => "先诊疗后付费";

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

        public static class ChuYuan
        {
            public static string Confirm => ChuYuan_Context + A.Separator + nameof(Confirm);
            public static string Print => ChuYuan_Context + A.Separator + nameof(Print);

        }

        public static class Loan
        {
            public static string Choice => Loan_Context + A.Separator + nameof(Choice);
            public static string Info => Loan_Context + A.Separator + nameof(Info);
            public static string Date => Loan_Context + A.Separator + nameof(Date);
            public static string Records => Loan_Context + A.Separator + nameof(Records);
            public static string RepayMethod => Loan_Context + A.Separator + nameof(RepayMethod);
            public static string Print => Loan_Context + A.Separator + nameof(Print);
        }

        public static class SY
        {
            public static string FaceRec => AInner.Cashier + A.Separator + nameof(FaceRec);
        }
    }
}
