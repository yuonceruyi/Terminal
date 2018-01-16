using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{
    class InDailyDateViewModel: YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDateViewModel
    {
        

        [Dependency]
        public IQueryChoiceModel QueryChoiceModel { get; set; }

        [Dependency]
        public IInPrePayRecordModel InPreInPrePayRecordModel { get; set; }

        protected override void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart.AddDays(1);

            DoCommand(lp =>
            {
                var queryChoiceModel = GetInstance<IQueryChoiceModel>();
                if (queryChoiceModel.InfoQueryType == InfoQueryTypeEnum.住院一日清单)
                {
                    var req = new req住院患者费用明细查询
                    {
                        patientId = PatientModel.住院患者信息.patientId,
                        patientHosId = PatientModel.住院患者信息.patientHosId,
                        cardNo = PatientModel.住院患者信息.cardNo,
                        startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                        endDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd")
                        //endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
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
                }
                else if (queryChoiceModel.InfoQueryType == InfoQueryTypeEnum.住院押金查询)
                {
                    var req = new req住院预缴金充值记录查询
                    {
                        patientId = PatientModel.住院患者信息.patientId,
                        //patientHosId = PatientModel.住院患者信息.patientHosId,
                        cardType = PatientModel.当前病人信息.cardType,
                        cardNo = PatientModel.住院患者信息.cardNo,
                        startDate = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd"),
                        endDate = InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")
                    };

                    InPreInPrePayRecordModel.Res住院预缴金充值记录查询 = DataHandlerEx.住院预缴金充值记录查询(req);
                    if (InPreInPrePayRecordModel.Res住院预缴金充值记录查询?.success ?? false)
                        if (InPreInPrePayRecordModel.Res住院预缴金充值记录查询?.data?.Count > 0)
                        {
                            ChangeNavigationContent(
                                $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{InDailyDetailModel.EndDate.ToString("yyyy-MM-dd")}");
                            Next();
                            return;
                        }
                    ShowAlert(false, "住院患者充值记录查询", "没有获得住院患者充值记录");
                }

            });
        }
    }
}
