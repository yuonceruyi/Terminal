using System;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.XiaoShanZYY.Component.Auth.Models;
using PatientInfo = YuanTu.Default.Component.Tools.Models.PatientInfo;

namespace YuanTu.XiaoShanZYY.Component.ViewModels
{
    internal class ConfirmViewModel : Default.Component.ViewModels.ConfirmViewModel
    {
        protected override void SecondConfirm(Action<bool> act)
        {
            act(true);
        }

        [Dependency]
        public IAuthModel Auth { get; set; }

        protected override void Confirm(Info i)
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
                
                var idNo = Auth.人员信息.身份证号;
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = Auth.人员信息.病人姓名,
                    PatientId = Auth.人员信息.就诊卡号,
                    IdNo = string.IsNullOrWhiteSpace(idNo) ? "330106200001011235" : idNo,
                    GuardianNo = "",
                    CardNo = Auth.Info.CardNo,
                    Remain = Auth.Info.Remain,
                    CardType = CardModel.CardType,
                };

                HandlePaymethod(i, payMethod);
            });
        }
    }
}