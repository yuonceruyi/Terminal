using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.ChongQingArea.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
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

            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm));
            if (ChoiceModel.Business == Business.补卡)
            {
                listOut.Remove(listOut.Find(t => t.Title == "诊疗卡支付"));
                CanQuickRecharge = false;
            }
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();
        }
        [Dependency]
        public IReSendModel ReSendModel { get; set; }
        //处理二次确认
        protected override void SecondConfirm(Action<bool> act)
        {
            Logger.Main.Info("进行二次确认SecondConfirm");
            var block = new TextBlock {TextAlignment = TextAlignment.Center, FontSize = 17};
            block.Inlines.Add("您即将为\r\n姓名:");
            if (ChoiceModel.Business == Business.补卡)
            {
                block.Inlines.Add(new TextBlock
                {
                    Text = ReSendModel.name,
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeights.Bold,
                    FontSize = 20
                });
            }
            else
            {
                block.Inlines.Add(new TextBlock
                {
                    Text = PatientModel.当前病人信息.name,
                    Foreground = new SolidColorBrush(Colors.Red),
                    FontWeight = FontWeights.Bold,
                    FontSize = 20
                });
            }
            //block.Inlines.Add(" 卡号:");
            //block.Inlines.Add(new TextBlock() { Text = CardModel.CardNo, Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });             
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");
            ShowConfirm("信息确认", block, act);
            Logger.Main.Info("二次确认完成");
        }
        protected override void Confirm(Info i)
        {
            if (ChoiceModel.Business == Business.补卡)
            {
                SecondConfirm(p =>
                {
                    if (!p)
                        return;
                    var payMethod = (PayMethod)i.Tag;
                    PaymentModel.PayMethod = payMethod;

                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                    ExtraPaymentModel.CurrentPayMethod = payMethod;
                    ExtraPaymentModel.Complete = false;
                    //准备门诊支付所需病人信息

                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = ReSendModel.name,
                        PatientId = ReSendModel.Res补卡查询.data[0].cardNo,
                        IdNo = ReSendModel.idNo,
                        GuardianNo = ReSendModel.guarderId,
                        CardNo = ReSendModel.Res补卡查询.data[0].patientCard,
                        CardType = CardType.就诊卡,
                    };

                    HandlePaymethod(i, payMethod);
                });
            }
            else
            {
                SecondConfirm(p =>
                {
                    if (!p)
                        return;
                    var payMethod = (PayMethod)i.Tag;
                    PaymentModel.PayMethod = payMethod;

                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                    ExtraPaymentModel.CurrentPayMethod = payMethod;
                    ExtraPaymentModel.Complete = false;
                    //准备门诊支付所需病人信息

                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = PatientModel.当前病人信息.name,
                        PatientId = PatientModel.当前病人信息.patientId,
                        IdNo = PatientModel.当前病人信息.idNo,
                        GuardianNo = PatientModel.当前病人信息.guardianNo,
                        CardNo = PatientModel.当前病人信息.cardNo,
                        Remain = decimal.Parse(PatientModel.当前病人信息.accBalance),
                        CardType = CardModel.CardType,
                    };

                    HandlePaymethod(i, payMethod);
                });
            }
        }
    }
}