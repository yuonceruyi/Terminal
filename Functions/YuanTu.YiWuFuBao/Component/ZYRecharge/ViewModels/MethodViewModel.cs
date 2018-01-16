using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;

namespace YuanTu.YiWuFuBao.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel: YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }
        protected override void OnCAClick()
        {
            Navigate(A.Third.Cash);
        }

        protected override Queue<IPrintable> IpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("住院押金");
            var patientInfo = PatientModel.住院患者信息;
            var sb = new StringBuilder();
            sb.Append($"状态：缴押金{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"住院号：{CardModel?.CardNo}\n");
            sb.Append($"住院ID：{patientInfo.patientHosId}\n");
            sb.Append($"缴押金方式：{ExtraPaymentModel.CurrentPayMethod}\n");
           // sb.Append($"缴押金前余额：{patientInfo.accBalance.In元()}\n");
           // sb.Append($"缴押金金额：{IpRechargeModel.Req住院预缴金充值.cash.In元()}\n");
            if (success)
            {
           //     sb.Append($"缴押金后余额：{IpRechargeModel.Res住院预缴金充值.data.cash.In元()}\n");
                sb.Append($"充值金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
                sb.Append($"收据号：{IpRechargeModel.Res住院预缴金充值.data.receiptNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}
