using System;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;

namespace YuanTu.ZheJiangHospital.Component.Recharge.ViewModels
{
    public class InputAmountViewModel : Default.Component.Recharge.ViewModels.InputAmountViewModel
    {
        [Dependency]
        public IAccountModel Account { get; set; }

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
            if (Account.Balance + amount > 200000)
            {
                ShowAlert(false, "温馨提示", "银医通账户余额上限为2000元", 10);
                return;
            }
            ExtraPaymentModel.TotalMoney = amount * 100;
            if (ExtraPaymentModel.TotalMoney <= 0)
            {
                ShowAlert(false, "温馨提示", "请输入充值金额", 10);
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