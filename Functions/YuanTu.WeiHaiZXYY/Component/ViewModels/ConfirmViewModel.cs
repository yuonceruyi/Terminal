using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.WeiHaiZXYY.Component.ViewModels
{
    public class ConfirmViewModel : YuanTu.Default.Component.ViewModels.ConfirmViewModel
    {
        protected override void Confirm(Info i)
        {
            PaymentModel.PayMethod = (PayMethod)i.Tag;
            var extra = GetInstance<IExtraPaymentModel>();
            extra.CurrentBusiness = ChoiceModel.Business;
            extra.TotalMoney = PaymentModel.Total;
            extra.CurrentPayMethod = PaymentModel.PayMethod;
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
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                    if (decimal.Parse(patientModel.accountNo)*100 < PaymentModel.Total)
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
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToUnion();
                    break;
                case PayMethod.社保:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    GoToSiPay();
                    break;
                case PayMethod.支付宝:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.微信支付:
                    extra.FinishFunc = () => Task.Run(PaymentModel.ConfirmAction);
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.苹果支付:
                    ShowAlert(false, "支付确认", "业务未实现");
                    break;
            }
        }
        protected void GoToSiPay()
        {
            Navigate(A.Third.SiPay);
        }

        protected override void QuickRecharge()
        {
            //临时卡不允许充值
            //var patientModel = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            //if (string.IsNullOrWhiteSpace(patientModel?.idNo) &&
            //    string.IsNullOrWhiteSpace(patientModel?.guardianNo))
            //{
            //    ShowAlert(false, "自助充值", "临时卡不允许充值，请到人工窗口补充身份信息\r\n或直接进行银联或医保缴费");
            //    return;
            //}
            //else
            //{
                StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
            //}

        }
    }
}
