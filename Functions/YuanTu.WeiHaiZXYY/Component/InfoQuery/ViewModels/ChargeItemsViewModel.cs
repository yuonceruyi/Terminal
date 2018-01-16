using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class ChargeItemsViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.ChargeItemsViewModel
    {
       
        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            ChargeItemsModel.Res收费项目查询.data.ForEach((p)=> 
            {
                try
                {
                    p.price = (decimal.Parse(p.price) * 100).ToString();
                }
                catch
                {
                    p.price = "暂无价格";
                }
                finally
                {
                    
                }
               
            });
            ChargeItemsData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = ChargeItemsModel.Res收费项目查询.data,
                Tag = ""
            };
        }
    }
}