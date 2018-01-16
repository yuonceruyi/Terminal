using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.JiaShanHospital.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        [Dependency]
        public IIdCardModel IdCardModel { get; set; }
        protected override void Confirm(Info i)
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
            if (!string.IsNullOrEmpty(ExtraPaymentModel?.PatientInfo?.Name))
            {
                ExtraPaymentModel.PatientInfo.Name = ExtraPaymentModel.PatientInfo.Name.Trim();
            }

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

    }
}
