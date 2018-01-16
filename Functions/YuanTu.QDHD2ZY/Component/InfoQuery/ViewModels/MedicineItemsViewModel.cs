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
using YuanTu.Core.Extension;

namespace YuanTu.QDHD2ZY.Component.InfoQuery.ViewModels
{
    public class MedicineItemsViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.MedicineItemsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            foreach (var info in MedicineModel.Res药品项目查询.data)
            {
                info.medicalRatio += "%";
            }

            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = MedicineModel.Res药品项目查询.data,
                Tag = ""
            };
        }
    }
}
