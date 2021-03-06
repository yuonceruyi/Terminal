﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.ViewModels;
using YuanTu.YuHangSecondHospital.Component.Auth.Models;

namespace YuanTu.YuHangSecondHospital.Component.Auth.ViewModels
{
    public class ChoiceViewModel:Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public override void OnSetButtons()
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<CardTypeModel>();
            var k = Enum.GetValues(typeof(CardType));
            foreach (CardType cardType in k)
            {
                if (CreateModel.CreateType == CreateType.儿童 && (cardType == CardType.社保卡 || cardType == CardType.居民健康卡))
                    continue;
                var spex = $"Card:{cardType}";
                var v = config.GetValue($"{spex}:Visabled");
                if (v == "1")
                    bts.Add(new CardTypeModel
                    {
                        CardType = cardType,
                        Name = config.GetValue($"{spex}:Name") ?? "未定义",
                        Order = config.GetValueInt($"{spex}:Order"),
                        ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                        Can建档 = config.GetValueInt($"{spex}:建档") == 1,
                        Can查询 = config.GetValueInt($"{spex}:查询") == 1
                    });
            }
            var list = bts.OrderBy(p => p.Order).Where(OnButtonVisabled).Select(p => new InfoCard
            {
                Title = p.Name,
                ConfirmCommand = confirmCommand,
                IconUri = p.ImageSource,
                Tag = p
            });

            Data = new ObservableCollection<InfoCard>(list);

            PlaySound(SoundMapping.选择验证方式);
        }

        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel)i.Tag;
            var cm = CardModel as CardModel;
            CardModel.CardType = button.CardType;
            cm.RealCardType = button.CardType;//不会被改变
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
