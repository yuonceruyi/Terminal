using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.QDHD2ZY.Component.InfoQuery.ViewModels
{
    public  class ChargeItemsViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.ChargeItemsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            foreach (var info in ChargeItemsModel.Res收费项目查询.data)
            {
                info.medicalRatio += "%";
            }

            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data,
                Tag = ""
            };
        }
    }
}
