using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;
using YuanTu.HuNanHangTianHospital.Component.InfoQuery.PrintView;
using YuanTu.Consts.Services;
using YuanTu.Consts.Models.Auth;
using System.Windows.Documents;
using System.Windows;
using System;
using System.Windows.Input;
using Prism.Commands;
using YuanTu.Core.Log;
using System.Threading;

namespace YuanTu.HuNanHangTianHospital.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        public override string Title => "查询缴费记录";

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }
        [Dependency]
        public IDiagReportModel DiagReportModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand(缴费明细打印);
            }
        }

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

            //var res = GetInstance<IResourceEngine>();
            //var sound = GetInstance<IAudioPlayer[]>().FirstOrDefault();
            //sound?.StartPlayAsync(res.GetResourceFullPath(SoundMapping.选择待缴费处方));
        }

        public void 缴费明细打印()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在打印缴费明细，请稍后...");
                var e = new AutoResetEvent(false);
                try
                {
                    var t = new Thread(() =>
                    {
                        #region 打印
                        Logger.Main.Info("缴费明细打印");
                        var printName = ConfigurationManager.GetValue("Printer:Laser");
                        var printHelperEx = new PrintHelperEx(printName);
                      
                        Logger.Main.Info("缴费明细打印 打印机名称"+printName);
                       
                        foreach (var 已结算概要信息 in PayCostRecordModel.Res获取已结算记录.data)
                        {
                            var docs = new BillDetailPrint(printName);
                            var resources = docs.doc.Resources;
                            resources["病人姓名"] = PatientModel.当前病人信息.name;
                            resources["性别"] = PatientModel.当前病人信息.sex;
                            resources["年龄"] = PatientModel.当前病人信息.birthday.Age().ToString();
                            resources["病人类别"] = 已结算概要信息.billType;
                            resources["挂号单号"] = 已结算概要信息.extend;
                            resources["就诊科室"] = 已结算概要信息.deptName;
                            resources["交易日期"] = $"{已结算概要信息.tradeTime}";
                            resources["医生"] = 已结算概要信息.doctName;
                                var contentRows = (
                                    from block in docs.doc.Blocks
                                    where block is Table && block.Name == "Contents"
                                    select (block as Table).RowGroups.First(one => one.Name == "ContentRows")
                                ).FirstOrDefault();
                            if (contentRows == null)
                                return;

                            decimal ToCost = 0;
                            int i = 1;
                            foreach (var item in 已结算概要信息.billItem)
                            {
                                var row = new TableRow();
                                row.Cells.Add(new TableCell(new Paragraph(new Run(i.ToString()))) { TextAlignment = TextAlignment.Left, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemSpecs))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemQty))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemPrice.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.billFee.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(item.billFee.InRMB()))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 1, 1) });
                                row.Cells.Add(new TableCell(new Paragraph(new Run(已结算概要信息.billNo))) { TextAlignment = TextAlignment.Center, BorderThickness = new Thickness(0, 0, 0, 1) });    
                                contentRows.Rows.Add(row);
                                ToCost += decimal.Parse(item.billFee);
                                i++;
                            }
                            var countRow = new TableRow();
                            countRow.Cells.Add(new TableCell(new Paragraph(new Run("合计："))) { TextAlignment = TextAlignment.Left, ColumnSpan = 6, BorderThickness = new Thickness(0, 0, 1, 0) });
                            countRow.Cells.Add(new TableCell(new Paragraph(new Run(ToCost.ToString().In元()))) { TextAlignment = TextAlignment.Left, BorderThickness = new Thickness(0, 0, 1, 0) });
                            countRow.Cells.Add(new TableCell(new Paragraph(new Run(ToCost.ToString().In元()))) { TextAlignment = TextAlignment.Left, BorderThickness = new Thickness(0, 0, 1, 0) });
                            countRow.Cells.Add(new TableCell(new Paragraph(new Run(""))) { TextAlignment = TextAlignment.Left });
                            contentRows.Rows.Add(countRow);
                            printHelperEx.Print(docs.doc, "缴费明细打印", 0, 0, null, null, true, true);
                            docs.Close();
                        }
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