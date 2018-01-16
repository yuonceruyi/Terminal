using YuanTu.Consts;

namespace YuanTu.Default.House
{
    public class AInner
    {
        public static string ScreenSaver => "屏保";
        public static string Health_Context => "健康服务";
        public static string Query_Context => "健康查询";

        public static string Create_Context => "建档发卡";

        public class Health
        {
            public static string HeightWeight => Health_Context + A.Separator + nameof(HeightWeight);
            public static string Fat => Health_Context + A.Separator + nameof(Fat);
            public static string BloodPressure => Health_Context + A.Separator + nameof(BloodPressure);
            public static string SpO2 => Health_Context + A.Separator + nameof(SpO2);
            public static string Temperature => Health_Context + A.Separator + nameof(Temperature);
            public static string Ecg => Health_Context + A.Separator + nameof(Ecg);
            public static string Report => Health_Context + A.Separator + nameof(Report);
            public static string ReportPreview => Health_Context + A.Separator + nameof(ReportPreview);
            //public static string TestInterfaceView => Health_Context + A.Separator + nameof(TestInterfaceView);
        }

        public class Query
        {
            public static string DateTimeView => Health_Context + A.Separator + nameof(DateTimeView);
            public static string QueryView => Health_Context + A.Separator + nameof(QueryView);
            public static string ReportPreview => Health_Context + A.Separator + nameof(ReportPreview);
        }
        public class XC
        {
            public static string ChoiceHospital => A.YuYue_Context + A.Separator + nameof(ChoiceHospital);
        }
        public class YY
        {
            public static string ChoiceHospital => A.YuYue_Context + A.Separator + nameof(ChoiceHospital);
        }

        public class CX
        {
            public static string QueryReport => Query_Context + A.Separator + nameof(QueryReport);
        }

        public class JD
        {
            public static string SelectType => Create_Context + A.Separator + nameof(SelectType);
            public static string IdCard => Create_Context + A.Separator + nameof(IdCard);
            public static string PatInfo => Create_Context + A.Separator + nameof(PatInfo);
            public static string PatInfoEx => Create_Context + A.Separator + nameof(PatInfoEx);
            public static string Print => Create_Context + A.Separator + nameof(Print);
        }
    }
}