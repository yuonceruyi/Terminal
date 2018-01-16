using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;

namespace YuanTu.WeiHaiZXYY.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            queue.Enqueue(new PrintItemText
            { Text = FrameworkConst.HospitalName/*医院名称*/, StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text = title, StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2 });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            return queue;
        }
    }
}
