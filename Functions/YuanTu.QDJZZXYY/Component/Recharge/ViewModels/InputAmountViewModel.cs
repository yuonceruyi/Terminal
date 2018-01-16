using System;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;


namespace YuanTu.QDJZZXYY.Component.Recharge.ViewModels
{
    public class InputAmountViewModel:YuanTu.Default.Component.Recharge.ViewModels.InputAmountViewModel
    {
        protected override void Confirm()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                ShowAlert(false, "温馨提示", "请输入充值金额", 10);
                return;
            }
            decimal amount;
            if (!decimal.TryParse(Amount, out amount))
            {
                ShowAlert(false, "温馨提示", "请输入正确的充值金额", 10);
                return;
            }
            ExtraPaymentModel.TotalMoney = amount * 100;
            if (ExtraPaymentModel.TotalMoney <= 0)
            {
                ShowAlert(false, "温馨提示", "请输入充值金额", 10);
                return;
            }
            if (ExtraPaymentModel.TotalMoney > 500000)
            {
                ShowAlert(false, "温馨提示", "最多充值5000元，请确认充值金额", 10);
                return;
            }

            ChangeNavigationContent(ExtraPaymentModel.TotalMoney.In元());

            switch (OpRechargeModel.RechargeMethod)
            {

                case PayMethod.未知:
                case PayMethod.现金:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    break;
                case PayMethod.银联:
                    Navigate(A.Third.PosUnion);
                    break;
                case PayMethod.支付宝:
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.微信支付:
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.苹果支付:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
