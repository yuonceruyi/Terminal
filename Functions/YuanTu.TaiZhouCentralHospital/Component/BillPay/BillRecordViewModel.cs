using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Service;

namespace YuanTu.TaiZhouCentralHospital.Component.BillPay
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;
            DoCommand(lp =>
            {
                if (CardModel.CardType == CardType.社保卡)
                {
                    //todo 医保预结算

                    lp.ChangeText("正在进行医保预结算,请稍候...");
                    var result = SiPreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    var resOpRegPre = SiModel.医保预结算结果;
                    PaymentModel.Self = decimal.Parse(resOpRegPre.合计现金支付) * 100;
                    PaymentModel.Insurance = decimal.Parse(resOpRegPre.合计报销金额) * 100;
                    PaymentModel.Total = decimal.Parse(resOpRegPre.费用总额) * 100;
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;

                }
                else
                {
                    PaymentModel.Self = decimal.Parse(recordInfo.billFee);
                    PaymentModel.Insurance = decimal.Parse("0");
                    PaymentModel.Total = decimal.Parse(recordInfo.billFee);
                    PaymentModel.NoPay = false;
                    PaymentModel.ConfirmAction = Confirm;
                }

                PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", recordInfo.billDate?.SafeToSplit(' ')?[0] ?? recordInfo.billDate),
                new PayInfoItem("时间：", recordInfo.billDate?.SafeToSplit(' ')?[1] ?? null),
                new PayInfoItem("科室：", recordInfo.deptName),
                new PayInfoItem("医生：", recordInfo.doctName)
            };

                PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
                Next();
            });
           
        }

        /// <summary>
        ///     医保预结算
        /// </summary>
        /// <returns></returns>
        protected virtual Result SiPreSettle()
        {
            //todo HIS提供医保预结算入参
            var reqOpPayPre = new req缴费预结算
            {
                patientId = PatientModel.当前病人信息?.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                billNo = BillRecordModel.所选缴费概要?.billNo,
                extend = SiModel.SiPatientInfo
            };
            var resOpPayPre = DataHandlerEx.缴费预结算(reqOpPayPre);
            if (!resOpPayPre.success)
                return Result.Fail($"门诊缴费预结算失败:{resOpPayPre.msg}");
            SiModel.门诊缴费预结算结果 = resOpPayPre.data;

            //todo 医保预结算
            var result = SiService.OpPreSettle(SiModel.门诊缴费预结算结果.extend);
            if (!result.IsSuccess)
                return Result.Fail($"门诊缴费医保预结算失败:{result.Message}");
            return Result.Success();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                if (CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算,请稍候...");
                    //todo 医保结算
                    var result = SiService.OpSettle(SiModel.门诊缴费预结算结果.insurFeeInfo);
                    if (!result.IsSuccess)
                        return Result.Fail($"门诊缴费医保结算失败:{result.Message}");

                    //lp.ChangeText("正在进行医保交易确认,请稍候...");
                    //todo 医保交易确认
                    //var req = new Req交易确认
                    //{
                    //    Ic卡信息 = SiModel.医保个人基本信息?.卡号,
                    //    交易类型 = "30",//门诊医保结算
                    //    医保交易流水号 = SiModel.医保结算流水号,
                    //    HIS事务结果 = "0",
                    //};
                    //result = SiService.TradeConfirm(req);
                    //if (!result.IsSuccess)
                    //{
                    //    ShowAlert(false, "医保交易确认", "医保交易确认失败", debugInfo: result.Message);
                    //    return Result.Fail($"门诊缴费医保交易确认失败:{result.Message}");
                    //}
                }

                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var record = BillRecordModel.所选缴费概要;

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    patientName = patientInfo.name,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Self.ToString(),
                    accountNo = patientInfo.patientId,
                    billNo = record.billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                };

                FillRechargeRequest(BillPayModel.Req缴费结算);
                FillSiInfo(BillPayModel.Req缴费结算);

                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        PrintablesList = new List<Queue<IPrintable>>
                        {
                            BillPayPrintables()
                        },
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                    return Result.Success();
                }

                //if (CardModel.CardType == CardType.社保卡)
                //    if (DataHandler.UnKnowErrorCode.Any(p => p == BillPayModel.Res缴费结算?.code))
                //    {
                //        //todo 打印医保单边账凭条
                //        if (PaymentModel.Insurance > 0)
                //        {
                //            var errorMsg = $"医保消费成功，网关返回未知结果{BillPayModel.Res缴费结算?.code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                //            打印医保单边账凭证(errorMsg);
                //        }
                //    }
                //    else
                //    {
                //        if (PaymentModel.Insurance > 0)
                //        {
                //            //todo 医保退费
                //            var req = new Req医保退费
                //            {
                //                Ic卡信息 = SiModel.医保个人基本信息?.卡号,
                //                要作废的结算流水号 = SiModel.医保结算流水号,
                //                经办人 = FrameworkConst.OperatorId,
                //                终端机编号 = "0"
                //            };
                //            var result = SiService.OpRefund(req);
                //            if (!result.IsSuccess)
                //            {
                //                //todo 医保退费失败处理
                //                ShowAlert(false, "医保门诊缴费诊间结算退费", "医保门诊缴费诊间结算退费失败", debugInfo: result.Message);
                //                var errorMsg = $"医保消费成功，医院业务处理失败，医保退费失败！";
                //                打印医保单边账凭证(errorMsg);
                //            }
                //        }
                //    }

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

        protected virtual void FillSiInfo(req缴费结算 req)
        {
            if (CardModel.CardType == CardType.社保卡)
            {

                req.patientTypeId = PaymentModel.Insurance.ToString();
                req.preYbinfo = SiModel.医保预结算结果字符串;
                req.extend = SiModel.医保结算结果字符串;
                req.ybCardNo = SiModel.SiPatientInfo;
            }
        }

        protected virtual void 打印医保单边账凭证(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"门诊号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"处方单号：{BillRecordModel.所选缴费概要?.billNo}\n");
            sb.Append($"医保金额：{PaymentModel?.Insurance.In元()}\n");
            sb.Append($"医保结算流水号:{SiModel.医保结算流水号}\n");
            sb.Append($"异常描述：{errorMsg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss)}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导医处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}