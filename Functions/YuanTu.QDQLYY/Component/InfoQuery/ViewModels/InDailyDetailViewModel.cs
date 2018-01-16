using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using System.Windows;
using System.Windows.Documents;
using YuanTu.Core.Extension;
using YuanTu.Consts.UserControls;
using System.Windows.Input;
using Prism.Commands;

namespace YuanTu.QDQLYY.Component.InfoQuery.ViewModels
{
    class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        public InDailyDetailViewModel()
        {
            DataChangeCommand = new DelegateCommand(dataChangeCommand);
        }

        public ICommand DataChangeCommand { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            //HideNavigating = true;
            InDailyDetailModel.Res住院患者费用明细查询?.data.Sort((x, y) =>{ return x.tradeTime.CompareTo(y.tradeTime); });
            var totalAmount = InDailyDetailModel.Res住院患者费用明细查询?.data.Sum(p => decimal.Parse(p.cost)).In元();
            var info = PatientModel.住院患者信息;
            string firstRow = new string(' ', 60)+ string.Format("住院费用清单\r\n 住院号：{0}    姓名：{1}    住院科室：{2}     床号：{3}    合计金额：{4}", info.patientHosId.TrimStart('0'), info.name, info.deptName, info.bedNo,totalAmount);
            Hint = firstRow;
            //Hint = $"住院费用清单  总计：{totalAmount}";
            DailyDetailData = new ListDataGrid.PageDataEx
            {
                CatalogContent = "",
                List = InDailyDetailModel.Res住院患者费用明细查询?.data.Select(p => new
                {
                    tradeTime = p.tradeTime.Split(' ')[0],
                    itemName = p.itemName,
                    itemUnits = p.itemUnits,
                    itemPrice = p.itemPrice,
                    itemQty = p.itemQty,
                    cost = p.cost,
                }),
                Tag = ""
            };
            HideNavigating = true;
        }
        protected void dataChangeCommand()
        {
            TimeOut = 220;
        }
    }
}
