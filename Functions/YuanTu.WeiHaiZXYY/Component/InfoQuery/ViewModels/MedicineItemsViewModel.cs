using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class MedicineItemsViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.MedicineItemsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            MedicineModel.Res药品项目查询.data.ForEach((p) => 
            {
                p.price = (decimal.Parse(p.price) * 100).ToString();
            });
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