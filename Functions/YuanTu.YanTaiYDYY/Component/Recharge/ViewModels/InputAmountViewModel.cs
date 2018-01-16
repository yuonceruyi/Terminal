using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.YanTaiYDYY.Component.Recharge.ViewModels
{
    public class InputAmountViewModel:YuanTu.Default.Component.Recharge.ViewModels.InputAmountViewModel
    {     
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {

            if (ChoiceModel.Business == Business.建档)
            {
                Hint = "充值金额最少1元";
            }
            else
            {
                Hint = "金额输入"; 
            }
            ChangeNavigationContent(null);
            Amount = null;
            
            
            PlaySound(SoundMapping.输入充值金额);
        }

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
            if (ChoiceModel.Business == Business.建档 && ExtraPaymentModel.TotalMoney < Convert.ToDecimal(100))
            {
                ShowAlert(false, "温馨提示", "办卡时，充值金额最少为1元", 10);
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
