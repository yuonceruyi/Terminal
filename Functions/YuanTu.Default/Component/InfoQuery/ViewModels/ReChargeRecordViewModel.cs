using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class RechargeRecordViewModel: ViewModelBase
    {
        private string _hint = "预缴金充值记录";

        private ListDataGrid.PageDataEx _rechargeRecordData;
        public override string Title => "充值记录列表";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx RechargeRecordData
        {
            get { return _rechargeRecordData; }
            set
            {
                _rechargeRecordData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IRechargeRecordModel RechargeRecordModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            RechargeRecordData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = RechargeRecordModel.Res查询预缴金充值记录.data,
                Tag = ""
            };
        }
    }
}
