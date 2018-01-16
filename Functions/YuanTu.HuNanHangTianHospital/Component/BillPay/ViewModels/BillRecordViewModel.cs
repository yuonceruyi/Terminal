using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;
using YuanTu.HuNanHangTianHospital.Common;

namespace YuanTu.HuNanHangTianHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        public decimal TotalBillFee { get; set; }
        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var record = BillRecordModel.所选缴费概要;

                BillPayModel.Req缴费结算 = new req缴费结算
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    cash = PaymentModel.Total.ToString(CultureInfo.InvariantCulture),
                    accountNo = patientInfo.patientId,
                    billNo = BillRecordModel.Res获取缴费概要信息.data.FirstOrDefault().billNo,
                    allSelf = PaymentModel.Insurance == 0 ? "1" : "0"
                };

                FillRechargeRequest(BillPayModel.Req缴费结算);

                BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
                if (BillPayModel.Res缴费结算?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    KY.MoveOutCard();
                    Navigate(A.JF.Print);
                    return Result.Success();
                }
                ShowAlert(false, "缴费结算", "缴费结算失败");
                return Result.Fail(BillPayModel.Res缴费结算?.code ?? -100, BillPayModel.Res缴费结算?.msg);
            }).Result;
        }

        protected override void Do()
        {
            var billPrePayModel = GetInstance<IBillPrePayModel>();
            TotalBillFee = decimal.Parse(billPrePayModel.Res缴费预结算.data.billFee);
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);
            PaymentModel.Self = TotalBillFee;
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = TotalBillFee;
            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.MidList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付【折后价格】：",TotalBillFee.In元()),
                new PayInfoItem("支付金额：",TotalBillFee.In元(),true),
            };
            Next();
        }

        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            var record = BillRecordModel.所选缴费概要;

            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{TotalBillFee.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"收据号：{billPay.receiptNo}\n");
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
            if (BillPayModel.Res缴费结算?.data?.billItem != null)
            {
                foreach (var detail in BillPayModel.Res缴费结算.data.billItem)
                {
                    queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));
                }
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            sb.Append($"打发票、退费 请到人工窗口办理\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

    }
    public class PrintItemTriText : PrintItemText
    {
        public string Text2;
        public string Text3;
        public float Text3X = PrintConfig.Default3X;

        public PrintItemTriText()
        {
        }
        public PrintItemTriText(string t1, string t2, string t3)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
        }

        public PrintItemTriText(string t1, string t2, string t3, float t4)
        {
            Text = t1;
            Text2 = t2;
            Text3 = t3;
            Text3X = t4;
        }

        public override float GetHeight(Graphics g, float w)
        {
            return g.MeasureString(Text, Font, (int)w).Height;
        }

        public override float Print(Graphics g, float Y, float w)
        {
            var h = GetHeight(g, w);
            g.DrawString(Text, Font, Brushes.Black, new RectangleF(10, Y, 170, h));
            g.DrawString(Text2, Font, Brushes.Black, new RectangleF(180, Y, 40, h));
            g.DrawString(Text3, Font, Brushes.Black, new RectangleF(220, Y, 70, h));
            return h;
        }
    }
}
