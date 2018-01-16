using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using CardType = YuanTu.Consts.Enums.UserCenter.CardType;

namespace YuanTu.Default.Tablet.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        [Dependency]
        public IAuthModel AuthModel { get; set; }

        public override void OnSetButtons()
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<CardTypeModel>();
            var k = Enum.GetValues(typeof(CardType));
            foreach (CardType cardType in k)
            {
                var spex = $"Card:{cardType}";
                var visible = config.GetValue($"{spex}:Visabled");
                if (visible != "1") continue;
                bts.Add(new CardTypeModel
                {
                    CardType = cardType,
                    Name = config.GetValue($"{spex}:Name") ?? "未定义",
                    Order = config.GetValueInt($"{spex}:Order"),
                    ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                    Can建档 = config.GetValueInt($"{spex}:建档") == 1,
                    Can查询 = config.GetValueInt($"{spex}:查询") == 1,
                    Can住院 = config.GetValueInt($"{spex}:住院") == 1
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
            var button = (CardTypeModel) i.Tag;
            AuthModel.CardType = button.CardType;
            var inHospital = NavigationEngine.Context == A.ZhuYuan_Context;
            switch (button.CardType)
            {
                case CardType.青岛区域诊疗卡:
                    Navigate(inHospital ? A.ZY.Card : A.CK.Card);
                    break;

                case CardType.青岛医保卡:
                    Navigate(inHospital ? A.ZY.HICard : A.CK.HICard);
                    break;

                case CardType.广州医保卡:
                    Navigate(inHospital ? A.ZY.HICard : A.CK.HICard);
                    break;

                case CardType.广州番禺民生卡:
                    Navigate(inHospital ? A.ZY.Card : A.CK.Card);
                    break;

                case CardType.河南安阳就诊卡:
                    Navigate(inHospital ? A.ZY.Card : A.CK.Card);
                    break;

                case CardType.广州番禺医保卡:
                    Navigate(inHospital ? A.ZY.HICard : A.CK.HICard);
                    break;
            }
        }

        /// <summary>
        ///     设置是否显示的逻辑
        /// </summary>
        /// <param name="cardTypeButton"></param>
        /// <returns></returns>
        protected virtual bool OnButtonVisabled(CardTypeModel cardTypeButton)
        {
            if (ChoiceModel.Business == Business.建档)
                return cardTypeButton.Can建档;
            if (NavigationEngine.Context == A.ZhuYuan_Context)
                return cardTypeButton.Can住院;
            return cardTypeButton.Can查询;
        }
    }

    public class CardTypeModel
    {
        public CardType CardType { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Uri ImageSource { get; set; }

        public bool Can建档 { get; set; }
        public bool Can查询 { get; set; }
        public bool Can住院 { get; set; }
    }
}