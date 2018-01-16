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

namespace YuanTu.ShengZhouHospital.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            queue.Enqueue(new PrintItemText
            { Text = "嵊州市人民医院（浙大一院嵊州分院）"/*医院名称*/, StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 11, FontStyle.Bold) });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text ="自助就医服务-"+title, StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 10, FontStyle.Bold) });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            return queue;
        }
    }
}
