using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.PrintAgain;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;
using YuanTu.ShengZhouHospital.Component.PrintAgain.Common;

namespace YuanTu.ShengZhouHospital.Component.PrintAgain.ViewModels
{
    public class DateTimeViewModel : YuanTu.Default.Component.PrintAgain.ViewModels.DateTimeViewModel
    {
        protected override void Confirm()
        {
            DoCommand(lp =>
            {
                var type = string.Empty;
                switch (PrintAgainModel.PrintAgainType)
                {
                    case PrintAgainTypeEnum.取号补打:
                        type = "2";
                        break;
                    case PrintAgainTypeEnum.挂号补打:
                        type = "1";
                        break;
                    case PrintAgainTypeEnum.缴费补打:
                        type = "3";
                        break;
                }
                Test();
                var req = new req凭条列表查询
                {
                    patientId = PatientModel.当前病人信息.patientId,
                    cardNo = PatientModel.当前病人信息.cardNo,
                    type = type,
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                };
                var res = DataHandlerEx.凭条列表查询(req);
                if (!res.success || res?.data?.Count <= 0)
                {
                    ShowAlert(false, "凭条查询", "未查询到凭条信息");
                    return;
                }
                PrintAgainModel.Slips = res?.data;
                Next();
            });
        }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        private void Test()
        {
            var queue = PrintManager.NewQueue("(挂号凭条)");
            var sb = new StringBuilder();
            sb.Append($"--------------------------------\n");
            sb.Append("【当日有效，隔日作废1】\n");
            sb.Append("【当日有效，隔日作废2】\n");
            sb.Append("【当日有效，隔日作废3】\n");
            sb.Append("【当日有效，隔日作废4】\n");

            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 9, FontStyle.Regular) });
            sb.Clear();
            sb.Append("【当日有效，隔日作废5】\n");
            sb.Append("【当日有效，隔日作废6】\n");
            sb.Append("【当日有效，隔日作废7】\n");
            sb.Append("【当日有效，隔日作废8】\n");
            sb.Append("【当日有效，隔日作废9】\n");
            sb.Append("【当日有效，隔日作废10】\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("宋体", 10, FontStyle.Regular) });
            sb.Clear();

            var res = SilpHandler.Serialize(queue);

            var re = new res凭条列表查询
            {
                success = true,
                code = 0,
                data = new List<凭条记录>
                    {
                        new 凭条记录
                        {
                            id="22",
                            cardNo="123123",
                            patientId = "312312",
                            type="1",
                            gmtCreate = "2018-01-12 12:12",
                            content = res.Value
                        }
                    }
            };
            var aa = re.ToJsonString();
           
        ;

        }
        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };
    }
}
