
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.Auth.ViewModels.ChoiceViewModel
    {

        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel)i.Tag;
            //CardModel.CardType = button.CardType;
            if (button.CardType == CardType.条码卡)
            {
                Navigate(AInner.CK.BarScan);
            }
            else {
                base.Confirm(i);
            }
        }
    }
}