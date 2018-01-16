using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Default.Component.Auth.ViewModels;

namespace YuanTu.TongXiangHospitals.Component.Auth.ViewModels
{
    public class ChoiceViewModel : Default.Component.Auth.ViewModels.ChoiceViewModel
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
                if (CreateModel.CreateType == CreateType.儿童 && cardType == CardType.社保卡)
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
    }
}