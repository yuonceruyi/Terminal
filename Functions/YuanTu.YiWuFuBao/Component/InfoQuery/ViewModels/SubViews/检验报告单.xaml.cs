using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.PrintService;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.InfoQuery.ViewModels.SubViews
{
    /// <summary>
    /// 检验报告单.xaml 的交互逻辑
    /// </summary>
    public partial class 检验报告单 : UserControl
    {
        public 检验报告单()
        {
            InitializeComponent();
        }

        public void PrintData(ICardModel card,IPatientModel patient,检验基本信息 info)
        {
            var res = doc.Resources;
            foreach (var key in res.Keys)
            {
                if (!(key is string))
                {
                    continue;
                }
                res[key] = "";
            }
            res["检测类型"] = info.examType;
            res["打印时间"] = DateTimeCore.Now.ToString("yyyy年MM月dd日");
            res["条码号"] = info.extend;
            res["报告编号"] = info.reportId;

            res["姓名"] = info.patientName;
            res["性别"] = info.sex;
            res["年龄"] = info.age;
            res["样本类型"] = info.sampleType;
            res["门诊号"] = info.patientId;
            res["科室"] = info.checkPart;
            res["临床诊断"] =info.examResult;
            res["备注"] = info.remark;

            res["送检医生"] = info.inspectDoctor;
            res["检验者"] = info.checkDoc;
            res["审核者"] = info.auditDoc;
            res["采集时间"] = info.sendTime;
            res["接收时间"] = info.auditTime;
            res["报告时间"] = info.resultTime;

            var total = info.examItem.Count;
            var adjustCount = 12;
            if (total< adjustCount)
            {
                //此前提清空双表的
                foreach (var tableRowGroup in DoubleTable.RowGroups)
                {
                    tableRowGroup.Rows.Clear();
                }

                ExamItems.Rows.Clear();
                foreach (var itm in info.examItem)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm.itemAbbr))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm.itemUnits))) { TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm.itemRefRange))) { TextAlignment = TextAlignment.Center });
                    ExamItems.Rows.Add(row);
                }
                for (int i = info.examItem.Count; i < adjustCount; i++)
                {
                    var emptyRow = new TableRow();
                    for (int j = 0; j < 5; j++)
                    {
                        emptyRow.Cells.Add(new TableCell(new Paragraph()));
                    }
                    ExamItems.Rows.Add(emptyRow);
                }
            }
            else
            {
                foreach (var tableRowGroup in SingleTable.RowGroups)
                {
                    tableRowGroup.Rows.Clear();
                }
                DoubleExamItems.Rows.Clear();
                for (int i = 0; i < adjustCount; i++)
                {
                    var itm1 = info.examItem[i];
                    var itm2 = info.examItem.Count > adjustCount+i ? info.examItem[adjustCount + i]:null;

                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm1.itemAbbr))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm1.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm1.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm1.itemUnits))) { TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm1.itemRefRange))) { TextAlignment = TextAlignment.Center });

                    var it = new TableCell(new Paragraph(new Run(itm2?.itemAbbr)));
                    if (itm2!=null)
                    {
                        it.BorderThickness=new Thickness(1,0,0,0);
                        it.BorderBrush=new SolidColorBrush(Color.FromRgb(0,0,0));
                    }
                    row.Cells.Add(it);
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm2?.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm2?.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm2?.itemUnits))) { TextAlignment = TextAlignment.Center });
                    row.Cells.Add(new TableCell(new Paragraph(new Run(itm2?.itemRefRange))) { TextAlignment = TextAlignment.Center });
                    DoubleExamItems.Rows.Add(row);
                }
            }
          

            var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var 激光 = manager.GetValue("Printer:Laser");
            new PrintHelperEx(激光).Print(doc, "检验报告单");
        }
    }
}
