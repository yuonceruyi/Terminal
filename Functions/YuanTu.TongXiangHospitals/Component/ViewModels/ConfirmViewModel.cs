using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;

namespace YuanTu.TongXiangHospitals.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

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

            var spex = $"PayIn:充值";
            QuickRechargeContent = config.GetValue($"{spex}:Name") ?? "未定义";
            ImgTxtBtnIconUri = resource.GetImageResourceUri(config.GetValue($"{spex}:ImageName"));
            CanQuickRecharge = config.GetValue($"{spex}:Visabled") == "1" && !NoPay && ChoiceModel.Business != Business.建档;

            var listOut = PayMethodDto.GetInfoPays(config, resource, "PayOut", new DelegateCommand<Info>(Confirm),
                p =>
                {
                    if (p.PayMethod == PayMethod.预缴金 &&
                        (ChoiceModel.Business == Business.建档 || CardModel.CardType != CardType.社保卡))
                        p.Visabled = false;
                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();
        }

        protected override void Confirm(Info i)
        {
            var payMethod = (PayMethod)i.Tag;
            PaymentModel.PayMethod = payMethod;

            ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
            ExtraPaymentModel.TotalMoney = PaymentModel.Self;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            //准备门诊支付所需病人信息
            if (ChoiceModel.Business == Business.建档)
            {
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = IdCardModel.Name,
                    PatientId = IdCardModel.IdCardNo,
                    IdNo = IdCardModel.IdCardNo,
                    GuardianNo = IdCardModel.IdCardNo,
                    CardNo = CardModel.CardType == CardType.社保卡 ? SiModel.OutCardNo : CardModel.CardNo,
                    CardType = CardModel.CardType,
                    Remain = 0
                };
            }
            else
            {
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = PatientModel.当前病人信息.name,
                    PatientId = PatientModel.当前病人信息.patientId,
                    IdNo = PatientModel.当前病人信息.idNo,
                    GuardianNo = PatientModel.当前病人信息.guardianNo,
                    CardNo = CardModel.CardType == CardType.社保卡 ? SiModel.OutCardNo : CardModel.CardNo,
                    CardType = CardModel.CardType,
                    Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
                };

                if (CardModel.CardType == CardType.社保卡)
                    SiModel.诊间结算 = payMethod == PayMethod.预缴金;
            }

            switch (payMethod)
            {
                case PayMethod.预缴金:
                    if (CardModel.CardType == CardType.社保卡)
                        PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                        {
                            var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                            if (rest?.IsSuccess ?? false)
                                ChangeNavigationContent($"自费金额:{ExtraPaymentModel.TotalMoney.In元()}");
                        }, null);
                    else
                        ShowAlert(false, "温馨提示", "自费病人请选择其他支付方式");
                    break;

                case PayMethod.银联:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;

                case PayMethod.支付宝:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;

                case PayMethod.微信支付:
                    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;

                case PayMethod.苹果支付:
                    ShowAlert(false, "支付确认", "业务未实现");
                    break;
            }
        }

        protected override void NoPayConfirm()
        {
            //if (ChoiceModel.Business == Business.建档)
            //{
            //    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
            //    ExtraPaymentModel.TotalMoney = PaymentModel.Total;
            //    ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
            //    ExtraPaymentModel.Complete = false;
            //    //准备门诊支付所需病人信息

            //    ExtraPaymentModel.PatientInfo = new PatientInfo
            //    {
            //        Name = IdCardModel.Name,
            //        PatientId = IdCardModel.IdCardNo,
            //        IdNo = IdCardModel.IdCardNo,
            //        GuardianNo = IdCardModel.IdCardNo,
            //        CardNo = CardModel.CardType == CardType.社保卡 ? SiModel.OutCardNo : CardModel.CardNo,
            //        Remain = 0
            //    };
            //    ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
            //    GoToUnion();
            //}
            //else
            if (ChoiceModel.Business == Business.预约)
            {
                base.NoPayConfirm();
            }
            else
            {
                if (PaymentModel.Self == 0)
                {
                    PaymentModel.PayMethod = PayMethod.现金;
                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                    ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
                    ExtraPaymentModel.Complete = false;
                    //准备门诊支付所需病人信息

                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = PatientModel.当前病人信息.name,
                        PatientId = PatientModel.当前病人信息.patientId,
                        IdNo = PatientModel.当前病人信息.idNo,
                        GuardianNo = PatientModel.当前病人信息.guardianNo,
                        CardNo = CardModel.CardType == CardType.社保卡 ? SiModel.OutCardNo : CardModel.CardNo,
                        CardType = CardModel.CardType,
                        Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
                    };

                    PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                    {
                        var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                        if (rest?.IsSuccess ?? false)
                            ChangeNavigationContent($"自费金额:{ExtraPaymentModel.TotalMoney.In元()}");
                    }, null);
                }
                else
                {
                    ShowAlert(false, "温馨提示", "业务逻辑错误");
                }
            }
        }

        protected override void SecondConfirm(Action<bool> act)
        {
            var block = new TextBlock { TextAlignment = TextAlignment.Center, FontSize = 17 };
            block.Inlines.Add("您即将为\r\n姓名:");
            block.Inlines.Add(new TextBlock
            {
                Text = PatientModel.当前病人信息.name,
                Foreground = new SolidColorBrush(Colors.Red),
                FontWeight = FontWeights.Bold,
                FontSize = 20
            });
            block.Inlines.Add(" 卡号:");
            block.Inlines.Add(new TextBlock
            {
                Text = CardModel.CardType == CardType.社保卡 ? SiModel.OutCardNo : CardModel.CardNo,
                Foreground = new SolidColorBrush(Colors.Coral),
                FontWeight = FontWeights.Bold,
                FontSize = 20
            });
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");
            ShowConfirm("信息确认", block, act);
        }
    }
}