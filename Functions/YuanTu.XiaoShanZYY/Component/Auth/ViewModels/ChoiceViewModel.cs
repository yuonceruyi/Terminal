using System;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Auth.ViewModels;
using YuanTu.XiaoShanZYY.Component.Auth.Models;

namespace YuanTu.XiaoShanZYY.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        [Dependency]
        public IAuthModel Auth { get; set; }

        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel) i.Tag;
            CardModel.CardType = button.CardType;

            Auth.RealCardType = button.CardType; //不会被改变

            var inHospital = NavigationEngine.Context == A.ZhuYuan_Context;
            switch (button.CardType)
            {
                case CardType.NoCard:
                    break;

                case CardType.身份证:
                    Navigate(inHospital ? A.ZY.IDCard : A.CK.IDCard);
                    break;

                case CardType.就诊卡:
                    Navigate(inHospital ? A.ZY.Card : A.CK.Card);
                    break;

                case CardType.银行卡:
                    break;

                case CardType.社保卡:
                    Navigate(inHospital ? A.ZY.HICard : A.CK.HICard);
                    break;

                case CardType.居民健康卡:
                    Navigate(inHospital ? A.ZY.HICard : A.CK.HICard);
                    break;

                case CardType.扫码:
                    Navigate(A.CK.QrCode);
                    break;

                case CardType.医保卡:
                    break;

                case CardType.门诊号:
                    break;

                case CardType.住院号:
                    Navigate(A.ZY.InPatientNo);
                    break;

                case CardType.刷脸:
                    Navigate(A.CK.FaceRec);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}