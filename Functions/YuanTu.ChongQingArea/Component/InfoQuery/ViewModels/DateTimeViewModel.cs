using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.InfoQuery;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        #region Overrides of ViewModelBase

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            var today = DateTimeCore.Today;

            DateTimeStartStartDate = today.AddYears(-10);
            DateTimeStartEndDate = today;
            DateTimeEndStartDate = today.AddYears(-10);
            DateTimeEndEndDate = today;

            DateTimeStart = today.AddDays(-Startup.PayCostRecordDay);
            DateTimeEnd = today;
        }

        [Dependency]
        public IInPrePayRecordModel InPreInPrePayRecordModel { get; set; }

        protected override void InPrePayRecordQuery()
        {
            var req = new req住院预缴金充值记录查询
            {
                patientId = PatientModel.住院患者信息.patientId,
                //patientHosId = PatientModel.住院患者信息.patientHosId,
                //cardType = PatientModel.当前病人信息.cardType,
                cardNo = PatientModel.住院患者信息.cardNo,
                startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                endDate = DateTimeEnd.ToString("yyyy-MM-dd")
            };

            InPreInPrePayRecordModel.Res住院预缴金充值记录查询 = DataHandlerEx.住院预缴金充值记录查询(req);
            if (InPreInPrePayRecordModel.Res住院预缴金充值记录查询?.success ?? false)
                if (InPreInPrePayRecordModel.Res住院预缴金充值记录查询?.data?.Count > 0)
                {
                    ChangeNavigationContent(
                        $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                    Next();
                    return;
                }
            ShowAlert(false, "住院患者充值记录查询", "没有获得住院患者充值记录");
        }
        #endregion Overrides of ViewModelBase
    }
}