using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;

namespace YuanTu.ShenZhen.PekingUniversityHospital.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            queue.Enqueue(new PrintItemText
            {
                Text = FrameworkConst.HospitalName /*医院名称*/,
                StringFormat = PrintConfig.Center,
                Font = PrintConfig.HeaderFont
            });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text = title, StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2 });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            return queue;
        }

        public override Result Print()
        {
            var ad = AdvertisePrintables();
            if (PrintModel.PrintInfo.PrintablesList != null)
            {
                foreach (var result in PrintModel.PrintInfo.PrintablesList.
                    Select(printables =>
                    {
                        var cp = new Queue<IPrintable>(printables);
                        if (ad?.Any() ?? false)
                        {
                            foreach (var printable in ad)
                            {
                                cp.Enqueue(printable);
                            }
                        }
                        return
                            new CurrentPrintHelper(PrintModel.PrintInfo.PrinterName, cp).Print();
                    }).
                    Where(result => !result.IsSuccess))
                {
                    return result;
                }
            }
            if (PrintModel.PrintInfo.Printables != null)
            {
                var cp = new Queue<IPrintable>(PrintModel.PrintInfo.Printables);
                if (ad?.Any() ?? false)
                {
                    foreach (var printable in ad)
                    {
                        cp.Enqueue(printable);
                    }
                }
                return new CurrentPrintHelper(PrintModel.PrintInfo.PrinterName, cp).Print();

            }
            return Result.Success();
        }

        public override Result QuickPrint(string printerName, Queue<IPrintable> printables)
        {
            var ad = AdvertisePrintables();
            var cp = new Queue<IPrintable>(printables);
            if (ad?.Any() ?? false)
            {
                foreach (var printable in ad)
                {
                    cp.Enqueue(printable);
                }
            }
            return new CurrentPrintHelper(printerName, cp).Print();
        }
    }
}
