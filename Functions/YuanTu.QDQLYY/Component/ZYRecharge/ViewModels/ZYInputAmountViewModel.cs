using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Core.Extension;

namespace YuanTu.QDQLYY.Component.ZYRecharge.ViewModels
{
    public class ZYInputAmountViewModel:YuanTu.Default.Component.ZYRecharge.ViewModels.ZYInputAmountViewModel
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
            if (ExtraPaymentModel.TotalMoney < 50000)
            {
                ShowAlert(false, "温馨提示", "住院充值最低充值500元，请确认充值金额", 10);
                //return;
            }
            ChangeNavigationContent(ExtraPaymentModel.TotalMoney.In元());

            switch (IpRechargeModel.RechargeMethod)
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
