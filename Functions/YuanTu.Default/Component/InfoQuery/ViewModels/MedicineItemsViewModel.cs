using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Component.InfoQuery.ViewModels
{
    public class MedicineItemsViewModel : ViewModelBase
    {
        private string _hint = "药品信息";

        private ListDataGrid.PageDataEx _medicineItemsData;
        public override string Title => "药品列表";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public ListDataGrid.PageDataEx MedicineItemsData
        {
            get { return _medicineItemsData; }
            set
            {
                _medicineItemsData = value;
                OnPropertyChanged();
            }
        }

        [Dependency]
        public IMedicineModel MedicineModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = MedicineModel.Res药品项目查询.data,
                Tag = ""
            };
        }
    }
}