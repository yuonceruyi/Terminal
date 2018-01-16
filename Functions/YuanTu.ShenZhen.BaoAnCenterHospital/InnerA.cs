using System;
using YuanTu.Consts;

namespace YuanTu.ShenZhen.BaoAnCenterHospital
{
    public class InnerA
    {

        /// <summary>
        /// 插卡
        /// </summary>
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

        /// <summary>
        /// 住院
        /// </summary>
        public class ZY
        {
            /// <summary>
            /// 社保卡密码
            /// </summary>
            public static string HICardPassword => A.ChaKa_Context + A.Separator + nameof(HICardPassword);
        }
        /// <summary>
        /// 现场挂号
        /// </summary>
        public class XC
        {
            /// <summary>
            /// 挂号的科室类别
            /// </summary>
            public static string DeptType => A.ChaKa_Context + A.Separator + nameof(DeptType);
        }

    }
}
