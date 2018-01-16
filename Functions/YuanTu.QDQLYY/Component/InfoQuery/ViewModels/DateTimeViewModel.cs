using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        protected override void Confirm()
        {
            if (QueryChoiceModel.InfoQueryType == InfoQueryTypeEnum.住院一日清单)
                QueryDayDetail();//齐鲁住院一日清单可查多天
            else
                base.Confirm();
        }

        [Dependency]
        public IInDailyDetailModel InDailyDetailModel { get; set; }
        protected void QueryDayDetail()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "住院清单查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询住院清单，请稍候...");
                InDailyDetailModel.StartDate = DateTimeStart;
                InDailyDetailModel.EndDate = DateTimeEnd;
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                };
                //测试一年数据，正式需删除
                //req.endDate = InDailyDetailModel.EndDate.AddDays(365).ToString("yyyy-MM-dd");
                ////////////////////////////////
                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                {
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent($"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细");
            });
        }
    }
}