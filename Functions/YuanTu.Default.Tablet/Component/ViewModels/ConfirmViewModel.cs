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
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Services;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.Default.Tablet.Component.ViewModels
{
    public class ConfirmViewModel:Default.Component.ViewModels.ConfirmViewModel
    {
        [Dependency]
        public IAuthModel AuthModel { get; set; }
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
                    var patientInfo = AuthModel.当前就诊人信息;

                    ExtraPaymentModel.PatientInfo = new PatientInfo
                    {
                        Name = patientInfo.patientName,
                        PatientId = patientInfo.patientId.ToString(),
                        IdNo = patientInfo.idNo,
                        GuardianNo = patientInfo.idNo,
                        CardNo = AuthModel.CardNo,
                        Remain = patientInfo.balance
                    };

                    switch (PaymentModel.PayMethod)
                    {
                        case PayMethod.预缴金:
                            

                            if (patientInfo.balance < ExtraPaymentModel.TotalMoney)
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
            var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };
            block.Inlines.Add("您即将为\r\n姓名:");
            block.Inlines.Add(new TextBlock() { Text = AuthModel.当前就诊人信息.patientName, Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add(" 卡号:");
            block.Inlines.Add(new TextBlock() { Text = AuthModel.CardNo, Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            block.Inlines.Add($"\r\n执行{ChoiceModel.Business}，\r\n确认继续操作吗？");
            ShowConfirm("信息确认", block, act);
        }
    }
}
