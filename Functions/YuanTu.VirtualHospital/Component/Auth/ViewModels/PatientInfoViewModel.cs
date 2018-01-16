using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Models.Print;
using YuanTu.Devices.CardReader;

namespace YuanTu.VirtualHospital.Component.Auth.ViewModels
{
    public class PatientInfoViewModel : Default.Component.Auth.ViewModels.PatientInfoViewModel
    {
        public PatientInfoViewModel(IRFCardDispenser[] rfCardDispenser) : base(rfCardDispenser)
        {
            _rfCardDispenser = rfCardDispenser?.FirstOrDefault(p => p.DeviceId == "KC100_RF");
        }

        protected override void PrintCard()
        {
            if (FrameworkConst.VirtualHardWare)
                return;
            var printText = new List<ZbrPrintTextItem>
            {

                new ZbrPrintTextItem()
                {
                    X = 150,
                    Y = 45,
                    Text = Name,
                    FontSize = 12,
                    Font = "宋体"
                },
                new ZbrPrintTextItem()
                {
                    X = 530,
                    Y = 45,
                    FontSize = 12,
                    Font = "宋体",
                    Text =   CreateModel.Res病人建档发卡.data.patientCard
                }
            };

            var printCode = new List<ZbrPrintCodeItem>();

            _rfCardDispenser.PrintContent(printText, printCode);
        }


        protected override Queue<IPrintable> CreatePrintables()
        {
            var queue = PrintManager.NewQueue("自助发卡");

            var sb = new StringBuilder();
            sb.Append($"状态：办卡成功\n");
            sb.Append($"发卡单位：{FrameworkConst.HospitalName}\n");
            sb.Append($"姓名：{IdCardModel.Name}\n");
            sb.Append($"就诊卡号：{CreateModel.Res病人建档发卡.data.patientCard}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}