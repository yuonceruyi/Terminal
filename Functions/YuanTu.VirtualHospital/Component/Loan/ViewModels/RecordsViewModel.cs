using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.Loan.ViewModels
{
    public class RecordsViewModel : ViewModelBase
    {
        public override string Title => "请触摸下方卡片选择借款记录";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = LoanModel.Res查询借款账单.data.billItem.Select(p => new InfoMore()
            {
                Title = p.billDate,
                SubTitle = p.hospName,
                Amount = decimal.Parse(p.billFee),
                Tag = p,
                ConfirmCommand = confirmCommand,
                IsEnabled = p.status != "2",
                DisableText = p.statusDesc,
            });
            Data = new ObservableCollection<Info>(list);
        }

        protected virtual void Confirm(Info i)
        {
            var item = i.Tag.As<借款账单详情>();
            LoanModel.所选借款账单 = item;

            DoCommand(lp =>
            {
                var req = new req用户借款还款下单()
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    billNo = item.billNo,
                    status = "2",
                };
                req.transNo = new string(req.flowId.Where(char.IsDigit).ToArray());
                LoanModel.Req用户借款还款下单 = req;
                var res = DataHandlerEx.用户借款还款下单(req);
                LoanModel.Res用户借款还款下单 = res;
                if (res == null || !res.success)
                {
                    ShowAlert(false, "还款下单", $"还款下单失败\n{res?.msg}");
                    return;
                }

                var info = res.data;

                var amount = decimal.Parse(info.repaymentAmt);
                PaymentModel.Self = amount;
                PaymentModel.Insurance = 0m;
                PaymentModel.Total = amount;
                PaymentModel.NoPay = false;
                PaymentModel.ConfirmAction = Confirm;

                var items = new List<PayInfoItem>()
                {
                    new PayInfoItem("账单状态：",item.statusDesc),
                    new PayInfoItem("借款日期：",item.billDate),
                    new PayInfoItem("借款医院：",item.hospName),
                    new PayInfoItem("借款金额：",amount.In元()),
                };
                if (info.overdueStatus == "1")
                    items.Insert(1, new PayInfoItem("逾期天数：", info.overdueDays));
                PaymentModel.MidList = items;

                Next();
            });
        }

        private Result Confirm()
        {
            return DoCommand(Act).Result;
        }

        private Result Act(LoadingProcesser p)
        {
            try
            {
                p.ChangeText("正在进行还款，请稍候...");
                LoanModel.Req用户借款还款确认 = null;
                var printerName = ConfigurationManager.GetValue("Printer:Receipt");
                var req = new req用户借款还款确认
                {
                    repayBillNo = LoanModel.Res用户借款还款下单.data.repayBillNo,

                    cash = ExtraPaymentModel.TotalMoney.ToString("0"),
                    tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                };
                LoanModel.Req用户借款还款确认 = req;

                //填充各种支付方式附加数据
                FillRechargeRequest(req);

                var res = DataHandlerEx.用户借款还款确认(req);
                LoanModel.Res用户借款还款确认 = res;
                ExtraPaymentModel.Complete = true;
                if (res == null || !res.success)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "还款失败",
                        TipMsg = $"您于{DateTimeCore.Now:HH:mm}分还款{ExtraPaymentModel.TotalMoney.In元()}失败",
                        PrinterName = printerName,
                        Printables = Printables(false),
                        TipImage = "提示_凭条",
                        DebugInfo = res?.msg
                    });
                    Navigate(InnerA.Loan.Print);
                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(res?.code ?? -100, res?.msg);
                }

                ExtraPaymentModel.Complete = true;
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "还款成功",
                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功还款{ExtraPaymentModel.TotalMoney.In元()}",
                    PrinterName = printerName,
                    Printables = Printables(true),
                    TipImage = "提示_凭条"
                });

                Navigate(A.CZ.Print);
                return Result.Success();
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"[{ExtraPaymentModel.CurrentPayMethod}还款]发起还款交易时出现异常，原因:{ex}");
                ShowAlert(false, "还款失败", "发生系统异常 ");
                return Result.Fail("系统异常");
            }
        }

        protected virtual void FillRechargeRequest(req用户借款还款确认 req)
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

        protected virtual Queue<IPrintable> Printables(bool success)
        {
            //只有现金才需要打失败凭条
            if (!success && ExtraPaymentModel.CurrentPayMethod != PayMethod.现金)
                return null;
            var queue = PrintManager.NewQueue("还款");
            var patientInfo = PatientModel.当前病人信息;
            var item = LoanModel.所选借款账单;
            var data = LoanModel.Res用户借款还款下单.data;
            var res = LoanModel.Res用户借款还款确认;

            var sb = new StringBuilder();
            sb.Append($"状态：还款{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"账单日期：{item.billDate}\n");
            sb.Append($"账单金额：{item.billFee.In元()}\n");
            sb.Append($"还款方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"还款金额：{data.cash.In元()}\n");
            sb.Append($"还款订单编号：{data.repayBillNo}\n");
            if (!success)
                sb.Append($"异常原因：{res.msg}\n");

            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

        #region Binding

        private ObservableCollection<Info> _data;

        public ObservableCollection<Info> Data
        {
            get => _data;
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        #region DI

        [Dependency]
        public ILoanModel LoanModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        #endregion DI
    }
}