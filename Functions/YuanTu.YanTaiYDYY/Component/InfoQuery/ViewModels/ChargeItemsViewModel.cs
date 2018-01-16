using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.UserControls;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class ChargeItemsViewModel:YuanTu.Default.Component.InfoQuery.ViewModels.ChargeItemsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data.Where(p =>p.type != "材料费"),
                Tag = ""
            };
        }
    }
}
