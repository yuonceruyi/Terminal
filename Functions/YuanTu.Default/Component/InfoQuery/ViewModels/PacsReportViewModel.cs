using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class PacsReportViewModel : ViewModelBase
    {
        private IReadOnlyCollection<PageData> _collection;

        public override string Title => "影像报告信息";
        public PageData SelectData { get; set; }

        public IReadOnlyCollection<PageData> Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            RowHeight = "1 1 1 3 3";
            ColumnWidth = "1 2";
            ChangeNavigationContent("");

            Collection = PacsReportModel.Res影像诊断结果查询.data.Select(p => new PageData
            {
                CatalogContent = $"{p.checkItem}\r\n{p.reportTime}",
                List = ListParse(p),
                Tag = p
            }).ToArray();
            BillCount = $"{PacsReportModel.Res影像诊断结果查询.data.Count}张影像";
        }

        public virtual List<PacsInfo> ListParse(影像诊断结果 result)
        {
            var pacsInfo = new PacsInfo
            {
                姓名 = result.name,
                报告时间 = result.reportTime,
                检查项目 = result.checkItem,
                检查结果 = result.checkResult,
                检查诊断 = result.diagnosis
            };
            return new List<PacsInfo> {pacsInfo};
        }

        public class PacsInfo
        {
            public string 姓名 { get; set; }
            public string 报告时间 { get; set; }
            public string 检查项目 { get; set; }
            public string 检查结果 { get; set; }
            public string 检查诊断 { get; set; }
        }

        #region Interface

        [Dependency]
        public IPacsReportModel PacsReportModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        #endregion Interface

        #region Binding

        private string _rowHeight;

        public string RowHeight
        {
            get { return _rowHeight; }
            set
            {
                _rowHeight = value;
                OnPropertyChanged();
            }
        }

        private string _columnWidth;

        public string ColumnWidth
        {
            get { return _columnWidth; }
            set
            {
                _columnWidth = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<InfoMore> _data;

        public ObservableCollection<InfoMore> Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

        private string _billCount;

        public string BillCount
        {
            get { return _billCount; }
            set
            {
                _billCount = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}