using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.InfoQuery.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.InfoQuery.ViewModels
{
    class PayCostRecordViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        [Dependency]
        public IInfoQueryModel InfoQuery { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            HideNavigating = true;
            ChangeNavigationContent("");
            var list = InfoQuery.Res缴费结算查询.BILLMX.Select(i => new billdetail()
            {
                billNo = i.billNo,
                tradeTime = i.tradeTime,
                receiptNo = i.receiptNo,
                billFee = ConvertYuan2Fen(i.billFee),
                billType = i.billType,
                selfFee = ConvertYuan2Fen(i.selfFee),
                insurFee = ConvertYuan2Fen(i.insurFee),
                insurFeeInfo = i.insurFeeInfo,
                discountFee = ConvertYuan2Fen(i.discountFee),
                tradeMode = i.tradeMode,
                operId = i.operId,

            }).ToArray();

            Collection = new[]{new PageData
            {
                CatalogContent =null,
                List = list,
                Tag = list
            }};
            //BillCount = $"{list.Length}张处方单";
            TotalAmount = list.Sum(p => decimal.Parse(p.billFee)).In元();
        }

        string ConvertYuan2Fen(string s)
        {
            var value = decimal.Parse(s);
            return (value * 100).ToString("0");
        }
    }
}
