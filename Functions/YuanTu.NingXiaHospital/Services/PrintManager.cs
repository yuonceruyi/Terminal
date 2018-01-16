using System.Collections.Generic;
using System.Drawing;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;

namespace YuanTu.NingXiaHospital.Services
{
    public class PrintManager : Core.Services.PrintService.PrintManager
    {
        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            queue.Enqueue(new PrintItemText
            {
                Text = FrameworkConst.HospitalName,
                StringFormat = PrintConfig.Center,
                Font = new Font("微软雅黑", 11, FontStyle.Bold)
            });
            queue.Enqueue(new PrintItemGap {Gap = 10f});
            return queue;
        }
    }
}