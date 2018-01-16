using System;
using YuanTu.Consts;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital
{
    public class InnerA
    {
        public static string InHospitalFeeList_Context => "住院费用查询";

        /// <summary>
        /// 住院费用查询
        /// </summary>
        public class ZYFYCX
        {
            public static string Date => InHospitalFeeList_Context + A.Separator + nameof(Date);
            public static string DailyDetail => InHospitalFeeList_Context + A.Separator + nameof(DailyDetail);
        }

        /// <summary>
        /// 住院一日清单
        /// </summary>
        public class ZYYRQD
        {
            public static string Print => A.InDayDetailList_Context + A.Separator + nameof(Print);
        }

        public class CK
        {
            /// <summary>
            /// 社保卡密码
            /// </summary>
            public static string HICardPassword => A.ChaKa_Context + A.Separator + nameof(HICardPassword);
            /// <summary>
            /// 登记号【扫描登记号】
            /// </summary>
            public static string PatientNumber => A.ChaKa_Context + A.Separator + nameof(PatientNumber);
        }
    }
}
