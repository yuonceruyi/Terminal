using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.Core.Services.PrintService
{
    public class PrintHelperEx
    {
        private readonly string printerName;

        public PrintHelperEx(string printerName)
        {
            this.printerName = printerName;
        }

        public PimpedPaginator PimpedPaginator { get; private set; }

        public Result Print(
            FlowDocument document,
            string fileName,
            double headerHeight = 0,
            double footerHeight = 0,
            PimpedPaginator.DrawHeaderFooter drawHeader = null,
            PimpedPaginator.DrawHeaderFooter drawFooter = null,
            bool needCopy = true,
            bool landscape = false)
        {
            Logger.Printer.Info($"Printing on {printerName} file:{fileName}");
            var printServer = new PrintServer();
            var list = printServer.GetPrintQueues();
            PrintQueue printQueue = null;
            foreach (var queue in list)
            {
                if (!queue.Name.Contains(printerName))
                    continue;
                printQueue = queue;
            }
            if (printQueue == null)
            {
                Logger.Printer.Error($"Printer {printerName} not found!");
                return Result.Fail($"Printer {printerName} not found!");
            }
            var dlg = new PrintDialog
            {
                PrintQueue = printQueue,
                PrintTicket = printQueue.UserPrintTicket
            };
            dlg.PrintTicket.PageOrientation = landscape ? PageOrientation.Landscape : PageOrientation.Portrait;
            var size = dlg.PrintTicket.PageMediaSize;
            var width = landscape ? size.Height.Value : size.Width.Value;
            var height = landscape ? size.Width.Value : size.Height.Value;

            var definition = new PimpedPaginator.Definition
            {
                Margins = new Thickness(32),
                PageSize = new Size(width, height),
                Header = drawHeader,
                Footer = drawFooter,
                HeaderHeight = headerHeight,
                FooterHeight = footerHeight
            };
            PimpedPaginator = new PimpedPaginator(document, definition, needCopy);

            PimpedPaginator.ComputePageCount();

            try
            {
                Logger.Printer.Info($"Printing to {printerName}");
                dlg.PrintDocument(PimpedPaginator, fileName);
                Logger.Printer.Info($"Printing to {printerName} done");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail("打印错误", ex);
            }
        }
    }
}