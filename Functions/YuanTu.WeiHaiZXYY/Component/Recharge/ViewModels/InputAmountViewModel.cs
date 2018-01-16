using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;

namespace YuanTu.WeiHaiZXYY.Component.Recharge.ViewModels
{
    public class InputAmountViewModel : YuanTu.Default.Component.Recharge.ViewModels.InputAmountViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
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
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var yue = decimal.Parse(patientInfo.accountNo);
            if (yue >= 2000)
            {
                ShowAlert(false, "温馨提示", $"您当前余额为{yue}元，卡内余额不能超过2000元，超额请使用现金充值或者到人工窗口处理");
                return;
            }
            if (yue + decimal.Parse(Amount) > 2000)
            {
                ShowAlert(false, "温馨提示", $"您当前余额为{yue}元，最多还能充值{2000 - yue}元,卡内余额不能超过2000元，超额请使用现金充值或者到人工窗口处理");
                return;
            }

            ChangeNavigationContent(ExtraPaymentModel.TotalMoney.In元());
            Navigate(A.Third.PosUnion);
        }
    }
}
