using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.SubViews;

namespace YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        private PrintHelperEx _printHelperEx;

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        protected override void Confirm()
        {
            DiagReportModel.所选检验信息 = SelectData.Tag.As<检验基本信息>();
            var t = new Thread(检查报告打印)
            {
                Name = "Printing",
                IsBackground = true
            };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            DoCommand(lp =>
            {
                lp.ChangeText($"正在打印,共计{DiagReportModel.Res检验基本信息查询?.data?.Count}张,请在出纸口取走报告单...");
                Thread.Sleep(7000);
                Next();
            });
            
        }

        protected virtual void 检查报告打印()
        {
            try
            {
                foreach (var info in DiagReportModel.Res检验基本信息查询.data)
                {
                    _printHelperEx = new PrintHelperEx(ConfigurationManager.GetValue("Printer:Laser"));
                    var docs = new LIS(ConfigurationManager);
                    //var info = DiagReportModel.所选检验信息;
                    var patient = PatientModel.当前病人信息;
                    var resources = docs.doc.Resources;
                    resources["姓名"] = patient.name;
                    resources["病人类型"] = string.IsNullOrEmpty(info.inhospId) ? "门诊" : "住院";
                    resources["床号"] = null; //缺失字段
                    resources["标本类型"] = info.examType;
                    resources["标本号"] = info.reportId;

                    resources["性别"] = patient.sex;
                    resources["住院号"] = info.inhospId;
                    resources["费别"] = null; //缺失字段
                    resources["采样时间"] = info.sendTime;

                    resources["年龄"] = info.age;
                    resources["科室"] = info.checkPart;
                    resources["诊断"] = null; //缺失字段
                    resources["备注"] = null; //缺失字段

                    resources["送检医生"] = info.inspectDoctor;
                    resources["核收时间"] = info.auditTime;
                    resources["报告日期"] = info.resultTime;
                    resources["检验者"] = info.checkDoc;
                    resources["复核者"] = info.auditDoc;

                    var contentRows = (
                        from block in docs.doc.Blocks
                        where block is Table && block.Name == "detail"
                        select (block as Table).RowGroups.First(one => one.Name == "details")
                    ).FirstOrDefault();
                    if (contentRows == null)
                    {
                        ShowAlert(false, "温馨提示", "打印失败：contentRows=null");
                        docs.Close();
                        return;
                    }

                    contentRows.Rows.Clear();
                    var indexChecked = 1;
                    foreach (var item in info.examItem)
                    {
                        var row = new TableRow();
                        //No  代号    项目   结果  参考值  单位
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(indexChecked.ToString()))
                            {
                                TextAlignment = TextAlignment.Left
                            })
                            {
                                ColumnSpan = 4
                            });
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(item.itemAbbr)) {TextAlignment = TextAlignment.Left})
                            {
                                ColumnSpan = 4
                            });
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(item.itemName))
                            {
                                TextAlignment = TextAlignment.Left
                            })
                            {
                                ColumnSpan = 4
                            });
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(item.itemRealValue))
                            {
                                TextAlignment = TextAlignment.Left
                            })
                            {
                                ColumnSpan = 4
                            });
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(item.itemRefRange)) {TextAlignment = TextAlignment.Left})
                            {
                                ColumnSpan = 4
                            });
                        row.Cells.Add(
                            new TableCell(new Paragraph(new Run(item.itemUnits)) {TextAlignment = TextAlignment.Left})
                            {
                                ColumnSpan = 4
                            });
                        contentRows.Rows.Add(row);
                        indexChecked++;
                    }
                    var result = _printHelperEx.Print(docs.doc, "检查报告");
                    if (!result.IsSuccess)
                        ShowAlert(false, "温馨提示", $"打印失败：{result.Message}");
                    docs.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Printer.Error($"{ex.Message} {ex.StackTrace}");
            }
            finally
            {
            }
        }
    }
}