using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.UserControls;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class MedicineItemsViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.MedicineItemsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = MedicineModel.Res药品项目查询.data,
                Tag = ""
            };
        }
    }
}
