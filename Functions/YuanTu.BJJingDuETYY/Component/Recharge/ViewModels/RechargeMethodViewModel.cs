﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Log;

namespace YuanTu.BJJingDuETYY.Component.Recharge.ViewModels
{

    public class RechargeMethodViewModel : YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    OpRechargeModel.Res预缴金充值 = null;
                    var patientInfo = PatientModel.当前病人信息;
                    OpRechargeModel.Req预缴金充值 = new req预缴金充值
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString("0"),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientId,
                        name = patientInfo.name
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(OpRechargeModel.Req预缴金充值);

                    OpRechargeModel.Res预缴金充值 = DataHandlerEx.预缴金充值(OpRechargeModel.Req预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (OpRechargeModel.Res预缴金充值.success)
                    {
                        #region 同步充值数据到HIS
                        OpRechargeModel.Req充值数据同步到his系统 = new req充值数据同步到his系统
                        {
                            patientId = patientInfo.patientId,
                            cardType = ((int)CardModel.CardType).ToString(),
                            cardNo = CardModel.CardNo,
                            operId = FrameworkConst.OperatorId,
                            cash = ExtraPaymentModel.TotalMoney.ToString(),
                            tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                            accountNo = patientInfo.patientId,
                            transNo = OpRechargeModel.Res预缴金充值.data.sFlowId,//平台交易流水号
                        };
                        //填充各种支付方式附加数据
                        FillRechargeRequest2HIS(OpRechargeModel.Req充值数据同步到his系统);

                        OpRechargeModel.Res充值数据同步到his系统 = DataHandlerEx.充值数据同步到his系统(OpRechargeModel.Req充值数据同步到his系统);
                        //失败不处理，平台成功就认为成功

                        #endregion

                        ExtraPaymentModel.Complete = true;
                        //PrintModel.SetPrintInfo(true, "充值成功",
                        //    $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), OpRechargePrintables(true));
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true),
                            TipImage = "提示_凭条"
                        });
                        patientInfo.accBalance = OpRechargeModel.Res预缴金充值.data.cash;
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context,
                            $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                        Navigate(A.CZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        //PrintModel.SetPrintInfo(false, "充值失败",
                        //    $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), OpRechargePrintables(false),
                        //    OpRechargeModel.Res预缴金充值?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(false),
                            TipImage = "提示_凭条",
                            DebugInfo = OpRechargeModel.Res预缴金充值?.msg
                        });
                        // Navigate(A.CZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(OpRechargeModel.Res预缴金充值?.code ?? -100, OpRechargeModel.Res预缴金充值.msg);
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
                    DBManager.Insert(new RechargeInfo
                    {
                        CardNo = CardModel?.CardNo,
                        PatientId = PatientModel?.当前病人信息?.patientId,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = OpRechargeModel.Res预缴金充值?.success ?? false,
                        ErrorMsg = OpRechargeModel.Res预缴金充值?.msg
                    });
                }
            });
        }

        protected override void FillRechargeRequest(req预缴金充值 req)
        {
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    if (posinfo != null)
                    {
                        req.bankCardNo = posinfo.CardNo;
                        req.bankTime = posinfo.TransTime;
                        req.bankDate = posinfo?.TransDate;
                        req.posTransNo = posinfo.Trace;
                        req.bankTransNo = posinfo.Ref;
                        req.deviceInfo = posinfo.TId;
                        req.sellerAccountNo = posinfo.MId;
                    }
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    if (thirdpayinfo != null)
                    {
                        req.payAccountNo = thirdpayinfo.buyerAccount;
                        req.transNo = thirdpayinfo.outPayNo;
                        req.outTradeNo = thirdpayinfo.outTradeNo;
                        req.tradeTime = thirdpayinfo.paymentTime;
                    }
                    break;
                case PayMethod.现金:
                    req.transNo = req.flowId;
                    break;
            }
        }


        protected virtual void FillRechargeRequest2HIS(req充值数据同步到his系统 req)
        {
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
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
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    if (thirdpayinfo != null)
                    {
                        req.extend = thirdpayinfo.buyerAccount;
                        req.transNo = thirdpayinfo.outTradeNo;
                    }
                    break;
                case PayMethod.现金:
                    //req.transNo = req.flowId;
                    break;
            }
        }


        protected override void OnCAClick()
        {

            if (ConfigurationManager.GetValueInt("StopRecharge:Enabled") == 1)
            {
                DateTime dtime;
                var time = ConfigurationManager.GetValue("StopRecharge:Time");
                if (DateTime.TryParseExact(time, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dtime))
                {
                    if (DateTimeCore.Now >
                        new DateTime(DateTimeCore.Now.Year, DateTimeCore.Now.Month, DateTimeCore.Now.Day, dtime.Hour,
                            dtime.Minute, 0))
                    {
                        ShowAlert(false, "现金充值", $"{time}后停止现金充值\r\n请使用其他支付方式，如需现金充值，请到人工窗口");
                        return;
                    }
                }
            }

            Navigate(A.Third.Cash);
        }

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
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");

            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        sb.Append($"{posinfo?.Receipt}\n");
                    }
                    else
                    {
                        sb.Append($"银行卡号：{posinfo.CardNo}\n");
                        sb.Append($"授权号：{posinfo.Auth}\n");
                        sb.Append($"终端编号：{posinfo.TId}\n");
                        sb.Append($"终端流水：{posinfo.Trace}\n");
                        sb.Append($"批次号：{posinfo.Batch}\n");
                        sb.Append($"检索参考号：{posinfo.Ref}\n");
                        sb.Append($"交易日期：{posinfo.TransDate}\n");
                        sb.Append($"交易时间：{posinfo.TransTime}\n");
                    }
                    break;
                default:
                    break;
            }
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{OpRechargeModel.Res预缴金充值.data.cash.In元()}\n");
                sb.Append($"交易流水号：{OpRechargeModel.Res预缴金充值.data.sFlowId}\n");
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
