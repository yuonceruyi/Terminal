using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class ChargeItemsViewModel : ViewModelBase
    {
        private ListDataGrid.PageDataEx _chargeItemsData;
        private string _hint = "诊疗项目收费信息";
        public override string Title => "诊疗项列表";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx ChargeItemsData
        {
            get { return _chargeItemsData; }
            set
            {
                _chargeItemsData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IChargeItemsModel ChargeItemsModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data,
                Tag = ""
            };
        }
    }
}