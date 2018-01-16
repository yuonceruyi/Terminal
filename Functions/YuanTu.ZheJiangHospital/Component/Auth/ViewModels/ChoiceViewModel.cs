using System;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.ZheJiangHospital.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel) i.Tag;
            CardModel.CardType = button.CardType;
            switch (button.CardType)
            {
                case CardType.身份证:
                    Navigate(A.CK.IDCard);
                    break;

                case CardType.社保卡:
                    Navigate(A.CK.HICard);
                    break;

                case CardType.市医保卡:
                    Navigate(A.CK.HICard);
                    break;

                case CardType.省医保卡:
                    Navigate(A.CK.HICard);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}