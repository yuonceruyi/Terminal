using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;

namespace YuanTu.JiaShanHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {
        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart.AddDays(1);

            DoCommand(lp =>
            {
                var extendArr = PatientModel.住院患者信息.extend.Split(',');
                var req = new req住院患者费用明细查询
                {
                    patientId = extendArr[0],
                    cardNo = PatientModel.住院患者信息.cardNo,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd")
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(DateTimeStart.ToString("yyyy-MM-dd"));
                        Next();
                        return;
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细");
            });
        }
    }
}
