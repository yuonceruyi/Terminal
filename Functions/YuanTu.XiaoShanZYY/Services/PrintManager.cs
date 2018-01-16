using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.XiaoShanZYY.Component.Auth.Models;

namespace YuanTu.XiaoShanZYY.Services
{
    public class PrintManager : YuanTu.Core.Services.PrintService.PrintManager
    {
        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IAuthModel Auth { get; set; }

        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            var list = FrameworkConst.HospitalName.Split('\n'); //医院名称

            queue.Enqueue(new PrintItemText
            {
                Text = list[0],
                StringFormat = PrintConfig.Center,
                Font = PrintConfig.HeaderFont
            });
            for (int i = 1; i < list.Length; i++)
                queue.Enqueue(new PrintItemText
                {
                    Text = list[i],
                    StringFormat = PrintConfig.Center,
                    Font = PrintConfig.DefaultFont
                });
            if (!string.IsNullOrWhiteSpace(title))
            {
                queue.Enqueue(new PrintItemGap { Gap = 5f });
                queue.Enqueue(new PrintItemText
                { Text = title + "凭条", StringFormat = PrintConfig.Center, Font = PrintConfig.HeaderFont2 });
            }
            queue.Enqueue(new PrintItemGap { Gap = 10f });
            var builder = new StringBuilder();
            builder.AppendLine("【自助机号】" + FrameworkConst.OperatorId);
            builder.AppendLine("【就诊卡号】" + Auth.人员信息.就诊卡号);
            builder.AppendLine("【 姓  名 】" + Auth.人员信息.病人姓名);
            builder.AppendLine("【 日  期 】" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            queue.Enqueue(new PrintItemText { Text = builder.ToString() });
            return queue;
        }
    }
}
