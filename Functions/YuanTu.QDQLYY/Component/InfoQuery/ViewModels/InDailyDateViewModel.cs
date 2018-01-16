using System.Runtime.CompilerServices;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel : YuanTu.QDKouQiangYY.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {
        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart.AddDays(1);

            DoCommand(lp =>
            {
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
