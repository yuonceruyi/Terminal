using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.WeiHaiZXYY.Component.Auth.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.Auth.ViewModels.ChoiceViewModel
    {

        [Dependency]
        public IQueryChoiceModel QueryChoiceModel { get; set; }
        public override void OnSetButtons()
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var resource =ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<CardTypeModel>();
            var k = Enum.GetValues(typeof(CardType));
            foreach (CardType cardType in k)
            {
                var spex = $"Card:{cardType}";
                var v = config.GetValue($"{spex}:Visabled");
                if (v == "1")
                {
                    if (QueryChoiceModel != null
                        && QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果
                        && cardType == CardType.身份证)
                    {
                        bts.Add(new CardTypeModel
                        {
                            CardType = cardType,
                            Name = "住院号",
                            Order = 100,
                            //ImageSource = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName")),
                        });
                    }
                    else
                    {
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
                }
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
        protected override bool OnButtonVisabled(CardTypeModel cardTypeButton)
        {
            if (ChoiceModel.Business == Business.建档)
            {
                return cardTypeButton.Can建档;
            }
            else if (QueryChoiceModel != null && QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果)
            {
                return true;
            }
            return cardTypeButton.Can查询;
        }

        protected override void Confirm(Info i)
        {
            var button = (CardTypeModel)i.Tag;
            CardModel.CardType = button.CardType;
            switch (button.CardType)
            {
                case CardType.NoCard:
                    break;

                case CardType.身份证:
                    if (QueryChoiceModel != null && QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.检验结果)
                    {
                        Navigate(A.ZY.InPatientNo);
                    }
                    else
                    {
                        Navigate(A.CK.IDCard);
                    }
                    break;

                case CardType.就诊卡:
                    Navigate(A.CK.Card);
                    break;

                case CardType.银行卡:
                    break;

                case CardType.社保卡:
                    Navigate(A.CK.HICard);
                    break;

                case CardType.居民健康卡:
                    break;

                case CardType.扫码:
                    break;

                case CardType.医保卡:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
