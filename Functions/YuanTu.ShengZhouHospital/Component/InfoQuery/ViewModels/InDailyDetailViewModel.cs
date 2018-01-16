using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.PrintView;

namespace YuanTu.ShengZhouHospital.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.InDailyDetailViewModel
    {
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public ICommand ConfirmCommand => new DelegateCommand(Print);

        private void Print()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在打印住院日清单，请稍后...");
                var e = new AutoResetEvent(false);
                try
                {
                    var t = new Thread(() =>
                    {
                        #region 打印
                        Logger.Main.Info("住院日清单");
                        var printName = ConfigurationManager.GetValue("Printer:Laser");
                        var printHelperEx = new PrintHelperEx(printName);

                        Logger.Main.Info("住院日清单打印 打印机名称" + printName);
                        var docs = new InDailyDetailPrint(printName);
                        var resources = docs.doc.Resources;
                        resources["姓名"] = PatientModel.住院患者信息.name;
                        resources["住院号"] = PatientModel.Req住院患者信息查询.patientId;
                        resources["费用性质"] = "暂无";
                        resources["费用合计"] = "暂无";
                        resources["病区床位"] = "暂无";
                        resources["入院日期"] = "暂无";
                        resources["出院日期"] = "暂无";
                        resources["清单日期"] = InDailyDetailModel.StartDate.ToString("yyyy-MM-dd");
                        resources["可报合计"] = "暂无";
                        resources["不可报合计"] = "暂无";
                        var contentRows = (
                            from block in docs.doc.Blocks
                            where block is Table && block.Name == "Contents"
                            select (block as Table).RowGroups.First(one => one.Name == "ContentRows")
                        ).FirstOrDefault();
                        if (contentRows == null)
                            return;

                        decimal ToCost = 0;
                        int i = 1;
                        foreach (var item in InDailyDetailModel.Res住院患者费用明细查询.data)
                        {
                            var border = new Thickness(0,0,1,0);
                            if (i == InDailyDetailModel.Res住院患者费用明细查询.data.Count)
                            {
                                border= new Thickness(0, 0, 1, 1);
                            }
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run(i.ToString()))) { TextAlignment = TextAlignment.Left, BorderThickness = new Thickness(1, 0, 1, 1) });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))) { TextAlignment = TextAlignment.Center , BorderThickness =border});
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemQty))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemPrice.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            contentRows.Rows.Add(row);
                            i++;
                        }
                        printHelperEx.Print(docs.doc, "住院日清单", 0, 0, null, null, true, true);
                        docs.Close();
                        #endregion
                        e.Set();
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    e.WaitOne();
                }
                catch (Exception ex)
                {
                    return;
                }
            });

        }
    }
}
