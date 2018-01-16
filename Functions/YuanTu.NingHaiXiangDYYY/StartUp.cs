using YuanTu.Consts;
using YuanTu.Core.Navigating;

namespace YuanTu.NingHaiXiangDYYY
{
    public class Startup:YuanTu.Default.Clinic.Startup
    {
        public override bool RegisterTypes(ViewCollection children)
        {
            base.RegisterTypes(children);
            children.Add(A.CK.Card, "个人信息", typeof (YuanTu.NingHaiXiangDYYY.Component.Auth.Views.CardView), A.CK.Info);
            return true;
        }
    }
}
