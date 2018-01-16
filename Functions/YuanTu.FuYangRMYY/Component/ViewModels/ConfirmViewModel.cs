using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.FuYangRMYY.Component.Auth.Models;

namespace YuanTu.FuYangRMYY.Component.ViewModels
{
    public class ConfirmViewModel:YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            var choice = GetInstance<IChoiceModel>();
            ChangeNavigationContent("");

            LeftList = PaymentModel.LeftList;
            RightList = PaymentModel.RightList;
            MidList = PaymentModel.MidList;

            NoPay = PaymentModel.NoPay;
            ViewTitle = NoPay ? $"请点击确定完成{ChoiceModel.Business}" : "请点击下方卡片选择支付方式";
            Hint = $"{ChoiceModel.Business}信息";

            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            var prefix = "PayIn:充值";
            QuickRechargeContent = config.GetValue($"{prefix}:Name") ?? "未定义";
            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName"));
            CanQuickRecharge = config.GetValue($"{prefix}:Visabled") == "1" && !NoPay;
            var quhao = ChoiceModel.Business == Business.取号;
            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm),
                dto =>
                {
                   
                    if (dto.PayMethod == PayMethod.社保&&(quhao||CardModel.CardType!=CardType.社保卡))
                    {
                        dto.Visabled = false;
                    }
                    if (dto.PayMethod == PayMethod.支付宝)
                    {
                        dto.Hint = "随机奖励金";
                        dto.Template = "InfoItemPayBubble";
                    }

                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();

        }

        protected override void HandlePaymethod(Info i, PayMethod payMethod)
        {
            if (payMethod==PayMethod.社保)
            {
                PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                {
                    var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                    if (rest?.IsSuccess ?? false)
                        ChangeNavigationContent(i.Title);
                }, null);
                return;
            }
            base.HandlePaymethod(i, payMethod);
        }
    }
}
