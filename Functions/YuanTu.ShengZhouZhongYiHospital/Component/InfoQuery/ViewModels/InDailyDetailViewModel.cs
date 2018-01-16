using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.PrintView;

namespace YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.ViewModels
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
                var flag = false;
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
                        resources["科室床位"] = PatientModel.住院患者信息.deptName;
                        resources["病案号"] = PatientModel.Req住院患者信息查询.patientId;
                        resources["病人性质"] = PatientModel.住院患者信息.patientType;
                        resources["时间"] = InDailyDetailModel.Res住院患者费用明细查询.data[0].tradeTime;
                        resources["笔数"] = InDailyDetailModel.Res住院患者费用明细查询.data.Count.ToString();

                        resources["打印日期"] = DateTimeCore.Now.ToString("yyyyMMdd HH:mm:ss");
                        var contentRows = (
                            from block in docs.doc.Blocks
                            where block is Table && block.Name == "Contents"
                            select (block as Table).RowGroups.First(one => one.Name == "ContentRows")
                        ).FirstOrDefault();
                        if (contentRows == null)
                            return;
                        int i = 0;
                        decimal toCost = 0;
                        for (int j = 0; j < InDailyDetailModel.Res住院患者费用明细查询.data.Count; j = j + 2)
                        {
                            var item1 = InDailyDetailModel.Res住院患者费用明细查询.data[j];
                            住院患者费用明细 item2 = null;
                            var no2 = (j + 2).ToString();
                            var dep2 = PatientModel.住院患者信息.deptName;
                            var border = new Thickness(0, 1, 1, 0);
                            var borderTop = new Thickness(1, 1, 1, 0);
                            var borderTop2 = new Thickness(0, 1, 1, 0);
                            if ((j + 1) < InDailyDetailModel.Res住院患者费用明细查询.data.Count)
                            {
                                item2 = InDailyDetailModel.Res住院患者费用明细查询.data[j + 1];
                            }
                            else
                            {
                                dep2 = "";
                                no2 = "";
                                borderTop = new Thickness(1, 1, 1, 1);
                                borderTop2 = new Thickness(0, 1, 1, 1);
                                border = new Thickness(0, 1, 1, 1);
                            }
                            var row = new TableRow();
                            row.Cells.Add(new TableCell(new Paragraph(new Run((j + 1).ToString()))) { TextAlignment = TextAlignment.Left, BorderThickness = borderTop });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            var name = item1.itemName.Substring(item1.itemName.IndexOf("]") + 1);
                            name = name.Length > 6 ? name.Substring(0, 6) + "..." : name;
                            row.Cells.Add(new TableCell(new Paragraph(new Run(name))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item1.itemPrice.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item1.itemQty))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item1.cost.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(PatientModel.住院患者信息.deptName))) { TextAlignment = TextAlignment.Center, BorderThickness = border });
                            toCost += decimal.Parse(item1.cost.InRMB());

                            row.Cells.Add(new TableCell(new Paragraph(new Run(no2)))
                            {
                                TextAlignment = TextAlignment.Left,
                                BorderThickness = borderTop2
                            });
                            row.Cells.Add(new TableCell(new Paragraph(new Run("")))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            row.Cells.Add(new TableCell(new Paragraph(new Run("")))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            name = string.IsNullOrEmpty(item2?.itemName) ? "" : item2.itemName.Substring(item2.itemName.IndexOf("]") + 1);
                            name = name.Length > 6 ? name.Substring(0, 6) + "..." : name;
                            row.Cells.Add(new TableCell(new Paragraph(new Run(name)))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item2?.itemPrice?.InRMB())))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item2?.itemQty)))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            row.Cells.Add(new TableCell(new Paragraph(new Run(item2?.cost?.InRMB())))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });

                            row.Cells.Add(new TableCell(new Paragraph(new Run(dep2)))
                            {
                                TextAlignment = TextAlignment.Center,
                                BorderThickness = border
                            });
                            if (!string.IsNullOrEmpty(item2?.cost?.InRMB()))
                            {
                                toCost += decimal.Parse(item2?.cost?.InRMB());
                            }
                            contentRows.Rows.Add(row);
                            i = i + 2;
                        }
                        resources["合计"] = toCost.ToString();
                        printHelperEx.Print(docs.doc, "住院日清单", 0, 0, null, null, true, false);
                        docs.Close();
                        #endregion
                        e.Set();
                    });
                    t.SetApartmentState(ApartmentState.STA);
                    t.Start();
                    e.WaitOne();
                    var task = Task.Run(() =>
                    {
                        Thread.Sleep(23000);
                        flag = true;
                    });
                    while (true)
                    {
                        if (flag)
                        {
                            break;
                        }
                        Thread.Sleep(1000);
                    }
                    Navigate(A.Home);
                }
                catch (Exception ex)
                {
                    Navigate(A.Home);
                }
            });

        }
    }
}
