using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Linq;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class MaterialItemsViewModel : ViewModelBase
    {
        private ListDataGrid.PageDataEx _chargeItemsData;
        private string _hint = "材料项目收费信息";
        public override string Title => "材料费列表";

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
            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data.Where(p => p.type == "材料费"),
                Tag = ""
            };
        }
    }
}