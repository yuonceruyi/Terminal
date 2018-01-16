using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.House.Component.InfoQuery.Models;
using YuanTu.Default.House.Component.Auth.Models;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House.Component.InfoQuery.ViewModels
{
    public class DateTimeViewModel : ViewModelBase
    {
        public override string Title => "设置查询时间范围";

        public string Hint => "选择查询日期";
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

        private DateTime _dateTimeEnd;

        public DateTime DateTimeEnd
        {
            get { return _dateTimeEnd; }
            set
            {
                _dateTimeEnd = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand => new DelegateCommand(Confirm);

        [Dependency]
        public IHealthModel HealthModel { get; set; }

        [Dependency]
        public IReportModel ReportModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            DateTimeStart = DateTimeCore.Now.AddDays(-7);
            DateTimeEnd = DateTimeCore.Now;
            HideNavigating = true;
        }

        protected void Confirm()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询体检报告单，请稍候...");
                var req = new req查询体检报告单
                {
                    healthUserId = HealthModel?.Res查询是否已建档?.data?.id,
                    idNo = HealthModel?.Res查询是否已建档?.data?.idNo,
                    beginTime = DateTimeStart.ToString("yyyy-MM-dd"),
                    endTime = DateTimeEnd.AddDays(1).ToString("yyyy-MM-dd")
                };

                var res = HealthDataHandlerEx.查询体检报告单(req);
                if (res?.success ?? false)
                {
                    if (res?.data?.records?.Count > 0 && res.data?.records?[0].groupList.Count > 0)
                    {
                        ReportModel.查询体检报告单分页数据 = res.data;
                        Next();
                        return;
                    }
                    ShowAlert(false, "温馨提示", "查询检查报告单失败(返回列表为空)");
                    return;
                }
                ShowAlert(false, "温馨提示", "查询检查报告单失败", debugInfo: res?.msg);
            });
        }
    }
}