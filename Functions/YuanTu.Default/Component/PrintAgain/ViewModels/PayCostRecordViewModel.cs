using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Services.PrintService;
using YuanTu.Consts;
using System.Text;
using YuanTu.Consts.Services;
using YuanTu.Consts.Models.Auth;

namespace YuanTu.Default.Component.PrintAgain.ViewModels
{
    public class PayCostRecordViewModel : ViewModelBase
    {
        public override string Title => "查询缴费记录";
        public PayCostRecordViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        #region Interface

        [Dependency]
        public IPayCostRecordModel PayCostRecordModel { get; set; }
        [Dependency]
        public ISettlementRecordModel SettlementRecordModel { get; set; }
        [Dependency]
        public IPrintManager PrintManager { get; set; }
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IPrintModel PrintModel { get; set; }

        #endregion Interface

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = PayCostRecordModel.Res获取已结算记录.data.Select(p => new PageData
            {
                CatalogContent =
                    $"{p.tradeTime.SafeConvertToDate("yyyy-MM-dd HH:mm:ss", "yyyy年MM月dd日")} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem,
                Tag = p
            }).ToArray();
            BillCount = $"{PayCostRecordModel.Res获取已结算记录.data.Count}张处方单";
            TotalAmount = PayCostRecordModel.Res获取已结算记录.data.Sum(p => decimal.Parse(p.billFee)).In元();
        }

        public ICommand ConfirmCommand { get; set; }
        protected virtual void Confirm()
        {
            SettlementRecordModel.所选已缴费概要 = SelectData.Tag.As<已缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);
            PrintModel.SetPrintInfo(true, new PrintInfo
            {
                TypeMsg = "补打成功",
                TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分补打",
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = SettlementRecordPrintables(),
                TipImage = "提示_凭条"
            });
            SettlementRecordPrintables();
            Navigate(A.JFBD.Print);
            //
        }
        protected virtual Queue<IPrintable> SettlementRecordPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            return queue;
        }
        #region Binding
        public PageData SelectData { get; set; }

        private IReadOnlyCollection<PageData> _collection;
        private ObservableCollection<InfoMore> _data;

        public IReadOnlyCollection<PageData> Collection
        {
            get { return _collection; }
            set
            {
                _collection = value;
                OnPropertyChanged();
            }
        }

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

        private string _totalAmount;

        public string TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}