using YuanTu.Consts;

namespace YuanTu.YuHangZYY
{
    public class InnerA
    {
        public class JD
        {
            public static string Confirm => A.JianDang_Context + A.Separator + nameof(Confirm);
            //public static string Print => A.JianDang_Context + A.Separator + nameof(Print);
        }
        public class QH
        {
            public static string Date => A.QuHao_Context + A.Separator + nameof(Date);
        }
    }
}