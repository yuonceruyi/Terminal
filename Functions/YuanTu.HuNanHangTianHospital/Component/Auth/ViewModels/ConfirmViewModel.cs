using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.HuNanHangTianHospital.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        [Dependency]
        public IIdCardModel IdCardModel { get; set; }

        protected override void NoPayConfirm()
        {
            if (ChoiceModel.Business == Business.建档)
            {
                ExtraPaymentModel.CurrentBusiness = ChoiceModel.Business;
                ExtraPaymentModel.TotalMoney = PaymentModel.Total;
                ExtraPaymentModel.CurrentPayMethod = PaymentModel.PayMethod;
                ExtraPaymentModel.Complete = false;
                //准备门诊支付所需病人信息

                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = IdCardModel.Name,
                    PatientId = null,
                    IdNo = IdCardModel.IdCardNo,
                    GuardianNo = IdCardModel.IdCardNo,
                    CardNo = CardModel.CardNo,
                    CardType = CardModel.CardType,
                    Remain = 0
                };
                ExtraPaymentModel.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                GoToUnion();
            }
            else if (ChoiceModel.Business == Business.预约)
            {
                base.NoPayConfirm();
            }
            else
            {
                SecondConfirm(p =>
                {
                    if (p)
                    {
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
                            CardNo = CardModel.CardNo,
                            CardType = CardModel.CardType,
                            Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
                        };

                        //全医保报销处理
                        if (PaymentModel.Self == 0)
                        {
                            PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                            {
                                var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                                if (rest?.IsSuccess ?? false)
                                    ChangeNavigationContent($"金额:{ExtraPaymentModel.TotalMoney.In元()}");
                            }, null);
                        }
                        else
                        {
                            switch (PaymentModel.PayMethod)
                            {
                                case PayMethod.预缴金:
                                    var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                                    if (decimal.Parse(patientModel.accBalance) < PaymentModel.Total)
                                    {
                                        ShowAlert(false, "余额不足", "您的余额不足以支付该次诊疗费用，请充值");
                                        return;
                                    }

                                    PaymentModel.ConfirmAction?.BeginInvoke(cp =>
                                    {
                                        var rest = PaymentModel.ConfirmAction?.EndInvoke(cp);
                                        if (rest?.IsSuccess ?? false)
                                            ChangeNavigationContent($"金额:{ExtraPaymentModel.TotalMoney.In元()}");
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
                            }
                        }
                    }
                });
            }
        }
    }
}