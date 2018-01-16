using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.NingXiaHospital.Component.ViewModels
{
    public class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        protected override void Confirm(Info i)
        {
            SecondConfirm(p =>
            {
                if (!p)
                    return;
                var payMethod = (PayMethod) i.Tag;
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
                    CardNo = CardModel.CardNo,
                    Remain = 0,
                    CardType = CardModel.CardType
                };

                HandlePaymethod(i, payMethod);
            });
        }
    }
}