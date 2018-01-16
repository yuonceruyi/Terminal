using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;

namespace YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel:Default.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {
        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            //InDailyDetailModel.EndDate = DateTimeStart.AddDays(1);
            InDailyDetailModel.EndDate = DateTimeStart;
            DoCommand(lp =>
            {
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd"),
                    extend= GetInstance<ICardModel>().ExternalCardInfo
                };

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
