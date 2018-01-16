using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        [Dependency]
        public IInDailyDetailModel InDailyDetailModel { get; set; }

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
                            endDate = DateTimeEnd.ToString("yyyy-MM-dd")
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
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;
                case InfoQueryTypeEnum.住院一日清单:
                    InDailyDetaiQuery();
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    RechargeRecorQuery();
                    break;
                default:
                    PacsReportQuery();
                    break;
            }
        }

        protected virtual void InDailyDetaiQuery()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeEnd;

            DoCommand(lp =>
            {
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    cardNo = PatientModel.住院患者信息.cardNo,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细");
            });
        }
    }
}
