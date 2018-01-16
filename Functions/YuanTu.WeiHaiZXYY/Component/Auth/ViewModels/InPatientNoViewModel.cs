using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class InPatientNoViewModel:YuanTu.Default.Component.Auth.ViewModels.InPatientNoViewModel
    {

        [Dependency]
        public ICardModel CardModel { get; set; }
        public override void Comfirm()
        {
            if (InPatientNo == "")
            {
                ShowAlert(false, "友情提示", "住院号不能为空");
                return;
            }
            CardModel.CardNo = InPatientNo;
            Navigate(A.JYJL.DiagReport);
        }
    }
}
