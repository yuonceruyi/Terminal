using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models.Print;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Services;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {


        public override void OnEntered(NavigationContext navigationContext)
        {
            var totalAmount = InDailyDetailModel.Res住院患者费用明细查询?.data.Sum(p => decimal.Parse(p.cost)).In元();
            Hint = $"住院费用清单  所选查询时间段费用总计：{totalAmount}";
            DailyDetailData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InDailyDetailModel.Res住院患者费用明细查询?.data,
                Tag = ""
            };
        }


    }
}
