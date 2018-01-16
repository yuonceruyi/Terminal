using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Services.LocalResourceEngine;

namespace YuanTu.ChongQingArea.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();

            var imageLogo = new ResourceEngine().GetResourceFullPath("1103s");

            FileStream fs = new FileStream(imageLogo, FileMode.Open, FileAccess.Read);
            var image = new System.Drawing.Bitmap(fs, false);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 8f,
                Width = image.Width / 8f
            });
            //queue.Enqueue(new PrintItemText
            //{
            //    Text = FrameworkConst.HospitalName /*医院名称*/,
            //    StringFormat = PrintConfig.Center,
            //    Font = PrintConfig.HeaderFont
            //});
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text = title + "凭条", StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2 });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            fs.Close();
            return queue;
        }
       
    }
}
