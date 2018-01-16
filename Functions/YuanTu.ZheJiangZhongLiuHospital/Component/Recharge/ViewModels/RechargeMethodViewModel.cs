using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.ZheJiangZhongLiuHospital.Component.Recharge.Models;
using YuanTu.ZheJiangZhongLiuHospital.ICBC;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    var req充值 = new Req充值
                    {
                        Chanel = "1",
                        AccountNo = PatientModel.当前病人信息.accountNo,
                        AccountId = $"{PatientModel.当前病人信息.name}^{PatientModel.当前病人信息.idNo}",
                        Cash = ExtraPaymentModel.TotalMoney.ToString("0"),

                        OperId = FrameworkConst.OperatorId,
                        DeviceInfo = FrameworkConst.OperatorId,
                        TradeSerial = DateTimeCore.Now.ToString("yyyyMMddHHmmssffff"),


                        Rsv1 = "",
                        Rsv2 = ""
                    };
                    //填充各种支付方式附加数据
                    FillRechargeRequest(req充值);

                    var result = PConnection.Handle<Res充值>(req充值);
                    ExtraPaymentModel.Complete = true;
                    if (result)
                    {
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true,result.Value.Remain),
                            TipImage = "提示_凭条"
                        });
                        PatientModel.当前病人信息.accBalance = PatientModel.当前病人信息.accBalance+ ExtraPaymentModel.TotalMoney;
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context,
                            $"{PatientModel.当前病人信息.name}\r\n余额{PatientModel.当前病人信息.accBalance.In元()}");
                        Navigate(A.CZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now:HH:mm}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(false),
                            TipImage = "提示_凭条",
                            DebugInfo = result.Value?.ResultMark
                        });
                        Navigate(A.CZ.Print);
                        return Result.Fail(result.Value?.ResultMark);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
            });
        }

        private void FillRechargeRequest(Req充值 req)
        {
            req.BankCardNo = string.Empty;
            req.MisposSerNo = string.Empty;
            req.MisposDate = string.Empty;
            req.MisposTime = string.Empty;
            req.MisposTermNo = string.Empty;
            req.MisposIndexNo = string.Empty;
            req.MisposInfo = string.Empty;
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    req.TradeMode = "2";
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    if (posinfo != null)
                    {
                        req.BankCardNo = posinfo.CardNo;
                        req.MisposSerNo = posinfo.Trace;
                        req.MisposDate = posinfo.TransDate;
                        req.MisposTime = posinfo.TransTime;
                        req.MisposTermNo = posinfo.TId;
                        req.MisposIndexNo = posinfo.Ref;
                        req.MisposInfo = posinfo.Receipt; // TODO
                    }
                    break;

                case PayMethod.现金:
                    req.TradeMode = "1";
                    break;
            }
        }

        protected  Queue<IPrintable> OpRechargePrintables(bool success,string remain)
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
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{remain.In元()}\n");
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

            return queue;
        }


    }
}