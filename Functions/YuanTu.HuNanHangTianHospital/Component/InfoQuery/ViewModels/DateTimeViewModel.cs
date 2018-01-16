using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;

namespace YuanTu.HuNanHangTianHospital.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        protected override void Confirm()
        {
            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.已缴费明细:
                    if (DateTimeStart > DateTimeEnd)
                    {
                        ShowAlert(false, "已缴费信息查询", "开始时间不能晚于结束时间！");
                        return;
                    }
                    DoCommand(lp =>
                    {
                        lp.ChangeText("正在查询已缴费信息，请稍候...");
                        PayCostRecordModel.Req获取已结算记录 = new req获取已结算记录
                        {
                            patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                            cardNo = CardModel.CardNo,
                            cardType = ((int)CardModel.CardType).ToString(),
                            beginDate = DateTimeStart.ToString("yyyy-MM-dd"),
                            startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                            endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                            extend="2"
                        };
                        PayCostRecordModel.Res获取已结算记录 = DataHandlerEx.获取已结算记录(PayCostRecordModel.Req获取已结算记录);
                        if (PayCostRecordModel.Res获取已结算记录?.success ?? false)
                        {
                            if (PayCostRecordModel.Res获取已结算记录?.data?.Count > 0)
                            {
                                ChangeNavigationContent(
                                    $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                                Next();
                                return;
                            }
                            ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息(列表为空)");
                            return;
                        }
                        ShowAlert(false, "已缴费信息查询", "没有获得已缴费信息", debugInfo: PayCostRecordModel.Res获取已结算记录?.msg);
                    });
                    break;

                case InfoQueryTypeEnum.药品查询:
                    break;

                case InfoQueryTypeEnum.项目查询:
                    break;

                case InfoQueryTypeEnum.检验结果:
                    DiagReportQuery();
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    RechargeRecorQuery();
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;

                default:
                    PacsReportQuery();
                    break;
            }
        }
    }
}
