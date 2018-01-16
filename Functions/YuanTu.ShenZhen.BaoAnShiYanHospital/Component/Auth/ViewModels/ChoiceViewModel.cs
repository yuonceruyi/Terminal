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

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
    {
        public override string Title => "验证方式选择";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
        }

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
            if (Data.Count > 1)
                PlaySound(SoundMapping.选择验证方式);
            else
                Confirm(Data[0]);
        }
    }
}