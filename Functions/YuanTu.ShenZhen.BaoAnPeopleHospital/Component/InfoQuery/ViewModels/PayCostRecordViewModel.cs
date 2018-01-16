using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using System;
using YuanTu.ShenZhenArea.Gateway;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        public override string Title => "查询已缴费记录";


        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = PayCostRecordModel.Res获取已结算记录.data.Select(p => new PageData
            {
                //CatalogContent = $"{p.tradeTime.SafeConvertToDate("yyyy-MM-dd HH:mm:ss", "yyyy年MM月dd日")} {p.deptName}\r\n金额 {p.billFee.In元()}",
                CatalogContent = $"{p.tradeTime.SafeConvertToDate("yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd")}{p.deptName}[{p.billType}]{p.billFee.In元()}",
                List = p.billItem.Where(d => !string.IsNullOrEmpty(d.itemName)).Select(d => new 结算记录()
                {
                    billFee = d.billFee,
                    itemName = d.itemName,
                    itemPrice = string.IsNullOrEmpty(d.itemPrice) ? "" : (Convert.ToDouble(d.itemPrice) * 100).ToString(),
                    cost = d.cost,
                    itemLiquid = d.itemLiquid,
                    itemQty = d.itemQty,
                    itemSpecs = d.itemQty,
                    itemUnits = d.itemUnits,
                    receiptNo = d.receiptNo,
                    tradeTime = d.tradeTime,
                    billType = d.itemSpecs
                }).ToList(),
                Tag = p
            }).ToArray();

            BillCount = $"{PayCostRecordModel.Res获取已结算记录.data.Count}张处方单";
            TotalAmount = PayCostRecordModel.Res获取已结算记录.data.Sum(p => decimal.Parse(p.billFee)).In元();
        }
    }
}