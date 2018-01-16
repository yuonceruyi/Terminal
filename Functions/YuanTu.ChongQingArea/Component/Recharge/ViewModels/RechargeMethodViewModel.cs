using System;
using System.Collections.Generic;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.ChongQingArea.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需打印凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("门诊充值");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            if (string.IsNullOrEmpty(patientInfo.extend))
            {
                sb.Append($"门诊号：{patientInfo.patientId}\n");
            }
            else
            {
                sb.Append($"门诊号：{patientInfo.extend.Split('|')[0]}\n");
            }
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
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                ;
                for (int i = 1; i < Startup.PrintSpaceLine; i++)
                {
                    queue.Enqueue(new PrintItemText { Text = "　　 \r\n" });
                }
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            }
            else
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            return queue;
        }

        protected override void OnPayButtonClick(Info i)
        {
            var payMethod = (PayMethod)i.Tag;
            OpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnRechargeCallback;
            //准备门诊充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.当前病人信息.name,
                PatientId = PatientModel.当前病人信息.patientId,
                IdNo = PatientModel.当前病人信息.idNo,
                GuardianNo = PatientModel.当前病人信息.guardianNo,
                CardNo = PatientModel.当前病人信息.cardNo,
                CardType = CardModel.CardType,
                Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
            };

            ChangeNavigationContent(OpRechargeModel.RechargeMethod.ToString());
            switch (payMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    OnCAClick();
                    break;

                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(A.CZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}