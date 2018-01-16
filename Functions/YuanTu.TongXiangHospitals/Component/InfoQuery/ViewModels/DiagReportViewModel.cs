using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;
using YuanTu.TongXiangHospitals.Component.InfoQuery.ViewModels.SubViews;


namespace YuanTu.TongXiangHospitals.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel:Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

      
        private PrintHelperEx printHelperEx;
        protected override void Confirm()
        {
            DiagReportModel.所选检验信息 = SelectData.Tag.As<检验基本信息>();
            var t = new Thread(Run)
            {
                Name = "Printing",
                IsBackground = true,
            };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            Next();
        }

        protected virtual void Run()
        {
            检查报告打印();
        }
        public void 检查报告打印()
        {
            printHelperEx = new PrintHelperEx(ConfigurationManager.GetValue("Printer:Laser"));
            var docs = new LIS(ConfigurationManager);
            var info = DiagReportModel.所选检验信息;
            var patient = PatientModel.当前病人信息;
            var resources = docs.doc.Resources;
            resources["标题"] = $"{FrameworkConst.HospitalName}检验报告单";
            resources["门诊"] = string.IsNullOrEmpty(info.inhospId) ? "门诊" : "住院";
            resources["样本条码"] = info.reportId;   // "缺字段";

            resources["姓名"] = patient.name;
            resources["病历号"] = info.patientId;
            resources["标本种类"] = info.examType;
            resources["样本编号"] = info.reportId;

            resources["性别"] = patient.sex;
            resources["科别"] = info.checkPart;
            resources["送检医生"] = info.checkDoc;
            resources["临床诊断"] = info.extend;


            resources["年龄"] = null;
            resources["床号"] = null;
            resources["检验目的"] = info.examType;  // "缺字段";


            resources["接收时间"] = info.sendTime;
            resources["报告时间"] = info.resultTime;
            resources["检验者"] = info.checkDoc;
            resources["核对者"] = null;
            resources["总数"] = info.examItem.Count.ToString();
            //var res = info.examItem.OrderBy(d => d.showIndex).ToList();
            var contentRows = (
                from block in docs.doc.Blocks
                where block is Table && block.Name == "detail"
                select (block as Table).RowGroups.First(one => one.Name == "details")
                ).FirstOrDefault();
            if (contentRows == null)
                return;
            contentRows.Rows.Clear();
            int indexChecked = 1;
            foreach (var item in info.examItem)
            {
                var row = new TableRow();
                //No  项目    结果   提示  单位  参考值
                row.Cells.Add(new TableCell(new Paragraph(new Run(indexChecked.ToString())) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName)) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRealValue)) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.quaResult)) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits)) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRefRange)) { TextAlignment = TextAlignment.Left })
                {
                    ColumnSpan = 4
                });
                contentRows.Rows.Add(row);
                indexChecked++;
            }

            //var success = printHelperEx.Print(docs.doc, $"LIS.{patient.name}.{info.reportId}",
            //    0, 20, null, LISFooter);
            printHelperEx.Print(docs.doc,"检查报告");
            docs.Close();
           
        }

       


    }
}
