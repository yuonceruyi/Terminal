using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.Printer;
using IPrintable = YuanTu.Consts.Models.Print.IPrintable;

namespace YuanTu.YiWuArea.Services
{
    public class OrderPrintHelper
    {
        private readonly Queue<IPrintable> _queue;
        private readonly int _port;
        private readonly int _baud;
        public OrderPrintHelper(int port,int baud,Queue<IPrintable> queue)
        {
            _port = port;
            _baud = baud;
            _queue = queue;
        }

        public Result  Print()
        {
            Logger.Printer.Info($"[串口号:{_port}]开始串口打印");
            var print = new SerialPrinter();
            var ctx = print.GetContext();
            ctx[PrintableContext.SerialPort] = "COM"+_port;
            ctx[PrintableContext.BaudRate] =_baud;
            var apt = print.Connect(ctx);
            if (!apt.Success)
            {
                return Result.Fail(apt.Message);
            }
            print.Reset();
            print.SetFontSize();
            print.SetAlign(0);
            var content = new StringBuilder();
            foreach (var printable in _queue)
            {
                content.AppendLine(GetPrintableContent(printable));
                if (printable is PrintItemGap)
                {
                    print.FeedLine(1);
                }
                else if (printable is PrintItemTriText)
                {
                    print.SetFontSize();
                    print.SetAlign(0);
                    //80mm==>24个字
                    var linewords = 24;
                    var pt = printable as PrintItemTriText;
                    //4 2 2
                    print.Text($"{pt.Text.TrimWithEncoding(linewords / (4 + 2 + 2) * 4, Encoding.Unicode)}{pt.Text2.TrimWithEncoding(linewords / (4 + 2 + 2) * 2, Encoding.Unicode)}{pt.Text3.TrimWithEncoding(linewords / (4 + 2 + 2) * 2, Encoding.Unicode)}").FeedLine();

                }
                else if (printable is PrintItemText)
                {
                   
                    var pt = printable as PrintItemText;
                    var bt = (byte) (pt.Font.Size >= 14 ? 1 : 0);
                    print.SetFontSize(bt,bt);
                    print.SetAlign(pt.StringFormat.Alignment==StringAlignment.Center?1:0);
                    print.Text(pt.Text).FeedLine();

                }else if (printable is PrintItemImage)
                {
                    print.Bitmap((Bitmap)(printable as PrintItemImage).Image);
                }
            }
            Logger.Printer.Info($"[打印端口 COM{_port}]开始打印:\r\n{content}");
            print.FeedLine(10);
            print.CutPaper(-1);

            print.Print();
            print.Disconnect();
            return Result.Success();
        }

        private string GetPrintableContent(IPrintable printable)
        {
            var type = printable.GetType();
            if (type == typeof(PrintItemImage))
            {
                return "[图片]";
            }
            if (type == typeof(PrintItemTriText))
            {
                var obj = printable as PrintItemTriText;
                return $"{obj.Text} {obj.Text2} {obj.Text3}";
            }
            if (type == typeof(PrintItemText))
            {
                var obj = printable as PrintItemText;
                return obj.Text;
            }
            if (type == typeof(PrintItemGap))
            {
                return "\n";
            }
            return "[Error]" + printable.GetType();
        }
    }
}
