using YuanTu.Consts;

namespace YuanTu.WeiHaiZXYY
{
    public class AInner
    {
        public static string BindLisencePlate => "绑定车牌";
        public class BDCP
        {
            public static string Bind => BindLisencePlate + A.Separator + nameof(Bind);
        }
    }
}
