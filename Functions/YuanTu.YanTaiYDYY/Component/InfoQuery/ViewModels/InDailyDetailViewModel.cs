using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            var totalAmount = InDailyDetailModel.Res住院患者费用明细查询?.data.Sum(p => decimal.Parse(p.cost)).In元();
            Hint = $"住院费用清单  总计：{totalAmount}";
            DailyDetailData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InDailyDetailModel.Res住院患者费用明细查询?.data,
                Tag = ""
            };
        }
    }
}