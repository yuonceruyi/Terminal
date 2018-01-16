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
using YuanTu.Consts.Models.PrintAgain;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.PrintAgain.ViewModels
{
    public class DateTimeViewModel : ViewModelBase
    {
        private DateTime _dateTimeEnd;
        private DateTime _dateTimeStart;
        private string _hint = "查询时段";

        public DateTimeViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "选择查询日期";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateTimeStart
        {
            get { return _dateTimeStart; }
            set
            {
                _dateTimeStart = value;
                OnPropertyChanged();

                DateTimeEndStartDate = value;
                DateTimeEndEndDate = value.AddDays(7);
            }
        }

        public DateTime DateTimeEnd
        {
            get { return _dateTimeEnd; }
            set
            {
                _dateTimeEnd = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeStartStartDate;

        public DateTime DateTimeStartStartDate
        {
            get { return _dateTimeStartStartDate; }
            set
            {
                _dateTimeStartStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeStartEndDate;

        public DateTime DateTimeStartEndDate
        {
            get { return _dateTimeStartEndDate; }
            set
            {
                _dateTimeStartEndDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeEndStartDate;

        public DateTime DateTimeEndStartDate
        {
            get { return _dateTimeEndStartDate; }
            set
            {
                _dateTimeEndStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeEndEndDate;

        public DateTime DateTimeEndEndDate
        {
            get { return _dateTimeEndEndDate; }
            set
            {
                _dateTimeEndEndDate = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IPrintAgainModel PrintAgainModel { get; set; }

        [Dependency]
        public IPayCostRecordModel PayCostRecordModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IDiagReportModel DiagReportModel { get; set; }

        [Dependency]
        public IPacsReportModel PacsReportModel { get; set; }

        [Dependency]
        public IRechargeRecordModel RechargeRecordModel { get; set; }
        [Dependency]
        public IInPrePayRecordModel InPrePayRecordModel { get; set; }

        public ICommand ConfirmCommand { get; set; }

        protected virtual void Confirm()
        {
            switch (PrintAgainModel.PrintAgainType)
            {
                case PrintAgainTypeEnum.缴费补打:
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


                default:
                    //
                    break;
            }
        }


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

            DateTimeStart = today.AddDays(-7);
            DateTimeEnd = today;
        }

        #endregion Overrides of ViewModelBase
    }
}