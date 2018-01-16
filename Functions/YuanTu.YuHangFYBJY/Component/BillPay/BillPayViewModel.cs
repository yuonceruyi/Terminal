using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.YuHangFYBJY.Component.BillPay.Models;
using YuanTu.YuHangFYBJY.NativeService;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.Component.BillPay
{
    public class BillRecordViewModel :Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IPrePayModel PrePayModel { get; set; }

        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;

            //todo 缴费预结算
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行预结算,请稍候...");
                var req = new PerCheckoutRequest
                {
                    CheckoutType = recordInfo.extend,
                    CardNo = CardModel.CardNo
                };

                var result = LianZhongHisService.GetHospitalPerCheckoutInfo(req);

                if (!result.IsSuccess)
                    return Result.Fail(result.Message);
                PrePayModel.PerCheckout = result.Value;
                return Result.Success();
            }).ContinueWith(ctx =>
            {
                if (!ctx.Result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"缴费预结算失败:{ctx.Result.Message}");
                    return;
                }
                var prePayOut = PrePayModel.PerCheckout;
                var pmodel = PaymentModel as YuanTu.YuHangFYBJY.Component.Models.PaymentModel;
                PaymentModel.Self = decimal.Parse(prePayOut.ActualPay ?? "0") * 100;
                PaymentModel.Insurance = decimal.Parse(prePayOut.HealthCarePay ?? "0") * 100;
                PaymentModel.Total = decimal.Parse(prePayOut.TotalPay ?? "0") * 100;
                pmodel.CitizenBlance= decimal.Parse(prePayOut.CitizenCardBalance ?? "0") * 100;

                PaymentModel.Self = PaymentModel.Self < 0m ? 0m : PaymentModel.Self;
                PaymentModel.NoPay = PaymentModel.Self <= 0; 
                PaymentModel.ConfirmAction = Confirm;

                var dateTime = recordInfo.billDate?.SafeToSplit(' ', 2);
                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", dateTime?[0] ?? recordInfo.billDate),
                    new PayInfoItem("时间：", dateTime?[1] ?? null),
                    new PayInfoItem("科室：", recordInfo.deptName),
                    new PayInfoItem("医生：", recordInfo.doctName)
                };
                if (pmodel.CitizenBlance>0)
                {
                    PaymentModel.LeftList.Add(new PayInfoItem("智慧医疗余额：", pmodel.CitizenBlance.In元()));
                }

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付总额：", PaymentModel.Total.In元(), true)
                };
                Next();
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行结算，请稍候...");
                var req = new CheckoutRequest
                {
                    CheckoutType = BillRecordModel.所选缴费概要.extend,
                    CardNo = CardModel.CardNo,
                    PayFlag = PayMedhodFlag.院内账户,
                };
                switch (PaymentModel.PayMethod)
                {
                    case PayMethod.未知:
                        break;

                    case PayMethod.现金:
                        break;

                    case PayMethod.银联:
                        req.PayFlag = PayMedhodFlag.银联;
                        break;

                    case PayMethod.预缴金:
                        req.PayFlag = PayMedhodFlag.院内账户;
                        break;

                    case PayMethod.社保:
                        break;

                    case PayMethod.支付宝:
                        req.PayFlag = PayMedhodFlag.支付宝扫码;
                        req.AlipayAmount = (PaymentModel.Self/100).ToString();
                        req.AlipayTradeNo = (ExtraPaymentModel.PaymentResult as 订单状态)?.outTradeNo;
                        break;

                    case PayMethod.微信支付:
                        break;

                    case PayMethod.苹果支付:
                        break;

                    case PayMethod.智慧医疗:
                        req.PayFlag = PayMedhodFlag.市民卡;
                        break;

                    default:
                        req.PayFlag = PayMedhodFlag.院内账户;
                        break;
                }
                var res = LianZhongHisService.ExcuteHospitalCheckout(req);
                PrePayModel.Checkout = res.Value;
                if (res.IsSuccess)
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

                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = res.Message
                    });
                    Navigate(A.JF.Print);
                }

                ExtraPaymentModel.Complete = true;
                return Result.Fail(res.Message);
            }).Result;
        }
        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var patientInfo = PatientModel.当前病人信息;
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
          //  sb.Append($"交易类型：自助缴费\n");
            
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"医保金额：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
           
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemRatioText("名称", "数量", "金额"));
            if (record?.billItem != null)
            {
                foreach (var detail in record.billItem)
                    queue.Enqueue(new PrintItemRatioText(detail.itemName, $"{detail.itemQty} {detail.itemUnits}", detail.billFee.InRMB()));
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            sb.Append($"如需打印发票请到门诊大厅发票打印窗口办理。\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}