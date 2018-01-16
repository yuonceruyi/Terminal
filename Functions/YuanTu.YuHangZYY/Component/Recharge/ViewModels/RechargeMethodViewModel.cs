using System.Collections.Generic;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;

namespace YuanTu.YuHangZYY.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel:Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("门诊充值");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{OpRechargeModel.Res预缴金充值.data.cash.In元()}\n");
                sb.Append($"收据号：{OpRechargeModel.Res预缴金充值.data.orderNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{OpRechargeModel.Res预缴金充值.msg}\n");
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
