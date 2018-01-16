using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.YanTaiYDYY.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = PayCostRecordModel.Res获取已结算记录.data.Select(p => new PageData
            {
                CatalogContent =
                    $"{p.tradeTime.SafeConvertToDate("yyyy-MM-dd HH:mm:ss", "yyyy年MM月dd日")} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem,
                Tag = p
            }).ToArray();
            BillCount = $"{PayCostRecordModel.Res获取已结算记录.data.Count}张处方单";
            TotalAmount = PayCostRecordModel.Res获取已结算记录.data.Sum(p => decimal.Parse(p.billFee)).In元();

            //
            //
            //PlaySound(SoundMapping.选择待缴费处方));
        }
    }
}
