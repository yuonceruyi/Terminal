using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.ShenZhenArea.Services;
using YuanTu.Core.Gateway;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Default.Component.Tools.Models;
using System.Text;
using System.Collections.Generic;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {

        [Dependency]
        public IAccountingService Account_Service { get; set; }
        protected override void OnCAClick()
        {
            if (ConfigurationManager.GetValue("CashBox:Type") == "JCM")
            {
                Navigate(A.Third.JCMCash);
                return;
            }
            if (FrameworkConst.DeviceType.ToUpper() == "YT-740")
            {
                Navigate(A.Third.AtmCash);
                return;
            }

            Navigate(A.Third.Cash);
        }

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
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString(),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.accountNo
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(IpRechargeModel.Req住院预缴金充值);

                    IpRechargeModel.Res住院预缴金充值 = DataHandlerEx.住院预缴金充值(IpRechargeModel.Req住院预缴金充值);

                    ExtraPaymentModel.Complete = true;
                    if (IpRechargeModel.Res住院预缴金充值.success)
                    {
                        Account_Service.住院押金记账(true);
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
                        Account_Service.住院押金记账(true, DataHandler.UnKnowErrorCode.Contains(IpRechargeModel.Res住院预缴金充值.code));
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
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}\n请妥善保管好充值小票，凭小票到住院收费处打印押金单。",
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

        protected override void FillRechargeRequest(req住院预缴金充值 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                //Logger.Main.Info($"这里是住院押金充值，银联刷卡的json数据是:{posinfo.ToJsonString()}");
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = DateTimeCore.Now.ToString("yyyy") + posinfo.TransDate + posinfo.TransTime;
                    req.posTransNo = posinfo.Trace.PadLeft(6, '0');
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                    req.posIndexNo = posinfo.Batch;
                    req.extend = posinfo.Auth;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }


        protected override Queue<IPrintable> IpRechargePrintables(bool success)
        {
            if (!success)
            {
                //if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("住院押金");
            var patientInfo = PatientModel.住院患者信息;
            var sb = new StringBuilder();
            sb.Append($"状态：缴押金{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"住院号：{patientInfo.patientHosId}\n");
            sb.Append($"缴押金方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"缴押金前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"缴押金金额：{IpRechargeModel.Req住院预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"缴押金后余额：{IpRechargeModel.Res住院预缴金充值.data.cash.In元()}\n");
                sb.Append($"收据号：{IpRechargeModel.Res住院预缴金充值.data.receiptNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好充值小票，凭小票到住院收费处打印押金单。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}