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
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
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
        public IQueryChoiceModel QueryChoiceModel { get; set; }

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
            switch (QueryChoiceModel.InfoQueryType)
            {
                case InfoQueryTypeEnum.已缴费明细:
                    PayCostQuery();
                    break;

                case InfoQueryTypeEnum.药品查询:
                    break;

                case InfoQueryTypeEnum.项目查询:
                    break;

                case InfoQueryTypeEnum.检验结果:
                    DiagReportQuery();
                    break;
                case InfoQueryTypeEnum.交易记录查询:
                    RechargeRecorQuery();
                    break;
                case InfoQueryTypeEnum.住院押金查询:
                    InPrePayRecordQuery();
                    break;
                case InfoQueryTypeEnum.药品变动价格查询:
                    break;
                case InfoQueryTypeEnum.项目变动价格查询:
                    break;
                default:
                    PacsReportQuery();
                    break;
            }
        }

        protected virtual void PayCostQuery()
        {
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
        }

        /// <summary>
        ///     检验结果查询
        /// </summary>
        protected virtual void DiagReportQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "检验报告信息查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询检验报告信息，请稍候...");
                DiagReportModel.Req检验基本信息查询 = new req检验基本信息查询
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    patientId = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].patientId,
                    patientName = PatientModel.Res病人信息查询?.data[PatientModel.PatientInfoIndex].name,
                    startTime = DateTimeStart.ToString("yyyy-MM-dd"),
                    endTime = DateTimeEnd.ToString("yyyy-MM-dd"),
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                    type = "1" //门诊
                };
                DiagReportModel.Res检验基本信息查询 = DataHandlerEx.检验基本信息查询(DiagReportModel.Req检验基本信息查询);
                if (DiagReportModel.Res检验基本信息查询?.success ?? false)
                {
                    if (DiagReportModel.Res检验基本信息查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                    return;
                }
                ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息");
            });
        }

        /// <summary>
        ///    影像检验结果查询
        /// </summary>
        protected virtual void PacsReportQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "影像报告信息查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询影像报告信息，请稍候...");
                PacsReportModel.Req影像诊断结果查询 = new req影像诊断结果查询
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    platformId = PatientModel.当前病人信息.platformId,
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd"),
                    type = "1" //查询类型 N1、门诊号 2、住院号3、全部
                };
                PacsReportModel.Res影像诊断结果查询 = DataHandlerEx.影像诊断结果查询(PacsReportModel.Req影像诊断结果查询);
                if (PacsReportModel.Res影像诊断结果查询?.success ?? false)
                {
                    if (PacsReportModel.Res影像诊断结果查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "影像报告信息查询", "没有获得影像报告信息(列表为空)");
                    return;
                }
                ShowAlert(false, "影像报告信息查询", "没有获得影像报告信息");
            });
        }
        /// <summary>
        ///交易记录查询
        /// </summary>
        protected virtual void RechargeRecorQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "交易记录查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询交易记录信息，请稍候...");
                RechargeRecordModel.Req查询预缴金充值记录 = new req查询预缴金充值记录
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    patientId = PatientModel.当前病人信息.patientId,
                    patientName = PatientModel.当前病人信息.name,
                    startDate = DateTimeStart.ToString("yyyy-MM-dd"),
                    endDate = DateTimeEnd.ToString("yyyy-MM-dd")
                };
                RechargeRecordModel.Res查询预缴金充值记录 = DataHandlerEx.查询预缴金充值记录(RechargeRecordModel.Req查询预缴金充值记录);
                if (RechargeRecordModel.Res查询预缴金充值记录?.success ?? false)
                {
                    if (RechargeRecordModel.Res查询预缴金充值记录?.data?.Count > 0)
                    {
                        ChangeNavigationContent(
                            $"{DateTimeStart.ToString("yyyy-MM-dd")}至\n{DateTimeEnd.ToString("yyyy-MM-dd")}");
                        Next();
                        return;
                    }
                    ShowAlert(false, "交易记录查询", "没有获得交易记录(列表为空)");
                    return;
                }
                ShowAlert(false, "交易记录查询", "没有获得交易记录");
            });
        }

        /// <summary>
        ///     住院押金查询
        /// </summary>
        protected virtual void InPrePayRecordQuery()
        {
            ShowAlert(false, "温馨提示", "业务未实现");
        }

        /// <summary>
        /// 自费记录信息查询 
        /// </summary>
        protected virtual void SelfPayingQuery()
        {
            ShowAlert(false, "温馨提示", "业务未实现");
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