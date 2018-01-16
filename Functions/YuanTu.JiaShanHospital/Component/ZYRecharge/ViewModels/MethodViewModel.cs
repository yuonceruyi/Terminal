using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.JiaShanHospital.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel:YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    IpRechargeModel.Res住院预缴金充值 = null;
                    var patientInfo = PatientModel.住院患者信息;
                    IpRechargeModel.Req住院预缴金充值 = new req住院预缴金充值
                    {
                        patientId = PatientModel.Res住院患者信息查询.extend,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString(),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientHosId,
                        patientHosId = patientInfo.patientHosId,
                        patientName = PatientModel.住院患者信息.name
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(IpRechargeModel.Req住院预缴金充值);

                    IpRechargeModel.Res住院预缴金充值 = DataHandlerEx.住院预缴金充值(IpRechargeModel.Req住院预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (IpRechargeModel.Res住院预缴金充值.success)
                    {
                        ExtraPaymentModel.Complete = true;
                        //PrintModel.SetPrintInfo(true, "缴住院押金成功",
                        //    $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(true));
                        //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条"));
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(true);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                TipImage = "提示_凭条_740"
                            });
                        }
                        else if (FrameworkConst.DeviceType == "YT-BG350")
                        {
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(true),
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(true),
                                TipImage = "提示_凭条"
                            });
                        }
                        patientInfo.accBalance = IpRechargeModel.Res住院预缴金充值.data.cash;
                        ChangeNavigationContent(A.ZY.InPatientInfo, A.ZhuYuan_Context,
                            $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                        Navigate(A.ZYCZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        //PrintModel.SetPrintInfo(false, "充值失败",
                        //    $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(false),
                        //    IpRechargeModel.Res住院预缴金充值?.msg);
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(false);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金失败",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分缴押金{ExtraPaymentModel.TotalMoney.In元()}失败",
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "充值失败",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(false),
                                TipImage = "提示_凭条",
                                DebugInfo = IpRechargeModel.Res住院预缴金充值?.msg
                            });
                        }
                        Navigate(A.ZYCZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(IpRechargeModel.Res住院预缴金充值?.code ?? -100, IpRechargeModel.Res住院预缴金充值.msg);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new ZYRechargeInfo
                    {
                        PatientId = PatientModel?.住院患者信息?.patientHosId,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = IpRechargeModel.Res住院预缴金充值?.success ?? false,
                        ErrorMsg = IpRechargeModel.Res住院预缴金充值?.msg
                    });
                }
            });
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
            sb.Append($"住院号：{PatientModel.Res住院患者信息查询.extend}\n");
            sb.Append($"缴押金方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"缴押金金额：{IpRechargeModel.Req住院预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"收据号：{IpRechargeModel.Res住院预缴金充值.data.receiptNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管，出院结算请出示\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        protected override void FillRechargeRequest(req住院预缴金充值 req)
        {
            if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (ExtraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     ExtraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
    }
}
