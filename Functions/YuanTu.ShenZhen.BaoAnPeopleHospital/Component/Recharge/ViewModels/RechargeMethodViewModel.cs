﻿using Microsoft.Practices.Unity;
using Prism.Regions;
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
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.ShenZhenArea.Services;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {

        [Dependency]
        public IAccountingService Account_Service { get; set; }
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
                        accountNo = patientInfo.accountNo,
                        name = patientInfo.name
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(OpRechargeModel.Req预缴金充值);

                    OpRechargeModel.Res预缴金充值 = DataHandlerEx.预缴金充值(OpRechargeModel.Req预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (OpRechargeModel.Res预缴金充值.success)
                    {
                        //充值成功后记账到钱箱里面
                        if (ExtraPaymentModel.CurrentPayMethod == PayMethod.现金)
                        {
                            DBManager.Insert(new CashInputInfo
                            {
                                TotalSeconds = ExtraPaymentModel.TotalMoney
                            });
                        }
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true),
                            TipImage = "提示_凭条"
                        });

                        patientInfo.accBalance = OpRechargeModel.Res预缴金充值.data.cash;
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context, $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

                        Account_Service.充值记账(true);
                        Navigate(A.CZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        if (DataHandler.UnKnowErrorCode.Contains(OpRechargeModel.Res预缴金充值.code))  //单边账。。
                        {
                            OpRechargeModel.Res预缴金充值.msg = $"{OpRechargeModel.Res预缴金充值.code} 服务受理异常,充值失败!";
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "充值单边账",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = BillErrSheBaoPayPrintables(),
                                TipImage = "提示_凭条"
                            });

                            ShowAlert(false, "充值结果未知", $"{OpRechargeModel.Res预缴金充值.code} 服务受理异常,充值失败!", 20);
                            Account_Service.充值记账(false, true);
                            Navigate(A.JF.Print);
                            return Result.Fail(OpRechargeModel.Res预缴金充值?.code ?? -100, OpRechargeModel.Res预缴金充值?.msg);
                        }

                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(false),
                            TipImage = "提示_凭条",
                            DebugInfo = OpRechargeModel.Res预缴金充值?.msg
                        });
                        Account_Service.充值记账(false);
                        Navigate(A.CZ.Print);
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
                        OpRechargeModel.Req预缴金充值.bankCardNo = posinfo.CardNo;
                        OpRechargeModel.Req预缴金充值.bankTime = posinfo.TransTime;
                        OpRechargeModel.Req预缴金充值.bankDate = posinfo.TransDate;
                        OpRechargeModel.Req预缴金充值.posTransNo = posinfo.Trace;
                        OpRechargeModel.Req预缴金充值.bankTransNo = posinfo.Ref;
                        OpRechargeModel.Req预缴金充值.deviceInfo = posinfo.TId;
                        OpRechargeModel.Req预缴金充值.sellerAccountNo = posinfo.MId;
                        OpRechargeModel.Req预缴金充值.posIndexNo = posinfo.Batch;
                    }
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    if (thirdpayinfo != null)
                    {
                        OpRechargeModel.Req预缴金充值.payAccountNo = thirdpayinfo.buyerAccount;
                        OpRechargeModel.Req预缴金充值.transNo = thirdpayinfo.outPayNo;
                        OpRechargeModel.Req预缴金充值.outTradeNo = thirdpayinfo.outTradeNo;
                        OpRechargeModel.Req预缴金充值.tradeTime = thirdpayinfo.paymentTime;
                    }
                    break;
                case PayMethod.现金:
                    OpRechargeModel.Req预缴金充值.transNo = req.flowId;
                    break;
            }
        }


        protected Queue<IPrintable> BillErrSheBaoPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊充值单边账");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            sb.Append($"异常原因：{OpRechargeModel.Res预缴金充值.msg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请凭该凭条找工作人员处理。\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            
            if (string.IsNullOrEmpty(PatientModel.当前病人信息.accountNo))
            {
                ShowAlert(false, "该账户不能充值", "预交金账户信息不存在,不能充值。请到人工窗口激活门诊账户信息");
                Navigate(A.Home);
            }
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
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            if (success)
            {
                sb.Append($"充值后余额：{OpRechargeModel.Res预缴金充值.data.cash.In元()}\n");
                //sb.Append($"收据号：{OpRechargeModel.Res预缴金充值.data.orderNo}\n");
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