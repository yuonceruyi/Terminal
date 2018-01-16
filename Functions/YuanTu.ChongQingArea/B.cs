using YuanTu.Consts;

namespace YuanTu.ChongQingArea
{
    internal class B
    {
        public class CK
        {
            public static string Password => A.ChaKa_Context + A.Separator + nameof(Password);
            public static string InputText => A.ChaKa_Context + A.Separator + nameof(InputText);
        }
        public class XC
        {
            public static string SelectSi => A.XianChang_Context + A.Separator + nameof(SelectSi);
            public static string InsertSiCard => A.XianChang_Context + A.Separator + nameof(InsertSiCard);
        }
        public class JF
        {
            public static string SelectSi => A.JiaoFei_Context + A.Separator + nameof(SelectSi);
            public static string InsertSiCard => A.JiaoFei_Context + A.Separator + nameof(InsertSiCard);
        }
        public static string ChuYuanJieSuan_Context => "出院结算";
        public class CY
        {
            public static string CYInfo => B.ChuYuanJieSuan_Context + A.Separator + nameof(CYInfo);
            public static string SelectSi => B.ChuYuanJieSuan_Context + A.Separator + nameof(SelectSi);
            public static string InsertSiCard => B.ChuYuanJieSuan_Context + A.Separator + nameof(InsertSiCard);
            public static string Confirm => B.ChuYuanJieSuan_Context + A.Separator + nameof(Confirm);
            public static string Print => B.ChuYuanJieSuan_Context + A.Separator + nameof(Print);
        }

        public static string BuKa_Context => "补卡";
        public class BK
        {
            public static string Confirm => B.BuKa_Context + A.Separator + nameof(Confirm);
            public static string Print => B.BuKa_Context + A.Separator + nameof(Print);
        }

        public static string MedicineItemsView => "药品信息查询";
        public class YP
        {
            public static string Query => MedicineItemsView + A.Separator + nameof(Query);
            public static string Medicine => MedicineItemsView + A.Separator + nameof(Medicine);
        }

        #region  取号

        public class QH
        {
            public static string SelectSi => A.QuHao_Context + A.Separator + nameof(SelectSi);
            public static string InsertSiCard => A.QuHao_Context + A.Separator + nameof(InsertSiCard);
        }

        #endregion

        #region  医改价格变动信息显示

        public static string NewMedicineItemsQuery => "药品变动价格查询";

        public class NMIQ
        {
            public static string Query => B.NewMedicineItemsQuery + A.Separator + nameof(Query);
            public static string NewMedicineItems => B.NewMedicineItemsQuery + A.Separator + nameof(NewMedicineItems);
        }

        public static string NewMedicineProjectQuery => "项目变动价格查询";

        public class NMPQ
        {
            public static string Query => B.NewMedicineProjectQuery + A.Separator + nameof(Query);
            public static string NewMedicineProject => B.NewMedicineProjectQuery + A.Separator + nameof(NewMedicineProject);
        }

        #endregion

        #region 指纹验证身份

        public static string BiometricCheck_Context => "生物信息验证";

        public class Bioc
        {
            public static string FingerPrintValidation => BiometricCheck_Context + A.Separator + nameof(FingerPrintValidation);
        }

        #endregion

    }
}