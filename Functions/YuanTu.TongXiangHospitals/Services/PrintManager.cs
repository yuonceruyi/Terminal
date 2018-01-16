using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;

namespace YuanTu.TongXiangHospitals.Services
{
    public class PrintManager:YuanTu.Core.Services.PrintService.PrintManager
    {
        [Dependency]
        public IBarQrCodeGenerator BarQrCodeGenerator { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override Queue<IPrintable> NewQueue(string title)
        {
            var queue = new Queue<IPrintable>();
            var platformId = PatientModel?.Res病人信息查询?.data?[PatientModel.PatientInfoIndex]?.platformId;
            if(!string.IsNullOrEmpty(platformId))
                queue.Enqueue(new PrintItemImage()
                {
                    Align = ImageAlign.Center,
                    Height = 40,
                    Width = 120,
                    Image = BarQrCodeGenerator.BarcodeGenerate(platformId)
                });

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
            return queue;
        }
    }
}
