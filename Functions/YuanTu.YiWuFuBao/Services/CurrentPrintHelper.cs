using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;

namespace YuanTu.YiWuFuBao.Services
{
    public class CurrentPrintHelper
    {
        private readonly string _printerName;
        private readonly Queue<IPrintable> _printQueue;

        public CurrentPrintHelper(string printer, Queue<IPrintable> queue)
        {
            _printerName = printer;
            _printQueue = queue;
        }

        public Result Print()
        {
            try
            {
                var docToPrint = new PrintDocument();
                docToPrint.PrintPage += docToPrint_PrintPage;
                docToPrint.DefaultPageSettings.PaperSize = GetPageSize(docToPrint.DefaultPageSettings.PaperSize, 70);

                //诊间屏
                if (FrameworkConst.Strategies[0] == DeviceType.Clinic)
                    docToPrint.DefaultPageSettings.PaperSize = GetPageSize(docToPrint.DefaultPageSettings.PaperSize, 46.5);
                if (FrameworkConst.DeviceType.Contains("BG"))
                {
                    docToPrint.DefaultPageSettings.PaperSize = GetPageSize(docToPrint.DefaultPageSettings.PaperSize, 46.5);
                }

                docToPrint.PrinterSettings.PrinterName = _printerName;
                docToPrint.PrintController = new StandardPrintController();
                var content = new StringBuilder();
                foreach (var printable in _printQueue)
                {
                    content.AppendLine(GetPrintableContent(printable));
                }
                Logger.Printer.Info($"[{_printerName}]开始打印:\r\n{content}");
                docToPrint.Print();
                Logger.Printer.Info($"[{_printerName}]打印完成");
                return Result.Success();
            }
            catch (Exception ex)
            {
                // TODO
                ReportService.凭条打印机离线(ex.Message, ErrorSolution.凭条打印机离线);
                return Result.Fail("打印失败", ex);
            }
        }

        private string GetPrintableContent(IPrintable printable)
        {
            return printable.GetLogText();
            //var type = printable.GetType();
            //if (type == typeof(PrintItemImage))
            //{
            //    return "[图片]";
            //}
            //if (type == typeof(PrintItemTriText))
            //{
            //    var obj = printable as PrintItemTriText;
            //    return $"{obj.Text} {obj.Text2} {obj.Text3}";
            //}
            //if (type == typeof(PrintItemText))
            //{
            //    var obj = printable as PrintItemText;
            //    return obj.Text;
            //}
            //if (type == typeof(PrintItemGap))
            //{
            //    return "\n";
            //}
            //return "[Error]" + printable.GetType();
        }

        public PaperSize GetPageSize(PaperSize p, double w = 46.5)
        {
            //毫米与英寸的转换  1mm= 0.039英寸
            var width = (int)(w * 0.04 * 100);
            var height = p.Height;
            var ps = new PaperSize("Custom", width, height);
            return ps;
        }

        private void docToPrint_PrintPage(object sender, PrintPageEventArgs e)
        {
            Logger.Printer.Info(_printerName + " 页面 " + _printQueue.Count);
            var top = 0f;
            int count = 0;
            while (_printQueue.Count > 0)
            {
                if ((top + _printQueue.Peek().GetHeight(e.Graphics, e.PageBounds.Width) > e.PageBounds.Height) && count > 0)
                    break;
                top += _printQueue.Dequeue().Print(e.Graphics, top, e.PageBounds.Width);
                count++;
            }
            e.HasMorePages = _printQueue.Count > 0;

            Logger.Printer.Info(_printerName + " 页面打印结束 " + _printQueue.Count);
        }
    }
}
