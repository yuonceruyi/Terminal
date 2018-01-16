using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        [Dependency]
        public IAuthModel Auth { get; set; }

        [Dependency]
        public IRechargeModel Recharge { get; set; }

        [Dependency]
        public IAccountModel Account { get; set; }
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
                Name = Auth.Info.NAME,
                PatientId = Auth.Info.PATIENTID.ToString(),
                IdNo = Auth.Info.IDNO,
                CardNo = Auth.Info.CARDNO,
                CardType = CardModel.CardType,
                Remain = Account.Balance
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

        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");

                    Recharge.Balance = Account.Balance;
                    Recharge.Res充值 = null;
                    Recharge.Req充值 = new Req充值
                    {
                        Chanel = "1",
                        AccountNo = Account.AccountNo,
                        AccountId = Account.AccountId,
                        Cash = ExtraPaymentModel.TotalMoney.ToString("0"),

                        OperId = FrameworkConst.OperatorId,
                        DeviceInfo = FrameworkConst.OperatorId,
                        TradeSerial = BalanceDeal.TradeSerial,

                        Rsv1 = "",
                        Rsv2 = ""
                    };
                    //填充各种支付方式附加数据
                    FillRechargeRequest(Recharge.Req充值);

                    var result = PConnection.Handle<Res充值>(Recharge.Req充值);
                    Recharge.Res充值 = result.Value;
                    ExtraPaymentModel.Complete = true;
                    Recharge.Success = result.IsSuccess;
                    BalanceDeal.InsertRechargeInfo(result.IsSuccess, Recharge.Req充值, Recharge.Res充值, Recharge.Counts);
                    InsertThirdPayInfo(Recharge.Res充值.BankSerial);
                    if (Recharge.Success)
                    {
                        Recharge.NewBalance = Convert.ToDecimal(Recharge.Res充值.Remain);
                        Account.Balance = Recharge.NewBalance;

                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true),
                            TipImage = "提示_凭条"
                        });
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context,
                            $"{Auth.Info.NAME}\r\n余额{Account.Balance.In元()}");
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
                            DebugInfo = Recharge.Res充值?.ResultMark
                        });
                        Navigate(A.CZ.Print);
                        return Result.Fail(Recharge.Res充值?.ResultMark);
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
                        PatientId = Auth?.Info?.PATIENTID.ToString(),
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = Recharge.Success,
                        ErrorMsg = Recharge.Res充值?.ResultMark
                    });
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

        private void InsertThirdPayInfo(string transNo)
        {
            var thirdPayInfo = new BalanceDeal.ThirdPayInfo()
            {
                cardNo = Auth.Info.CARDNO,
                hisPatientId = Auth.Info.PATIENTID.ToString(),
                patientName = Auth.Info.NAME,
                idNo = Auth.Info.IDNO,
                amount = ExtraPaymentModel.TotalMoney,
                recharge = true,
                transNo = transNo,
            };
            var card = GetInstance<ICardModel>();
            switch (card.CardType)
            {
                case CardType.身份证:
                    thirdPayInfo.cardType = "1";
                    break;

                case CardType.社保卡:
                    thirdPayInfo.cardType = "10";
                    break;

                case CardType.市医保卡:
                    thirdPayInfo.cardType = "10";
                    break;

                case CardType.省医保卡:
                    thirdPayInfo.cardType = "10";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    thirdPayInfo.tradeMode = "DB";
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    if (posinfo != null)
                    {
                        thirdPayInfo.bankCardNo = posinfo.CardNo;
                        thirdPayInfo.bankDate = posinfo.TransDate;
                        thirdPayInfo.bankTime = posinfo.TransTime;
                        thirdPayInfo.bankTransNo = posinfo.Ref;
                    }
                    break;

                case PayMethod.现金:
                    thirdPayInfo.tradeMode = "CA";
                    break;
            }
            BalanceDeal.IntertThirdPayInfo(thirdPayInfo);
        }

        protected override Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                    return null;
            var queue = PrintManager.NewQueue("门诊充值");
            var sb = new StringBuilder();
            sb.Append($"状态：充值{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{Auth.Info.NAME}\n");
            sb.Append($"就诊卡号：{Auth.Info.CARDNO}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{Recharge.Balance.In元()}\n");
            sb.Append($"充值金额：{Recharge.Req充值.Cash.In元()}\n");
            sb.Append($"本地流水：{Recharge.Req充值.TradeSerial}\n");
            if (success)
            {
                sb.Append($"充值后余额：{Recharge.NewBalance.In元()}\n");
                sb.Append($"银行流水：{Recharge.Res充值.BankSerial}\n");
            }
            else
            {
                sb.Append($"异常原因：{Recharge.Res充值?.ResultMark ?? "通信异常 结果未知"}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});

            return queue;
        }
    }
}