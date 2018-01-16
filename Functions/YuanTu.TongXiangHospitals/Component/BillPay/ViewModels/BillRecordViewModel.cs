using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Services.PrintService;
using YuanTu.TongXiangHospitals.HealthInsurance;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;

namespace YuanTu.TongXiangHospitals.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        public decimal TotalBillFee { get; set; }
        public string TotalBillNo { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "我要缴费";
            var data = BillRecordModel.Res获取缴费概要信息.data;
            decimal sum = data.Sum(p => decimal.Parse(p.billFee));
            Collection = new List<PageData>{ new PageData()
            {
                CatalogContent = $"金额 {sum.In元()}",
                List = data.SelectMany(i=>i.billItem),
            }};
            BillCount = $"{data.Count}张处方单";
            TotalAmount = sum.In元();
            TotalBillFee = sum;
            TotalBillNo = string.Join(",", data.Select(i => i.billNo));
        }

        protected override void Do()
        {
            ChangeNavigationContent($"金额:{TotalAmount}");

            SiModel.诊间结算 = false;
            DoCommand(ctx =>
            {
                ctx.ChangeText("正在进行门诊缴费预结算，请稍候...");
                var result = PreSettle();
                if (!result.IsSuccess)
                    return;
                //SiModel.PreSettleAction = PreSettle;
                Next();
            });
        }

        public virtual Result PreSettle()
        {
            if (CardModel.CardType == CardType.社保卡)
            {
                var result = SiPreSettle();
                if (!result.IsSuccess)
                {
                    return Result.Fail(result.Message);
                }
                var resOpRegPre = SiModel.门诊缴费预结算结果确认结果;
                PaymentModel.Self = decimal.Parse(resOpRegPre.selfFee);
                PaymentModel.Insurance = decimal.Parse(resOpRegPre.insurFee);
                PaymentModel.Total = TotalBillFee;
                PaymentModel.NoPay = PaymentModel.Self == 0;
                PaymentModel.ConfirmAction = Confirm;
            }
            else
            {
                PaymentModel.Self = TotalBillFee;
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = TotalBillFee;
                PaymentModel.NoPay = PaymentModel.Self == 0;
                PaymentModel.ConfirmAction = Confirm;
            }

            PaymentModel.MidList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("总计金额：",PaymentModel.Total.In元(),true),
            };
            return Result.Success();
        }

        public virtual Result SiPreSettle()
        {
            //请求HIS预结算
            //ctx.ChangeText("正在进行门诊缴费预结算，请稍候...");
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = TotalBillFee.ToString("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = TotalBillNo
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费预结算失败", debugInfo: resOpPayPre.msg);
                return Result.Fail(resOpPayPre.msg);
            }
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;

            //医保预结算
            //ctx.ChangeText("正在进行医保预结算，请稍候...");
            string insurFeeInfo = SiModel.门诊缴费预结算结果.insurFeeInfo;
            var result = SiModel.诊间结算
                ? SiService.OpPayClinicPreSettle(insurFeeInfo)
                : SiService.OpPreSettle(insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //HIS预结算确认
            //ctx.ChangeText("正在进行门诊缴费预结算结果确认，请稍候...");
            var reqOpPayPreConfirm = new req门诊缴费预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = TotalBillFee.ToString("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = TotalBillNo
            };
            var resOpPayPreConfirm = DataHandlerEx.门诊缴费预结算结果确认(reqOpPayPreConfirm);
            if (!resOpPayPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpPayPreConfirm.msg);
                return Result.Fail(resOpPayPreConfirm.msg);
            }
            SiModel.门诊缴费预结算结果确认结果 = resOpPayPreConfirm.data;
            return Result.Success();
        }

        protected virtual Result GetSiSettleReq()
        {
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = TotalBillFee.ToString("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = TotalBillNo
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费预结算失败", debugInfo: resOpPayPre.msg);
                return Result.Fail(resOpPayPre.msg);
            }
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;

            //医保预结算
            string insurFeeInfo = SiModel.门诊缴费预结算结果.insurFeeInfo;
            var result = SiModel.诊间结算
                ? SiService.OpPayClinicPreSettle(insurFeeInfo)
                : SiService.OpPreSettle(insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊缴费预结算", "门诊缴费医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //HIS预结算确认
            var reqOpPayPreConfirm = new req门诊缴费预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),

                cash = TotalBillFee.ToString("0"),
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                billNo = TotalBillNo
            };
            var resOpPayPreConfirm = DataHandlerEx.门诊缴费预结算结果确认(reqOpPayPreConfirm);
            if (!resOpPayPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpPayPreConfirm.msg);
                return Result.Fail(resOpPayPreConfirm.msg);
            }
            SiModel.门诊缴费预结算结果确认结果 = resOpPayPreConfirm.data;
            return Result.Success();
        }
        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                //医保结算
                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算，请稍候...");
                    if (SiModel.诊间结算)
                    {
                        var res = GetSiSettleReq();
                        if (!res.IsSuccess)
                            return Result.Fail(res.Message);
                    }
                    string insurFeeInfo = SiModel.门诊缴费预结算结果确认结果?.insurFeeInfo;
                    var result = SiModel.诊间结算 ? SiService.OpPayClinicSettle(insurFeeInfo)
                                                   : SiService.OpSettle(insurFeeInfo);
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "门诊缴费医保结算", "门诊缴费医保结算失败", debugInfo: result.Message);
                        return Result.Fail(result.Message);
                    }
                }

                //HIS结算
                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.当前病人信息;

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = GetPayMethod(PaymentModel.PayMethod),
                    cash = PaymentModel.Self.ToString("0"),
                    accountNo = patientInfo.patientId,
                    billNo = TotalBillNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0"
                };
                if (Startup.TestRefund)
                {
                    BillPayModel.Req缴费结算.cash = (PaymentModel.Self + 1).ToString("0");
                }

                base.FillRechargeRequest(BillPayModel.Req缴费结算);

                if (CardModel.CardType == CardType.社保卡)
                    FillSiRequest(BillPayModel.Req缴费结算);

                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                    return Result.Success();
                }
                if (SiModel.诊间结算 && CardModel.CardType == CardType.社保卡)
                {
                    if (DataHandler.UnKnowErrorCode.Any(p => p == BillPayModel.Res缴费结算?.code))
                    {
                        //打印医保单边账凭条
                        if (PaymentModel.Insurance > 0)
                        {
                            var errorMsg = $"医保消费成功，网关返回未知结果{BillPayModel.Res缴费结算?.code.ToString()}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                            医保单边账凭证(errorMsg);
                        }
                    }
                    else if (BillPayModel.Res缴费结算?.data?.extend != null)
                    {
                        //医保退费
                        var result = SiService.OpPayClinicSettleRefund(BillPayModel.Res缴费结算?.data?.extend);
                        if (!result.IsSuccess)
                        {
                            //医保退费失败处理
                            ShowAlert(false, "医保门诊缴费诊间结算退费", "医保门诊缴费诊间结算退费失败", debugInfo: result.Message);
                        }
                    }
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = BillPayModel.Res缴费结算?.msg
                    });
                    Navigate(A.JF.Print);
                }

                ExtraPaymentModel.Complete = true;
                return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
            }).Result;
        }

        public virtual string GetPayMethod(PayMethod payMethod)
        {
            return payMethod == PayMethod.预缴金 ? "SMK" : payMethod.GetEnumDescription();
        }

        public virtual void FillSiRequest(req缴费结算 req)
        {
            req.extend = SiModel.RetMessage;
            req.preYbinfo = new SiInfo
            {
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                transNo = SiModel.门诊缴费预结算结果确认结果?.transNo
            }.ToJsonString();
        }
        protected virtual void 医保单边账凭证(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"处方单号：{TotalBillNo}\n");
            sb.Append($"医保金额：{PaymentModel?.Insurance.In元()}\n");
            sb.Append($"异常描述：{errorMsg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导医处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            PrintModel.PrintInfo = new PrintInfo
            {
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = queue
            };
            PrintManager.Print();
        }

        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            //var record = BillRecordModel.所选缴费概要;
            var resOpRegPre = SiModel.门诊缴费预结算结果确认结果;

            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号码：{patientInfo.platformId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            if (resOpRegPre != null)
            {
                sb.Append($"个人支付：{resOpRegPre?.selfFee.In元()}\n");
                sb.Append($"医保报销：{resOpRegPre?.insurFee.In元()}\n");
            }
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"发票号：{billPay.receiptNo}\n");
            sb.Append($"发药窗口：{billPay.takeMedWin}\n");
            if (!string.IsNullOrEmpty(billPay.testCode))
            {
                sb.Append($"检验条码：{billPay.testCode}\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb.Clear();
                var image = BarCode128.GetCodeImage(billPay.testCode, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Left,
                    Image = image,
                    Height = image.Height / 1.5f,
                    Width = image.Width / 1.5f
                });
            }
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));

            foreach (var record in BillRecordModel.Res获取缴费概要信息.data)
                if (record?.billItem != null)
                    foreach (var detail in record.billItem)
                        queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));

            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证，\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}