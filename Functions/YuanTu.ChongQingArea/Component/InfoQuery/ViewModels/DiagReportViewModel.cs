using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Services.PrintService;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using System.Xml;
using YuanTu.ChongQingArea.Component.InfoQuery.Views;
using YuanTu.Core.Log;

namespace YuanTu.ChongQingArea.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            Collection = DiagReportModel.Res检验基本信息查询.data.Select(p => new PageData
            {
                CatalogContent = $"{p.examType}\r\n{p.checkPart}",
                List = p.examItem,
                Tag = p
            }).ToArray();
            BillCount = $"{DiagReportModel.Res检验基本信息查询.data.Count}张报告单";
        }

        protected override void Confirm()
        {
            var info = SelectData.Tag as 检验基本信息;
            if (!string.IsNullOrEmpty(info.printTimes) && int.Parse(info.printTimes) > 1)
            {
                ShowAlert(false, "报告单打印", "该报告单已打印");
                return;
            }
            if (info.examItem==null || info.examItem.Count==0)
            {
                ShowAlert(false, "报告单打印", "该报告单无结果，无法打印");
                return;
            }
            DoCommand(lp =>
            {
                var req = new req打印检验结果()
                {
                    patientId = PatientModel.当前病人信息.patientId,
                    reportId = info.reportId,
                };
                var res = DataHandlerEx.打印检验结果(req);
                if (!res.success)
                {
                    ShowAlert(false, "报告单打印", res.msg);
                    return;
                }
                var mre = new ManualResetEvent(false);
                var t = new Thread(() =>
                {
                    try
                    {
                        Print(info);
                    }
                    finally
                    {
                        mre.Set();
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                mre.WaitOne();

                lp.ChangeText("正在上传打印结果，请稍后...");
                var uploadReq = new req上传打印检验结果()
                {
                    patientId = PatientModel.当前病人信息.patientId,
                    reportId = info.reportId,
                };
                var uploadRes = DataHandlerEx.上传打印检验结果(uploadReq);
                if (!uploadRes.success)
                {
                    ShowAlert(false, "报告单打印", res.msg);
                    return;
                }

                ShowAlert(true, "报告单打印", "打印完成！");
            });
        }

        private void Print(检验基本信息 info)
        {
            var window = new Report();
            var resources = window.doc.Resources;
            resources["姓名"] = info.patientName;
            resources["性别"] = info.sex;
            resources["年龄"] = info.age;
            resources["标本类型"] = info.sampleType;
            resources["标本条码号"] = info.barCode;
            resources["科室"] = info.sendDept;
            resources["床号"] = info.bedNo;
            resources["门诊号"] = info.inhospId;
            resources["检验号"] = info.checkNum;
            resources["临床诊断"] = info.examResult;
            resources["检验项目"] = info.itemType;
            resources["日期"] = info.date;
            resources["仪器"] = info.machineNum;
            resources["采样时间"] = info.sendTime;
            resources["签收时间"] = info.receiveTime;
            resources["申请医师"] = info.auditDoc;
            
            resources["报告时间"] = info.resultTime;
            resources["审核者"] = info.receiveDoct;
            resources["打印时间"] = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm");// info.打印时间;
            resources["打印者"] = FrameworkConst.OperatorId;//info.打印者;

            //打印电子签名
            {
                window.JYZ.Visibility=Visibility.Collapsed;
                resources["检验者"] = null;
                var jyzImg = GetRemoteImage(info.checkDoc);
                if (jyzImg == null)
                {
                    resources["检验者"] = info.checkDoc;
                }
                else
                {
                    window.JYZ.Source = jyzImg;
                    window.JYZ.Visibility = Visibility.Visible;
                }

                window.SHZ.Visibility=Visibility.Collapsed;
                resources["审核者"] = null;
                var shzImg = GetRemoteImage(info.receiveDoct);
                if (shzImg == null)
                {
                    resources["审核者"] = info.receiveDoct;
                }
                else
                {
                    window.SHZ.Source = shzImg;
                    window.SHZ.Visibility = Visibility.Visible;
                }


            }

            string bz = "";
            if (info.remark.Split('|').Length >= 3)
            {
                var bzs = info.remark.Split('|');
                bz += (bzs[0] == "null" ? "" : bzs[0]);
                bz += (bzs[1] == "null" ? "" : bzs[1]);
                bz += (bzs[2] == "null" ? "" : bzs[2]);
            }
            else
            {
                bz = info.remark;
            }
            resources["备注"] = bz;
            resources["提示"] = GetTips(info);
            var header1 = window.Table1Header;
            var header21 = window.Table2Column1Header;
            var header22 = window.Table2Column2Header;
            var rowGroup1 = window.Table1RowGroup;
            var rowGroup21 = window.Table2Column1RowGroup;
            var rowGroup22 = window.Table2Column2RowGroup;

            const int RowCount = 18;

            var count = info.examItem.Count;
            if (count <= RowCount)
            {
                header21.Rows.Clear();
                header22.Rows.Clear();

                foreach (var item in info.examItem)
                {
                    var row = new TableRow();

                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemAbbr))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(ParseMark(item.itemMark)))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRefRange))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.extend))));

                    rowGroup1.Rows.Add(row);
                }
                while (rowGroup1.Rows.Count < RowCount)
                {
                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph()));
                    rowGroup1.Rows.Add(row);
                }
                Print(window);
            }
            else if (count <= RowCount * 2)
            {
                header1.Rows.Clear();

                foreach (var item in info.examItem.Take(RowCount))
                {
                    var row = new TableRow();

                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(ParseMark(item.itemMark)))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRefRange))));

                    rowGroup21.Rows.Add(row);
                }
                foreach (var item in info.examItem.Skip(RowCount))
                {
                    var row = new TableRow();

                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRealValue))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(ParseMark(item.itemMark)))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRefRange))));

                    rowGroup22.Rows.Add(row);
                }
                Print(window);
            }
            else
            {
                header21.Rows.Clear();
                header22.Rows.Clear();

                var pageCount = count / RowCount + (count % RowCount > 0 ? 1 : 0);
                for (int i = 0; i < pageCount; i++)
                {
                    rowGroup1.Rows.Clear();
                    foreach (var item in info.examItem.Skip(i * RowCount).Take(RowCount))
                    {
                        var row = new TableRow();

                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemName))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemAbbr))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRealValue))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(ParseMark(item.itemMark)))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemUnits))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.itemRefRange))));
                        row.Cells.Add(new TableCell(new Paragraph(new Run(item.extend))));

                        rowGroup1.Rows.Add(row);
                    }
                    while (rowGroup1.Rows.Count < RowCount)
                        rowGroup1.Rows.Add(new TableRow());
                    Print(window);
                }
            }
            info.printTimes = "1";
        }

        private void Print(Report window)
        {
            var manager = GetInstance<IConfigurationManager>();
            var 激光 = manager.GetValue("Printer:Laser");
            new PrintHelperEx(激光).Print(window.doc, "检验报告单", drawHeader: Header, landscape:true);
        }

        private void Header(DrawingContext context, Rect bounds, int pageNr, int pageSize)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = ResourceEngine.GetImageResourceUri("1103");
            image.EndInit();
            context.DrawImage(image,new Rect(new Point(40, 11),new Size(80, 80)));
        }

        private string ParseMark(string mark)
        {
            if (mark.Contains("高"))
                return "↑";
            if (mark.Contains("低"))
                return "↓";
            return string.Empty;
        }

        private string GetTips(检验基本信息 info)
        {
            string t = "本报告仅对所测标本负责。如有疑问请向本实验室咨询。";//电话：63502965，内线：6420\\6534
            XmlDocument conxml = new XmlDocument();
            conxml.Load(@"CurrentResource\YuanTu.ChongQingArea\化验单提示.xml");
            foreach (XmlNode tn in conxml.ChildNodes[1].ChildNodes)
            {
                string locTips = "";
                bool find = false;
                foreach (XmlNode fn in tn.ChildNodes)
                {
                    if (fn.Name == "examType" && fn.InnerText != info.examType) break;
                    if (fn.Name == "sampleType" && fn.InnerText != info.sampleType) break;
                    if (fn.Name == "sendDept" && fn.InnerText != info.sendDept) break;
                    if (fn.Name == "itemType" && fn.InnerText != info.itemType) break;
                    if (fn.Name == "machineNum" && fn.InnerText != info.machineNum) break;
                    if (fn.Name == "TipsValue") locTips = fn.InnerText;
                    find = true;
                }
                if (find)
                {
                    t = locTips;
                    break;
                }
            }

            return t;
        }

        private ImageSource GetRemoteImage(string name)
        {
            try
            {
                Logger.Main.Info($"[获取电子签名]姓名:{name}");
                using (var http = new HttpClient())
                {
                    var stream = http.GetStreamAsync($"http://192.168.0.43:8001/Home/GetImage?name={name}").Result;
                    var ms=new MemoryStream();
                    stream.CopyTo(ms);
                    if (ms.Length>10)
                    {
                        var img = System.Drawing.Image.FromStream(ms); 
                        return ImageToSouce(img);
                    }
                    Logger.Main.Error($"[获取电子签名]姓名:{name} 内容长度:{ms.Length}");
                }
            }
            catch (Exception e)
            {
                Logger.Main.Error($"[获取电子签名]姓名:{name} 异常内容:{e}");
                return null;
            }
            return null;
        }

        private ImageSource ImageToSouce(System.Drawing.Image img)
        {
            if (img == null)
            {
                return null;
            }
            var stream = new MemoryStream();
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                var imgConverter = new ImageSourceConverter();

                return (ImageSource)imgConverter.ConvertFrom(stream);
            }


        }
    }
}