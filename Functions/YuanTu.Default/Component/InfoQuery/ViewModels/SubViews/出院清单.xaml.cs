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
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.Default.Component.InfoQuery.ViewModels.SubViews
{
    /// <summary>
    /// 出院清单.xaml 的交互逻辑
    /// </summary>
    public partial class 出院清单 : UserControl
    {
        public 出院清单()
        {
            InitializeComponent();
        }
        public void PrintFromData(  string bizc200101Content)
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
            res["打印时间"] = DateTimeCore.Now.ToString("yyyy年MM月dd日");

            if (!string.IsNullOrWhiteSpace(bizc200101Content))
            {
                var xdoc = XDocument.Parse(bizc200101Content);
                res["结算员"] = FrameworkConst.OperatorId;
                #region[个人信息
                var info = xdoc.Descendants("info").FirstOrDefault()?.Descendants("row1").FirstOrDefault();
                if (info != null)
                {
                    res["医保住院登记号"] = info.Element("serial_no")?.Value;
                    res["姓名"] = info.Element("name")?.Value;
                    res["性别"] = info.Element("sex")?.Value;
                    res["出生日期"] = info.Element("birthday")?.Value;
                    res["个人电脑号"] = info.Element("indi_id")?.Value;
                    res["人员类别"] = info.Element("pers_name")?.Value;
                    res["单位名称"] = info.Element("corp_name")?.Value;
                    res["公务员级别"] = info.Element("official_name")?.Value;
                    res["联系电话"] = info.Element("telephone")?.Value;
                    res["身份证号"] = info.Element("idcard")?.Value;
                    res["住院号"] = info.Element("patient_id")?.Value;
                    res["科别"] = info.Element("in_dept_name")?.Value;
                    res["床号"] = "";/*info.Element("sex")?.Value;*/
                    res["住院时间"] = info.Element("begin_date")?.Value;
                    res["出院时间"] = info.Element("end_date")?.Value;
                    res["住院天数"] = info.Element("days")?.Value;
                    res["入院第一诊断"] = info.Element("in_disease")?.Value;
                    res["出院第一诊断"] = info.Element("fin_disease")?.Value;
                    res["待遇类型"] = info.Element("treatment_name")?.Value;
                    res["账户余额"] = "";/*info.Element("sex")?.Value;*/
                    res["结算时间"] = info.Element("fin_date")?.Value;
                    //res["主管医师"] = info.Element("fin_man")?.Value;

                    //res["本年住院次数"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                    //res["性别"] = info.Element("sex")?.Value;
                }
                #endregion

                #region[年度信息]


                var his = xdoc.Descendants("his").FirstOrDefault()?.Descendants("row1").FirstOrDefault();
                var scene = xdoc.Descendants("scene").FirstOrDefault()?.Nodes().ToArray();
                if (his != null)
                {
                    res["本年住院次数"] = his.Element("cur_biz_times")?.Value;
                }
                if (scene != null)
                {
                    res["本年度分段计算费用累计"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "year_cumulate_ih") as
                            XElement)?.Element("scene_value").Value;
                    res["医疗费合计"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == " inhosp_fee") as
                            XElement)?.Element("scene_value").Value;
                    res["已付起付线"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "year_sill_120c") as
                            XElement)?.Element("scene_value").Value;
                    res["统筹支付"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "D0010") as
                            XElement)?.Element("scene_value").Value;
                    res["完全政策自付"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "ZF_A") as
                            XElement)?.Element("scene_value").Value;
                    res["部分政策自付"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "ZF_P") as
                            XElement)?.Element("scene_value").Value;
                    res["分段比例自付"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "ZF_S") as
                            XElement)?.Element("scene_value").Value;
                    res["大病支付"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "D2010") as
                            XElement)?.Element("scene_value").Value;
                    res["公务员补助费"] = (scene.FirstOrDefault(p => ((XElement)p).Element("scene_code")?.Value == "D3010") as
                            XElement)?.Element("scene_value").Value;

                }
                #endregion

                #region[当前费用类别]

                var feetotalFee = 0m;
                var feetotalSelf = 0m;
                var feepartSelf = 0m;
                var fees = xdoc.Descendants("fee").FirstOrDefault()?.Nodes().ToArray();
                if (fees != null)
                {

                    for (var i = 0; i < fees.Length; i += 3)
                    {
                        var row = new TableRow();
                        for (int j = 0; j < 3; j++)
                        {
                            var index = i + j;
                            if (fees.Length <= index)
                            {
                                row.Cells.Add(new TableCell(new Paragraph()));
                                row.Cells.Add(new TableCell(new Paragraph()));
                                row.Cells.Add(new TableCell(new Paragraph()));
                                row.Cells.Add(new TableCell(new Paragraph()));
                                continue;
                            }
                            var item = fees[index];
                            var zfy = ((XElement)item).Element("zfy")?.Value;
                            var blzf = ((XElement)item).Element("blzf")?.Value;
                            var stat_name = ((XElement)item).Element("stat_name")?.Value;
                            //var stat_type = ((XElement)item).Element("stat_type")?.Value;
                            var qzf = ((XElement)item).Element("qzf")?.Value;
                            feetotalFee += decimal.Parse(zfy ?? "0");
                            feetotalSelf += decimal.Parse(qzf ?? "0");
                            feepartSelf += decimal.Parse(blzf ?? "0");
                            row.Cells.Add(new TableCell(new Paragraph(new Run(stat_name))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(zfy))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(qzf))));
                            row.Cells.Add(new TableCell(new Paragraph(new Run(blzf))));
                        }
                        ContentRows.Rows.Add(row);
                    }

                    //添加几个空行
                    for (int i = 0; i < 4; i++)
                    {
                        var emptyRow = new TableRow();
                        for (int j = 0; j < 12; j++)
                        {
                            emptyRow.Cells.Add(new TableCell(new Paragraph()));
                        }
                        ContentRows.Rows.Add(emptyRow);
                    }
                    res["总费用合计"] = feetotalFee.ToString("F2");
                    res["完全自付合计"] = feetotalSelf.ToString("F2");
                    res["部分自付合计"] = feepartSelf.ToString("F2");
                }
                #endregion

                #region[本次业务政策自付和分段信息]

                var segTotalCash = 0m;
                var segTotalSelf = 0m;
                var segTotalTongChou = 0m;
                var segTotalGongWuYuan = 0m;
                var segTotalYiYuan = 0m;
                var segTotal = 0m;
                var segFund2rd = 0m;
                var segZhaogu = 0m;
                var seg = xdoc.Descendants("seg").FirstOrDefault()?.Nodes().ToArray();
                for (int i = 0; i < seg.Length; i++)
                {
                    var element = seg[i] as XElement;
                    if (element == null)
                    {
                        continue;
                    }
                    var row = new TableRow();
                    var name = element.Element("policy_type")?.Value;
                    var total = decimal.Parse(element.Element("total_pay")?.Value ?? "0");
                    var cash = decimal.Parse(element.Element("cash_pay")?.Value ?? "0");
                    var cashpect = total == 0m ? "0" : (cash / total).ToString("0.00%");
                    var acct = decimal.Parse(element.Element("acct_pay")?.Value ?? "0");
                    var acctpect = total == 0m ? "0" : (acct / total).ToString("0.00%");
                    var basepay = decimal.Parse(element.Element("base_pay")?.Value ?? "0");
                    var basepect = total == 0m ? "0" : (basepay / total).ToString("0.00%");
                    var officia = decimal.Parse(element.Element("official_pay")?.Value ?? "0");
                    var officiapect = total == 0m ? "0" : (officia / total).ToString("0.00%");
                    var hosp = decimal.Parse(element.Element("hosp_pay")?.Value ?? "0");

                    var fund2 = decimal.Parse(element.Element("fund402_pay")?.Value ?? "0");
                    var dabing = decimal.Parse(element.Element("zhaogu_pay")?.Value ?? "0");

                    segTotalCash += cash;
                    segTotalSelf += acct;
                    segTotalTongChou += basepay;
                    segTotalGongWuYuan += officia;
                    segTotalYiYuan += hosp;
                    segFund2rd += fund2;
                    segZhaogu += dabing;
                    segTotal += total;

                    row.Cells.Add(new TableCell(new Paragraph(new Run(name))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(cash == 0m ? "" : cash.ToString("F2")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(cash == 0m ? "" : cashpect))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(acct == 0m ? "" : acct.ToString("F2")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(acct == 0m ? "" : acctpect))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(basepay == 0m ? "" : basepay.ToString("F2")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(basepay == 0m ? "" : basepect))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(officia == 0m ? "" : officia.ToString("F2")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(officia == 0m ? "" : officiapect))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(hosp == 0 ? "" : hosp.ToString("F2")))));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(total == 0 ? "" : total.ToString("F2")))));
                    ContentInsurRows.Rows.Add(row);
                }
                res["现金支付总计"] = segTotalCash == 0m ? "" : segTotalCash.ToString("F2");
                res["个人账户支付总计"] = segTotalSelf == 0m ? "" : segTotalSelf.ToString("F2");
                res["统筹基金支付总计"] = segTotalTongChou == 0m ? "" : segTotalTongChou.ToString("F2");
                res["公务员补助总计"] = segTotalGongWuYuan == 0m ? "" : segTotalGongWuYuan.ToString("F2");
                res["医院支付总计"] = segTotalYiYuan == 0m ? "" : segTotalYiYuan.ToString("F2");
                res["合计总计"] = segTotal.ToString("F2");


                var dict = new Dictionary<string, string>
                {
                    ["本次医疗总费用"] = "total_pay",
                    ["现金支付金额"] = "cash_pay",
                    ["个人账户支付金额"] = "self_pay",
                    ["其中差别化支付"] = "self_pay_exceed",
                    ["基本统筹基金支付"] = "fund_pay",
                    ["大病互助基金支付"] = "db_pay",
                    ["公务员补助支付"] = "official_pay",
                    ["公务员特殊补贴"] = "additional_pay_offi",
                    ["大病补助基金支付"] = "大病补助基金支付",
                    ["医院支付"] = "hosp_pay",
                    ["二次补助基金"] = "大病补助基金支付"
                };
                foreach (var kv in dict)
                {
                    res[kv.Key] = "0.00";
                    res[kv.Key + "大写"] = "零元整";
                }
                res["本次医疗总费用"] = segTotal.ToString("F2");
                res["本次医疗总费用大写"] = (segTotal * 100).In大写();

                res["现金支付金额"] = segTotalCash == 0m ? "0.00" : segTotalCash.ToString("F2");
                res["现金支付金额大写"] = segTotalCash == 0m ? "零元整" : (segTotalCash * 100).In大写();

                res["基本统筹基金支付"] = segTotalTongChou == 0m ? "0.00" : segTotalTongChou.ToString("F2");
                res["基本统筹基金支付大写"] = segTotalTongChou == 0m ? "零元整" : (segTotalTongChou * 100).In大写();

                res["公务员补助支付"] = segTotalGongWuYuan == 0m ? "0.00" : segTotalGongWuYuan.ToString("F2");
                res["公务员补助支付大写"] = segTotalGongWuYuan == 0m ? "零元整" : (segTotalGongWuYuan * 100).In大写();

                res["大病补助基金支付"] = segZhaogu == 0m ? "0.00" : segZhaogu.ToString("F2");
                res["大病补助基金支付大写"] = segZhaogu == 0m ? "零元整" : (segZhaogu * 100).In大写();

                res["医院支付"] = segTotalYiYuan == 0m ? "0.00" : segTotalYiYuan.ToString("F2");
                res["医院支付大写"] = segTotalYiYuan == 0m ? "零元整" : (segTotalYiYuan * 100).In大写();

                res["二次补助基金"] = segFund2rd == 0m ? "0.00" : segFund2rd.ToString("F2");
                res["二次补助基金大写"] = segFund2rd == 0m ? "零元整" : (segFund2rd * 100).In大写();


                #endregion

                //if (chaKaModel != null && chaKaModel.住院患者信息 != null)
                //{
                //    var totalb = decimal.Parse(chaKaModel.住院患者信息.accBalance);
                //    res["已交的预缴款"] = (totalb / 100).ToString("F2");
                //    res["已交的预缴款大写"] = totalb.In大写();
                //    var selfp = decimal.Parse(result?.selfAmount ?? "0") - totalb;
                //    res["应交金额"] = (selfp / 100).ToString("F2");
                //    res["应交金额大写"] = selfp.In大写();


                //}


            }
            var manager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var 激光 = manager.GetValue("Printer:Laser");
            new PrintHelperEx(激光).Print(doc, "出院清单");
        }
    }
}
