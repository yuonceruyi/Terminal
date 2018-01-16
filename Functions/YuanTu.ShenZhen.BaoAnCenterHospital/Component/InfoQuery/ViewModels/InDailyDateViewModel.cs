using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {
        public InDailyDateViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "住院一日清单查询";

        [Dependency]
        public ICardModel CardModel { get; set; }

        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart;  //同一天
            DoCommand(lp =>
            {
                var req = new req住院患者费用明细查询
                {
                    patientId = PatientModel.住院患者信息.patientHosId,
                    startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                    endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd"),
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                };

                InDailyDetailModel.Res住院患者费用明细查询 = DataHandlerEx.住院患者费用明细查询(req);
                if (InDailyDetailModel.Res住院患者费用明细查询?.success ?? false)
                    if (InDailyDetailModel.Res住院患者费用明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart:yyyy-MM-dd}至\n{InDailyDetailModel.EndDate:yyyy-MM-dd}");
                        Next();
                        return;
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细", debugInfo: InDailyDetailModel.Res住院患者费用明细查询?.msg);
            });
        }
    }
}