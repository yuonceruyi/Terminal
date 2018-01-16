using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : ViewModelBase
    {
        private IReadOnlyCollection<PageData> _collection;

        public DiagReportViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public ICommand ConfirmCommand { get; set; }
        public override string Title => "检验信息";
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
            ChangeNavigationContent("");

            Collection = DiagReportModel.Res检验基本信息查询.data.Select(p => new PageData
            {
                CatalogContent = $"{p.examType}\r\n{p.checkPart}",
                List = p.examItem,
                Tag = p
            }).ToArray();
            BillCount = $"{DiagReportModel.Res检验基本信息查询.data.Count}张报告单";
        }

        protected virtual void Confirm()
        {
            //todo 报告打印处理
        }

        #region Interface

        [Dependency]
        public IDiagReportModel DiagReportModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        #endregion Interface

        #region Binding

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