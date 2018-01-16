using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;

namespace YuanTu.HuNanHangTianHospital.Component.PrintAgain.ViewModels
{
    public class PayCostRecordViewModel:YuanTu.Default.Component.PrintAgain.ViewModels.PayCostRecordViewModel
    {
        protected override Queue<IPrintable> SettlementRecordPrintables()
        {
            var queue = PrintManager.NewQueue("缴费补打");
            var record = SettlementRecordModel.所选已缴费概要;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            var sb = new StringBuilder();
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"金额总计：{record.billFee.In元()}\n");
            sb.Append($"支付方式：{record.tradeMode}\n");
            sb.Append($"收据号：{record.billNo}\n");
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));
            if (record?.billItem != null)
            {
                foreach (var detail in record.billItem)
                {
                    queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));
                }
            }
            sb.Append($"交易时间：{record.tradeTime}\n");
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
