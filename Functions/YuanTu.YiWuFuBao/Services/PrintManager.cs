using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.YiWuArea.Services;

namespace YuanTu.YiWuFuBao.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            var res = ServiceLocator.Current.GetInstance<IResourceEngine>();
            var imgPath = res.GetResourceFullPath("打印_头");
            if (File.Exists(imgPath))
            {
                var img = new Bitmap(imgPath);
                queue.Enqueue(new PrintItemImage()
                {
                    Image = img,
                    Align = ImageAlign.Center,
                    Height = img.Height/ 3.7f,
                    Width = img.Width/3.7f
                });
            }

            queue.Enqueue(new PrintItemText
            { Text = FrameworkConst.HospitalName/*医院名称*/, StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text = title + "凭条", StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2 });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            return queue;
        }

        public override Result Print()
        {
            var ad = AdvertisePrintables();
            if (PrintModel.PrintInfo.PrintablesList != null)
            {
                foreach (var result in PrintModel.PrintInfo.PrintablesList.Select(printables =>
                {
                    var cp = new Queue<IPrintable>(printables);
                    if (ad?.Any() ?? false)
                    {
                        foreach (var printable in ad)
                        {
                            cp.Enqueue(printable);
                        }
                    }
                    return Start(PrintModel.PrintInfo.PrinterName, cp);
                }).Where(result => !result.IsSuccess))
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
                return Start(PrintModel.PrintInfo.PrinterName, cp);

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
            return Start(printerName, cp);
        }


        private Result Start(string printName, Queue<IPrintable>queue)
        {

            //if (FrameworkConst.DeviceType.Contains("BG"))
            //{
            //    var port = ConfigurationManager.GetValueInt("Printer:ReceiptOrder:Port", 4);
            //    var baud = ConfigurationManager.GetValueInt("Printer:ReceiptOrder:Baud", 9600);
            //    return new OrderPrintHelper(port, baud, queue).Print();
            //}
            return new CurrentPrintHelper(printName, queue).Print();
        }
    }
}
