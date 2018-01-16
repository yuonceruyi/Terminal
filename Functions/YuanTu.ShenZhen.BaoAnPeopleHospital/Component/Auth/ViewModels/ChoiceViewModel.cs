using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        public override string Title => "验证方式选择";

        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel)i.Tag;
            CardModel.CardType = button.CardType;
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
                    break;

                case CardType.扫码:
                    Navigate(A.CK.QrCode);
                    break;

                case CardType.医保卡:
                    break;

                case CardType.门诊号:

                    Navigate(InnerA.CK.PatientNumber);
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