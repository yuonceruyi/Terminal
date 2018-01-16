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

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class ChoiceViewModel : ViewModelBase
    {
        public override string Title => "卡种选择";

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            OnSetButtons();
        }

        public virtual void OnSetButtons()
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
                    Can住院 = config.GetValueInt($"{spex}:住院") == 1,
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

        protected virtual void Confirm(Info i)
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

        #region Binding

        private ObservableCollection<InfoCard> _data;

        public ObservableCollection<InfoCard> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
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