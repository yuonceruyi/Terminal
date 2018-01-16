using YuanTu.Consts;

namespace YuanTu.YiWuBeiYuan
{
   public static class AInner
    {
        public static class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
        }
        public static class JYJL
        {
            public static string Print => A.DiagReportQuery + A.Separator + nameof(Print);
        }
    }
}
