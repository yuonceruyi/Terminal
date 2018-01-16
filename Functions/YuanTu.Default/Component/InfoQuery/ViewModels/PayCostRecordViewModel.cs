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

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel : ViewModelBase
    {
        public override string Title => "查询缴费记录";

        #region Interface

        [Dependency]
        public IPayCostRecordModel PayCostRecordModel { get; set; }

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

        #region Binding

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