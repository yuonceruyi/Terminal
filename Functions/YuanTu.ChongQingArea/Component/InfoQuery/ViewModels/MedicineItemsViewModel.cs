using Microsoft.Practices.Unity;
using Prism.Regions;
using System.Linq;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{
    public class MedicineItemsViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.MedicineItemsViewModel
    {
        private string _hint = "药品信息";


        public override string Title => "药品列表";



      

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            MedicineItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = MedicineModel.Res药品项目查询.data.Where( t=> t.type != null ).Select(p => new
                {
                    medicineName = p.medicineName,
                    specifications = p.specifications,
                    priceUnit = p.priceUnit,
                    price = p.price,
                    producer = p.producer,
                    type = p.type,
                }),
                Tag = ""
            };
        }
    }
}