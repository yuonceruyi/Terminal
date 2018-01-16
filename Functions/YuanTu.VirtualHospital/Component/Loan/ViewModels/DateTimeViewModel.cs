using System;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase;
using YuanTu.VirtualHospital.Component.Loan.Models;

namespace YuanTu.VirtualHospital.Component.Loan.ViewModels
{
    internal class DateTimeViewModel : ViewModelBase
    {
        public DateTimeViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "选择查询日期";

        [Dependency]
        public ILoanModel LoanModel { get; set; }

        public DelegateCommand ConfirmCommand { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var today = DateTimeCore.Today;

            DateTimeStartStartDate = today.AddYears(-10);
            DateTimeStartEndDate = today;
            DateTimeEndStartDate = today.AddYears(-10);
            DateTimeEndEndDate = today;

            DateTimeStart = today.AddDays(-30);
            DateTimeEnd = today;
        }

        protected virtual void Confirm()
        {
            var start = DateTimeStart;
            var end = DateTimeEnd;
            DoCommand(lp =>
            {
                var req = new req查询借款账单()
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),

                    endDate = end.ToString("yyyy-MM-dd"),
                    startDate = start.ToString("yyyy-MM-dd"),

                    pageSize = "-1",
                    currentPage = "1"
                };
                LoanModel.Req查询借款账单 = req;
                var res = DataHandlerEx.查询借款账单(req);
                LoanModel.Res查询借款账单 = res;
                if (res == null || !res.success)
                {
                    ShowAlert(false, "查询借款账单", $"查询借款账单失败\n{res?.msg}");
                    return;
                }

                if (!res.data.billItem.Any())
                {
                    ShowAlert(false, "查询借款账单", "未找到当前时间段内的借款账单");
                    return;
                }

                Next();
            });
        }

        #region Bindings

        private string _hint = "查询时段";

        public string Hint
        {
            get => _hint;
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeStart;

        public DateTime DateTimeStart
        {
            get => _dateTimeStart;
            set
            {
                _dateTimeStart = value;
                OnPropertyChanged();

                DateTimeEndStartDate = value;
                DateTimeEndEndDate = value.AddDays(7);
            }
        }

        private DateTime _dateTimeEnd;

        public DateTime DateTimeEnd
        {
            get => _dateTimeEnd;
            set
            {
                _dateTimeEnd = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeStartStartDate;

        public DateTime DateTimeStartStartDate
        {
            get => _dateTimeStartStartDate;
            set
            {
                _dateTimeStartStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeStartEndDate;

        public DateTime DateTimeStartEndDate
        {
            get => _dateTimeStartEndDate;
            set
            {
                _dateTimeStartEndDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeEndStartDate;

        public DateTime DateTimeEndStartDate
        {
            get => _dateTimeEndStartDate;
            set
            {
                _dateTimeEndStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _dateTimeEndEndDate;

        public DateTime DateTimeEndEndDate
        {
            get => _dateTimeEndEndDate;
            set
            {
                _dateTimeEndEndDate = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }
}