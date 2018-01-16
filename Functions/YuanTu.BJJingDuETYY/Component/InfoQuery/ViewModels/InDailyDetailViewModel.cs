using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using System.Windows;
using System.Windows.Documents;
using YuanTu.Core.Extension;
using YuanTu.Consts;

namespace YuanTu.BJJingDuETYY.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        protected virtual string Caption
        {
            get { return FrameworkConst.HospitalName + "住院病人医药费用一日清单"; }
        }
        protected string Tips2 = "";
        public override void OnSet()
        {
            base.OnSet();
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            var sum = 0m;
            var doc = this.View.FindName("doc") as FrameworkContentElement;
            var resources = doc.Resources;
            var info = PatientModel.住院患者信息;
            var items = InDailyDetailModel.Res住院患者费用明细查询.data;

            resources["医院名称"] = Caption;
            resources["友情提示2"] = Tips2;
            string firstRow = string.Format("住院号：{0}      姓名：{1}      住院科室：{2}      床号：{3}      计费期间：{4}---{5}", info.patientHosId, info.name, info.deptName, info.bedNo, InDailyDetailModel.StartDate.ToString("yyyy年MM月dd日"), InDailyDetailModel.EndDate.ToString("yyyy年MM月dd日"));
            resources["首行"] = firstRow;

            var contentRows = this.View.FindName("details") as TableRowGroup;

            if (contentRows == null)
                return;
            contentRows.Rows.Clear();

            foreach (var item in items)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName)) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemSpecs)) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 2
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits)) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 1
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemQty)) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 1
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemPrice.InRMB())) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 1
                });
                var cost = decimal.Parse(item.cost);
                row.Cells.Add(new TableCell(new Paragraph(new Run(cost.InRMB())) { TextAlignment = TextAlignment.Center })
                {
                    ColumnSpan = 1
                });
                sum += cost;
                contentRows.Rows.Add(row);
            }
            resources["日清单费用合计"] = sum.InRMB();
            resources["截止到目前总费用"] = decimal.Parse(info.cost).InRMB();
            OnPropertyChanged();
        }
    }
}
