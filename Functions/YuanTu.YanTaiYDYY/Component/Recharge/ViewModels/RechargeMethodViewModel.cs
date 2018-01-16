using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Net;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.CardReader;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Core.Navigating;

namespace YuanTu.YanTaiYDYY.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        public Font HeaderFont2 = new Font("微软雅黑", 14, FontStyle.Bold);

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        protected override void OnPayButtonClick(Info i)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            var payMethod = (PayMethod)i.Tag;

            if (choiceModel.Business != Business.建档)
            {
                OpRechargeModel.RechargeMethod = payMethod;
                ExtraPaymentModel.Complete = false;
                ExtraPaymentModel.CurrentBusiness = Business.充值;
                ExtraPaymentModel.CurrentPayMethod = payMethod;
                ExtraPaymentModel.FinishFunc = OnRechargeCallback;
                //准备门诊充值所需病人信息
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = PatientModel.当前病人信息.name,
                    PatientId = CardModel.CardNo,
                    IdNo = PatientModel.当前病人信息.idNo,
                    GuardianNo = PatientModel.当前病人信息.guardianNo,
                    CardNo = CardModel.CardNo,
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
                return;
            }
            
            var idCardModel = GetInstance<IIdCardModel>();
            OpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            //ExtraPaymentModel.CurrentBusiness = Business.建档;
            //强制指定为充值
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnJianDangRechargeCallback;
            //准备门诊充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = idCardModel.Name,
                PatientId = CardModel.CardNo,
                IdNo = idCardModel.IdCardNo,
                GuardianNo = null,
                CardNo = CardModel.CardNo,
                CardType = CardModel.CardType,
                Remain = 0m
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
                    Navigate(InnerA.JDCZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
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
                        ShowAlert(false, "现金充值", $"{time}后停止现金充值\r\n请使用其他支付方式");
                        return;
                    }
                }
            }

            Navigate(A.Third.Cash);
        }

        protected Task<Result> OnJianDangRechargeCallback()
        {
            var choiceModel = GetInstance<IChoiceModel>();
            if (choiceModel.Business != Business.建档)
            {
                return base.OnRechargeCallback();

            }

            //强制指定为充值
            ExtraPaymentModel.CurrentBusiness = Business.充值;

            var createModel = GetInstance<ICreateModel>();
            var idCardModel = GetInstance<IIdCardModel>();
            //此处执行真正的建档流程
            return DoCommand<Result>(lp =>
            {
                try
                {
                    //填充支付信息
                    createModel.Req病人建档发卡.cash = ExtraPaymentModel.TotalMoney.ToString("0");
                    FillRechargeRequest(createModel.Req病人建档发卡);
                    lp.ChangeText("正在建档，请稍候...");
                    createModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(createModel.Req病人建档发卡);
                    if (createModel.Res病人建档发卡?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        lp.ChangeText("正在发卡，请及时取卡。");
                        if (!FrameworkConst.VirtualThridPay)
                        {
                            PrintCard();
                        }
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "建档发卡成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分建档",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = CreatePrintables(true),
                            TipImage = "提示_凭条和发卡"
                        });
                        ChangeNavigationContent($"{idCardModel.Name}\r\n卡号{CardModel.CardNo}");
                        Navigate(InnerA.JDCZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        //第三方支付失败时去支付流程里面处理，不在业务里面处理
                        var state = NavigationEngine.State;
                        if (state != A.Third.PosUnion && state != A.Third.SiPay)
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "建档发卡失败",
                                TipMsg =$"您于{DateTimeCore.Now.ToString("HH:mm")}分建档并充值{ExtraPaymentModel.TotalMoney.In元()}失败,\r\n失败原因：{createModel.Res病人建档发卡?.msg}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = CreatePrintables(false),
                                TipImage = "提示_凭条",
                                DebugInfo = createModel.Res病人建档发卡?.msg
                            });
                            Navigate(InnerA.JDCZ.Print);
                        }
                        ExtraPaymentModel.Complete = true;

                        return Result.Fail(createModel.Res病人建档发卡?.code ?? -100, createModel.Res病人建档发卡?.msg);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error($"[{ExtraPaymentModel.CurrentPayMethod}充值]发起交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "建档充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new RechargeInfo
                    {
                        CardNo = createModel?.Req病人建档发卡.cardNo,
                        PatientId = createModel?.Res病人建档发卡?.data?.patientid,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = createModel?.Res病人建档发卡?.success ?? false,
                        ErrorMsg = createModel?.Res病人建档发卡?.msg
                    });
                }
            });
        }

        protected virtual void FillRechargeRequest(req病人建档发卡 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();

            req.tradeMode = extraPaymentModel.CurrentPayMethod.GetEnumDescription();//支付方式附加

            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.现金)
            {
                req.transNo = req.flowId;
            }
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
                        //req.extend = thirdpayinfo.buyerAccount;
                        //req.transNo = thirdpayinfo.outTradeNo;

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

        private void PrintCard()
        {
            var CreateModel = GetInstance<ICreateModel>();
            List<ZbrPrintTextItem> PrintText = new List<ZbrPrintTextItem>();
            ZbrPrintTextItem patientName = new ZbrPrintTextItem()
            {
                X = 726,
                Y = 150,
                FontSize = 14,
                Text = CreateModel.Req病人建档发卡.name,
            };
            ZbrPrintTextItem hospitalName = new ZbrPrintTextItem()
            {
                X = 80,
                Y = 510,
                FontSize = 10,
                Text = "烟台业达医院发卡"
            };
            PrintText.Add(patientName);
            PrintText.Add(hospitalName);

            List<ZbrPrintCodeItem> PrintCode = new List<ZbrPrintCodeItem>()
                                        {
                                            new ZbrPrintCodeItem()
                                        };

            var magCardDispenser = GetInstance<IMagCardDispenser[]>().FirstOrDefault(p => p.DeviceId == "ZBR_Mag");
            magCardDispenser?.PrintContent(PrintText, PrintCode);
        }

        protected virtual Queue<IPrintable> CreatePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }

            var CreateModel = GetInstance<ICreateModel>();
            var req = CreateModel.Req病人建档发卡;
            var res = CreateModel.Res病人建档发卡;
            var queue = PrintManager.NewQueue("自助发卡");

            #region  大粗字体
            var sb = new StringBuilder();
            sb.Append($"状态：办卡{(success ? "成功" : "失败")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = HeaderFont2 });
            #endregion

            sb = new StringBuilder();
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{req.name}\n");
            //sb.Append($"门诊号：{res.data?.patientid}\n");
            sb.Append($"卡号：{req.cardNo}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值金额：{req.cash.In元()}\n");
            if (success)
            {
                sb.Append($"已扣除就诊卡费用\n");
                sb.Append($"充值后余额：{res.data?.accBalance.In元()}\n");
            }
            else
            {
                sb.Append($"异常原因：{res.msg}\n");
                sb.Append($"请凭此条到人工窗口联系医院工作人员\n");
            }
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
                case PayMethod.现金:
                    sb.Append($"交易流水号：{req.flowId}\n");
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    sb.Append($"支付账号：{thirdpayinfo.buyerAccount}\n");
                    var str1 = string.Empty;
                    var str2 = string.Empty;
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n");
                    }
                    break;
                default:
                    break;
            }

            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
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

            var OpRechargeModel = GetInstance<IOpRechargeModel>();
            var req = OpRechargeModel.Req预缴金充值;

            var queue = PrintManager.NewQueue("门诊充值");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            #region  大粗字体
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = HeaderFont2 });
            #endregion

            sb = new StringBuilder();
            sb.Append($"姓名：{patientInfo.name}\n");
            //sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"卡号：{OpRechargeModel.Req预缴金充值.cardNo}\n");
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
                case PayMethod.现金:
                    sb.Append($"交易流水号：{req.flowId}\n");
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    sb.Append($"支付账号：{thirdpayinfo.buyerAccount}\n");
                    var str1 = string.Empty;
                    var str2 = string.Empty;
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n");
                    }
                    break;
                default:
                    break;
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
