using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.WeiHaiZXYY.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel :YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            HideNavigating = true;
            //PayCostRecordModel.Res获取已结算记录.data.ForEach((t) =>
            //{
            //    t.billItem.ForEach((p) =>
            //    {
            //        try
            //        {
            //            p.itemPrice = (decimal.Parse(p.itemPrice) * 100).ToString();
            //            p.billFee = (decimal.Parse(p.billFee) * 100).ToString();
            //        }
            //        catch 
            //        {
            //            p.itemPrice = "无法显示";
            //            p.billFee = "无法显示";
            //        }

            //    });
            //});
            Collection = PayCostRecordModel.Res获取已结算记录.data.Select(p => new PageData
            {
                //CatalogContent = $"已缴费列表",
                List = p.billItem,
                //Tag = p,
            }).ToArray();
            //BillCount = $"{PayCostRecordModel.Res获取已结算记录.data.Count}张处方单";
            //TotalAmount = PayCostRecordModel.Res获取已结算记录.data.Sum(p => decimal.Parse(p.billFee)).In元();

            //
            //
            //PlaySound(SoundMapping.选择待缴费处方));
        }
    }
}
