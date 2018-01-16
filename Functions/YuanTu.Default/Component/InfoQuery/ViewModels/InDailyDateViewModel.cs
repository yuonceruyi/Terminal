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

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class InDailyDateViewModel : ViewModelBase
    {
        public InDailyDateViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "住院日清单打印";
        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IInDailyDetailModel InDailyDetailModel { get; set; }

        /// <summary>
        ///     进入当期页面时触发
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnEntered(NavigationContext navigationContext)
        {
            DateTimeStart = DateTimeCore.Now.AddDays(-1);
        }

        protected virtual void Confirm()
        {
            InDailyDetailModel.StartDate = DateTimeStart;
            InDailyDetailModel.EndDate = DateTimeStart.AddDays(1);

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
                            $"{DateTimeStart:yyyy-MM-dd}至\n{InDailyDetailModel.EndDate:yyyy-MM-dd}");
                        Next();
                        return;
                    }
                ShowAlert(false, "住院患者费用明细查询", "没有获得住院患者费用明细",debugInfo: InDailyDetailModel.Res住院患者费用明细查询?.msg);
            });
        }

        #region Binding

        private DateTime _dateTimeStart;

        public DateTime DateTimeStart
        {
            get { return _dateTimeStart; }
            set
            {
                _dateTimeStart = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}