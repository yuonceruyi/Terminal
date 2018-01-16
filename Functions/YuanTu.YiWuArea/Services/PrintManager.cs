using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.YiWuArea.Services
{
    public class PrintManager/*: IPrintManager*/
    {
        [Dependency]
        public IPrintModel PrintModel { get; set; }

        public string ServiceName => "指令打印";

        public Result Print()
        {
            var cfgmanager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var port = cfgmanager.GetValueInt("Printer:ReceiptOrder:Port",0);
            var baud = cfgmanager.GetValueInt("Printer:ReceiptOrder:Baud",9600);
            if (PrintModel.PrintInfo.PrintablesList != null)
            {
                foreach (var result in PrintModel.PrintInfo.PrintablesList.
                    Select(printables => new OrderPrintHelper(port,baud, printables).Print()).
                    Where(result => !result.IsSuccess))
                {
                    return result;
                }
            }
            if (PrintModel.PrintInfo.Printables != null)
                return new OrderPrintHelper(port, baud, PrintModel.PrintInfo.Printables).Print();
            return Result.Success();
        }

        public Result QuickPrint(string printerName, Queue<IPrintable> printables)
        {
            var cfgmanager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var port = cfgmanager.GetValueInt("Printer:ReceiptOrder:Port", 0);
            var baud = cfgmanager.GetValueInt("Printer:ReceiptOrder:Baud", 9600);
            return new OrderPrintHelper(port,baud, printables).Print();
        }

        public Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
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
    }
}
