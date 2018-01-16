using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using YuanTu.TaiZhouCentralHospital.Component.Register.Models;

namespace YuanTu.TaiZhouCentralHospital.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
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
                p => {
                    if (p.PayMethod == PayMethod.预缴金 && ChoiceModel.Business == Business.建档)
                        p.Visabled = false;
                    if (p.PayMethod == PayMethod.现金 && ChoiceModel.Business != Business.建档)
                        p.Visabled = false;
                });
            PayOut = new ObservableCollection<InfoIcon>(listOut);

            PlaySound();
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            var destAddress = navigationContext.Parameters.ToList()[3].Value.ToString();
            if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡 &&
                 destAddress != A.XC.Print)
            {
                //TODO 取消预约
                GetInstance<ISiRegHelpModel>().CancelAppoint?.Invoke();
            }
            return base.OnLeaving(navigationContext);
        }

        protected override void Confirm(Info i)
        {
            SecondConfirm(p =>
            {
                if (p)
                {
                    PaymentModel.PayMethod = (PayMethod)i.Tag;

                    ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                    ExtraPaymentModel.TotalMoney = PaymentModel.Self;
                    ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
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
                            CardNo = CardModel.CardNo,
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
                            CardNo = CardModel.CardNo,
                            CardType = CardModel.CardType,
                            Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
                        };
                    }


                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.预缴金:
                            var patientModel = PatientModel.当前病人信息;

                            if (decimal.Parse(patientModel.accBalance) < PaymentModel.Self)
                            {
                                ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
                                return;
                            }

                            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                            {
                                var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                                if (rest?.IsSuccess ?? false)
                                    ChangeNavigationContent(i.Title);
                            }, null);
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

                        case PayMethod.现金:
                            ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                            Navigate(A.Third.Cash);
                            break;

                        case PayMethod.苹果支付:
                            ShowAlert(false, "支付确认", "业务未实现");
                            break;
                        case PayMethod.智慧医疗:
                            ShowAlert(false, "支付确认", "业务未实现");
                            break;
                    }
                }
            });
        }
        protected override void SecondConfirm(Action<bool> act)
        {
            var name = ChoiceModel.Business == Business.建档 ? IdCardModel.Name : PatientModel.当前病人信息.name;
            var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };
            block.Inlines.Add("您即将为\r\n姓名:");
            block.Inlines.Add(new TextBlock() { Text = name, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add(" 卡号:");
            block.Inlines.Add(new TextBlock() { Text = CardModel.CardNo, Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");
            ShowConfirm("信息确认", block, act);
        }
    }
}
