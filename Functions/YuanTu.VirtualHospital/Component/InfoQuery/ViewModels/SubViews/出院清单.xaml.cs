using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.VirtualHospital.Component.InfoQuery.ViewModels.SubViews
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
        public void PrintFromData()
        {
            var bizc200101Content = xml;
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

        string xml= "<?xml version=\"1.0\" encoding=\"GBK\"?><Program><ErrorNo>1</ErrorNo><ErrorMessage></ErrorMessage><ErrorType>0</ErrorType><parameters></parameters><fee><row1><zfy>3450.89</zfy><blzf>306.62</blzf><stat_name>西药费</stat_name><stat_type>001</stat_type><qzf>0</qzf></row1><row2><zfy>41500.1</zfy><blzf>365.87</blzf><stat_name>材料费</stat_name><stat_type>004</stat_type><qzf>39389.75</qzf></row2><row3><zfy>1299</zfy><blzf>0</blzf><stat_name>治疗费</stat_name><stat_type>005</stat_type><qzf>0</qzf></row3><row4><zfy>77</zfy><blzf>38.5</blzf><stat_name>输氧费</stat_name><stat_type>007</stat_type><qzf>0</qzf></row4><row5><zfy>30</zfy><blzf>0</blzf><stat_name>诊察费</stat_name><stat_type>008</stat_type><qzf>0</qzf></row5><row6><zfy>634</zfy><blzf>0</blzf><stat_name>护理费</stat_name><stat_type>010</stat_type><qzf>465</qzf></row6><row7><zfy>218</zfy><blzf>28.6</blzf><stat_name>检查费</stat_name><stat_type>011</stat_type><qzf>0</qzf></row7><row8><zfy>3519</zfy><blzf>10</blzf><stat_name>化验费</stat_name><stat_type>012</stat_type><qzf>0</qzf></row8><row9><zfy>180</zfy><blzf>36</blzf><stat_name>特检费</stat_name><stat_type>013</stat_type><qzf>0</qzf></row9><row10><zfy>3198</zfy><blzf>40</blzf><stat_name>手术费</stat_name><stat_type>014</stat_type><qzf>400</qzf></row10><row11><zfy>420</zfy><blzf>0</blzf><stat_name>床位费</stat_name><stat_type>017</stat_type><qzf>0</qzf></row11><row12><zfy>497.85</zfy><blzf>0</blzf><stat_name>放射费</stat_name><stat_type>025</stat_type><qzf>0</qzf></row12><row13><zfy>85.1</zfy><blzf>0</blzf><stat_name>其它费</stat_name><stat_type>099</stat_type><qzf>85.1</qzf></row13></fee><seg><row1><sn>2</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>完全政策自付</policy_type><acct_pay>0</acct_pay><policy_type_id>政策自付</policy_type_id><cyh_cash_pay>0</cyh_cash_pay><zhaogu_pay>0</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>40339.85</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>0</cyh_pay><total_pay>40339.85</total_pay><base_pay>0</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row1><row2><sn>3</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>部分政策自付</policy_type><acct_pay>0</acct_pay><policy_type_id>政策自付</policy_type_id><cyh_cash_pay>0</cyh_cash_pay><zhaogu_pay>0</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>825.59</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>0</cyh_pay><total_pay>825.59</total_pay><base_pay>0</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row2><row3><sn>4</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>转外自理费用</policy_type><acct_pay>0</acct_pay><policy_type_id>转外自理</policy_type_id><cyh_cash_pay>0</cyh_cash_pay><zhaogu_pay>0</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>0</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>0</cyh_pay><total_pay>0</total_pay><base_pay>0</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row3><row4><sn>4</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>应付起付线</policy_type><acct_pay>0</acct_pay><policy_type_id>起付标准</policy_type_id><cyh_cash_pay>0</cyh_cash_pay><zhaogu_pay>0</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>1250</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>0</cyh_pay><total_pay>1250</total_pay><base_pay>0</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row4><row5><sn>13</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>统筹三段(1万-16万)</policy_type><acct_pay>0</acct_pay><policy_type_id>统筹段</policy_type_id><cyh_cash_pay>1269.35</cyh_cash_pay><zhaogu_pay>0</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>2538.7</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>1269.35</cyh_pay><total_pay>12693.5</total_pay><base_pay>10154.8</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row5><row6><sn>30</sn><cyh_account_pay>0</cyh_account_pay><fund402_pay>0</fund402_pay><hosp_pay>0</hosp_pay><policy_type>大病补助</policy_type><acct_pay>0</acct_pay><policy_type_id>超大病段</policy_type_id><cyh_cash_pay>0</cyh_cash_pay><zhaogu_pay>2508.71</zhaogu_pay><offi_tsbt>0</offi_tsbt><official_pay_302>0</official_pay_302><gztckj>0</gztckj><xjtckj>0</xjtckj><cash_pay>-2508.71</cash_pay><official_pay>0</official_pay><center_pay>0</center_pay><cyh_pay>334.494666666666666666666666666666666667</cyh_pay><total_pay>0</total_pay><base_pay>0</base_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay></row6></seg><his><row1><cur_biz_times>2</cur_biz_times></row1></his><scene><row1><serial_no>56980204</serial_no><scene_code>D12000</scene_code><bka026>D12000</bka026><scene_value>0.0</scene_value></row1><row2><serial_no>56980204</serial_no><scene_code>D310100</scene_code><bka026>D310100</bka026><scene_value>0.0</scene_value></row2><row3><serial_no>56980204</serial_no><scene_code>D310200</scene_code><bka026>D310200</bka026><scene_value>833.88</scene_value></row3><row4><serial_no>56980204</serial_no><scene_code>D310300</scene_code><bka026>D310300</bka026><scene_value>0.0</scene_value></row4><row5><serial_no>56980204</serial_no><scene_code>D9991</scene_code><bka026>D9991</bka026><scene_value>13215.74</scene_value></row5><row6><serial_no>56980204</serial_no><scene_code>D999998</scene_code><bka026>D999998</bka026><scene_value>54462.26</scene_value></row6><row7><serial_no>56980204</serial_no><scene_code>F12D1</scene_code><bka026>F12D1</bka026><scene_value>0.0</scene_value></row7><row8><serial_no>56980204</serial_no><scene_code>FillSill_301</scene_code><bka026>FillSill_301</bka026><scene_value>0.0</scene_value></row8><row9><serial_no>56980204</serial_no><scene_code>FillSill_999</scene_code><bka026>FillSill_999</bka026><scene_value>0.0</scene_value></row9><row10><serial_no>56980204</serial_no><scene_code>S1200</scene_code><bka026>S1200</bka026><scene_value>12933.5</scene_value></row10><row11><serial_no>56980204</serial_no><scene_code>ZF_A</scene_code><bka026>ZF_A</bka026><scene_value>41633.2</scene_value></row11><row12><serial_no>56980204</serial_no><scene_code>ZF_P</scene_code><bka026>ZF_P</bka026><scene_value>825.59</scene_value></row12><row13><serial_no>56980204</serial_no><scene_code>ZF_S</scene_code><bka026>ZF_S</bka026><scene_value>13683.5</scene_value></row13><row14><serial_no>56980204</serial_no><scene_code>aae140</scene_code><bka026>aae140</bka026><scene_value>310</scene_value></row14><row15><serial_no>56980204</serial_no><scene_code>admin_id</scene_code><bka026>admin_id</bka026><scene_value>7</scene_value></row15><row16><serial_no>56980204</serial_no><scene_code>already_gy_003</scene_code><bka026>already_gy_003</bka026><scene_value>0</scene_value></row16><row17><serial_no>56980204</serial_no><scene_code>already_gy_202</scene_code><bka026>already_gy_202</bka026><scene_value>0</scene_value></row17><row18><serial_no>56980204</serial_no><scene_code>already_mz_001</scene_code><bka026>already_mz_001</bka026><scene_value>0</scene_value></row18><row19><serial_no>56980204</serial_no><scene_code>already_mz_801</scene_code><bka026>already_mz_801</bka026><scene_value>0</scene_value></row19><row20><serial_no>56980204</serial_no><scene_code>already_mztsb_801</scene_code><bka026>already_mztsb_801</bka026><scene_value>0</scene_value></row20><row21><serial_no>56980204</serial_no><scene_code>already_s0c_202</scene_code><bka026>already_s0c_202</bka026><scene_value>0</scene_value></row21><row22><serial_no>56980204</serial_no><scene_code>at_year</scene_code><bka026>at_year</bka026><scene_value>2016</scene_value></row22><row23><serial_no>56980204</serial_no><scene_code>atm_money</scene_code><bka026>atm_money</bka026><scene_value>0.0</scene_value></row23><row24><serial_no>56980204</serial_no><scene_code>benefit_limit</scene_code><bka026>benefit_limit</bka026><scene_value>0</scene_value></row24><row25><serial_no>56980204</serial_no><scene_code>benefit_scale</scene_code><bka026>benefit_scale</bka026><scene_value>0</scene_value></row25><row26><serial_no>56980204</serial_no><scene_code>biz_disease</scene_code><bka026>biz_disease</bka026><scene_value>S82.212</scene_value></row26><row27><serial_no>56980204</serial_no><scene_code>biz_month</scene_code><bka026>biz_month</bka026><scene_value>08</scene_value></row27><row28><serial_no>56980204</serial_no><scene_code>biz_times</scene_code><bka026>biz_times</bka026><scene_value>2</scene_value></row28><row29><serial_no>56980204</serial_no><scene_code>catalog_center</scene_code><bka026>catalog_center</bka026><scene_value>639900</scene_value></row29><row30><serial_no>56980204</serial_no><scene_code>city_code</scene_code><bka026>city_code</bka026><scene_value>0</scene_value></row30><row31><serial_no>56980204</serial_no><scene_code>city_flag</scene_code><bka026>city_flag</bka026><scene_value>0</scene_value></row31><row32><serial_no>56980204</serial_no><scene_code>cj_clinic_pay_flag</scene_code><bka026>cj_clinic_pay_flag</bka026><scene_value>0</scene_value></row32><row33><serial_no>56980204</serial_no><scene_code>clinic_mztc_801</scene_code><bka026>clinic_mztc_801</bka026><scene_value>0</scene_value></row33><row34><serial_no>56980204</serial_no><scene_code>clinic_pay_flag</scene_code><bka026>clinic_pay_flag</bka026><scene_value>1</scene_value></row34><row35><serial_no>56980204</serial_no><scene_code>corp_mode</scene_code><bka026>corp_mode</bka026><scene_value>2</scene_value></row35><row36><serial_no>56980204</serial_no><scene_code>dATMmoney</scene_code><bka026>dATMmoney</bka026><scene_value>0.0</scene_value></row36><row37><serial_no>56980204</serial_no><scene_code>dYearSpecialMT</scene_code><bka026>dYearSpecialMT</bka026><scene_value>0</scene_value></row37><row38><serial_no>56980204</serial_no><scene_code>first_balance_ln</scene_code><bka026>first_balance_ln</bka026><scene_value>833.88</scene_value></row38><row39><serial_no>56980204</serial_no><scene_code>having_zy</scene_code><bka026>having_zy</bka026><scene_value>0</scene_value></row39><row40><serial_no>56980204</serial_no><scene_code>hosp_district_code</scene_code><bka026>hosp_district_code</bka026><scene_value>639900</scene_value></row40><row41><serial_no>56980204</serial_no><scene_code>hosp_grade</scene_code><bka026>hosp_grade</bka026><scene_value>9</scene_value></row41><row42><serial_no>56980204</serial_no><scene_code>hosp_kind</scene_code><bka026>hosp_kind</bka026><scene_value>10</scene_value></row42><row43><serial_no>56980204</serial_no><scene_code>hosp_level</scene_code><bka026>hosp_level</bka026><scene_value>1</scene_value></row43><row44><serial_no>56980204</serial_no><scene_code>hosp_out_flag</scene_code><bka026>hosp_out_flag</bka026><scene_value>0</scene_value></row44><row45><serial_no>56980204</serial_no><scene_code>hosp_type</scene_code><bka026>hosp_type</bka026><scene_value>1</scene_value></row45><row46><serial_no>56980204</serial_no><scene_code>iBizTimes131</scene_code><bka026>iBizTimes131</bka026><scene_value>0</scene_value></row46><row47><serial_no>56980204</serial_no><scene_code>iContinuesMedi_pay_years</scene_code><bka026>iContinuesMedi_pay_years</bka026><scene_value>0</scene_value></row47><row48><serial_no>56980204</serial_no><scene_code>in_days</scene_code><bka026>in_days</bka026><scene_value>95</scene_value></row48><row49><serial_no>56980204</serial_no><scene_code>indi_join_flag</scene_code><bka026>indi_join_flag</bka026><scene_value>1</scene_value></row49><row50><serial_no>56980204</serial_no><scene_code>inhosp_biz_times</scene_code><bka026>inhosp_biz_times</bka026><scene_value>2</scene_value></row50><row51><serial_no>56980204</serial_no><scene_code>inhosp_biz_times120</scene_code><bka026>inhosp_biz_times120</bka026><scene_value>2</scene_value></row51><row52><serial_no>56980204</serial_no><scene_code>inhosp_biz_times122</scene_code><bka026>inhosp_biz_times122</bka026><scene_value>0</scene_value></row52><row53><serial_no>56980204</serial_no><scene_code>inhosp_fee</scene_code><bka026>inhosp_fee</bka026><scene_value>54848.94</scene_value></row53><row54><serial_no>56980204</serial_no><scene_code>insur_no</scene_code><bka026>insur_no</bka026><scene_value>310</scene_value></row54><row55><serial_no>56980204</serial_no><scene_code>intensive_disability_flag</scene_code><bka026>intensive_disability_flag</bka026><scene_value>0</scene_value></row55><row56><serial_no>56980204</serial_no><scene_code>last_balance</scene_code><bka026>last_balance</bka026><scene_value>0</scene_value></row56><row57><serial_no>56980204</serial_no><scene_code>last_balance_bn</scene_code><bka026>last_balance_bn</bka026><scene_value>0</scene_value></row57><row58><serial_no>56980204</serial_no><scene_code>last_hospital_id</scene_code><bka026>last_hospital_id</bka026><scene_value>0</scene_value></row58><row59><serial_no>56980204</serial_no><scene_code>low_flag</scene_code><bka026>low_flag</bka026><scene_value>0</scene_value></row59><row60><serial_no>56980204</serial_no><scene_code>nothing_flag</scene_code><bka026>nothing_flag</bka026><scene_value>1</scene_value></row60><row61><serial_no>56980204</serial_no><scene_code>out_medicare_flag</scene_code><bka026>out_medicare_flag</bka026><scene_value>0</scene_value></row61><row62><serial_no>56980204</serial_no><scene_code>partself_pay</scene_code><bka026>partself_pay</bka026><scene_value>825.59</scene_value></row62><row63><serial_no>56980204</serial_no><scene_code>payment_months</scene_code><bka026>payment_months</bka026><scene_value>24</scene_value></row63><row64><serial_no>56980204</serial_no><scene_code>posi_code</scene_code><bka026>posi_code</bka026><scene_value>190</scene_value></row64><row65><serial_no>56980204</serial_no><scene_code>C0000</scene_code><bka026>pre_C0000</bka026><scene_value>6</scene_value></row65><row66><serial_no>56980204</serial_no><scene_code>C1000</scene_code><bka026>pre_C1000</bka026><scene_value>2</scene_value></row66><row67><serial_no>56980204</serial_no><scene_code>C1100</scene_code><bka026>pre_C1100</bka026><scene_value>3</scene_value></row67><row68><serial_no>56980204</serial_no><scene_code>C1200</scene_code><bka026>pre_C1200</bka026><scene_value>1</scene_value></row68><row69><serial_no>56980204</serial_no><scene_code>D12000</scene_code><bka026>pre_D12000</bka026><scene_value>0</scene_value></row69><row70><serial_no>56980204</serial_no><scene_code>D0010</scene_code><bka026>pre_D310100</bka026><scene_value>0</scene_value></row70><row71><serial_no>56980204</serial_no><scene_code>D310200</scene_code><bka026>pre_D310200</bka026><scene_value>833.88</scene_value></row71><row72><serial_no>56980204</serial_no><scene_code>D310300</scene_code><bka026>pre_D310300</bka026><scene_value>0</scene_value></row72><row73><serial_no>56980204</serial_no><scene_code>D9991</scene_code><bka026>pre_D9991</bka026><scene_value>13215.74</scene_value></row73><row74><serial_no>56980204</serial_no><scene_code>D999998</scene_code><bka026>pre_D999998</bka026><scene_value>54462.26</scene_value></row74><row75><serial_no>56980204</serial_no><scene_code>F0000</scene_code><bka026>pre_F0000</bka026><scene_value>55296.14</scene_value></row75><row76><serial_no>56980204</serial_no><scene_code>F1000</scene_code><bka026>pre_F1000</bka026><scene_value>143.2</scene_value></row76><row77><serial_no>56980204</serial_no><scene_code>F1100</scene_code><bka026>pre_F1100</bka026><scene_value>304</scene_value></row77><row78><serial_no>56980204</serial_no><scene_code>F1200</scene_code><bka026>pre_F1200</bka026><scene_value>54848.94</scene_value></row78><row79><serial_no>56980204</serial_no><scene_code>F12D1</scene_code><bka026>pre_F12D1</bka026><scene_value>0</scene_value></row79><row80><serial_no>56980204</serial_no><scene_code>Q0000</scene_code><bka026>pre_Q0000</bka026><scene_value>750</scene_value></row80><row81><serial_no>56980204</serial_no><scene_code>Q1200</scene_code><bka026>pre_Q1200</bka026><scene_value>750</scene_value></row81><row82><serial_no>56980204</serial_no><scene_code>S1200</scene_code><bka026>pre_S1200</bka026><scene_value>12933.5</scene_value></row82><row83><serial_no>56980204</serial_no><scene_code>ZF_A</scene_code><bka026>pre_ZF_A</bka026><scene_value>41633.2</scene_value></row83><row84><serial_no>56980204</serial_no><scene_code>ZF_P</scene_code><bka026>pre_ZF_P</bka026><scene_value>825.59</scene_value></row84><row85><serial_no>56980204</serial_no><scene_code>ZF_S</scene_code><bka026>pre_ZF_S</bka026><scene_value>13683.5</scene_value></row85><row86><serial_no>56980204</serial_no><scene_code>reg_info</scene_code><bka026>reg_info</bka026><scene_value>NZ</scene_value></row86><row87><serial_no>56980204</serial_no><scene_code>special_range_use_flag</scene_code><bka026>special_range_use_flag</bka026><scene_value>9</scene_value></row87><row88><serial_no>56980204</serial_no><scene_code>speical_pers_flag</scene_code><bka026>speical_pers_flag</bka026><scene_value>0</scene_value></row88><row89><serial_no>56980204</serial_no><scene_code>urban_type</scene_code><bka026>urban_type</bka026><scene_value>0</scene_value></row89><row90><serial_no>56980204</serial_no><scene_code>year_cumulate_ih</scene_code><bka026>year_cumulate_ih</bka026><scene_value>12933.5</scene_value></row90><row91><serial_no>56980204</serial_no><scene_code>year_cumulate_ih_122</scene_code><bka026>year_cumulate_ih_122</bka026><scene_value>0.0</scene_value></row91><row92><serial_no>56980204</serial_no><scene_code>year_sill_120</scene_code><bka026>year_sill_120</bka026><scene_value>750.0</scene_value></row92><row93><serial_no>56980204</serial_no><scene_code>year_sill_122</scene_code><bka026>year_sill_122</bka026><scene_value>0.0</scene_value></row93><row94><serial_no>56980204</serial_no><scene_code>year_sill_131</scene_code><bka026>year_sill_131</bka026><scene_value>0.0</scene_value></row94><row95><serial_no>56980204</serial_no><scene_code>year_sill_131_s</scene_code><bka026>year_sill_131_s</bka026><scene_value>0</scene_value></row95><row96><serial_no>56980204</serial_no><scene_code>year_sill_131_x</scene_code><bka026>year_sill_131_x</bka026><scene_value>0</scene_value></row96><row97><serial_no>56980204</serial_no><scene_code>year_sill_132</scene_code><bka026>year_sill_132</bka026><scene_value>0.0</scene_value></row97><row98><serial_no>56980204</serial_no><scene_code>year_sill_133</scene_code><bka026>year_sill_133</bka026><scene_value>0.0</scene_value></row98><row99><serial_no>56980204</serial_no><scene_code>year_sill_140</scene_code><bka026>year_sill_140</bka026><scene_value>0.0</scene_value></row99><row100><serial_no>56980204</serial_no><scene_code>year_sill_170</scene_code><bka026>year_sill_170</bka026><scene_value>0.0</scene_value></row100><row101><serial_no>56980204</serial_no><scene_code>year_sill_ih</scene_code><bka026>year_sill_ih</bka026><scene_value>750.0</scene_value></row101></scene><fund><row1><additional_pay_cash>0</additional_pay_cash><hosp_pay>0</hosp_pay><self_pay_seg>2538.7</self_pay_seg><exceed_public_pay>0</exceed_public_pay><acct_pay>0</acct_pay><start_pay>1250</start_pay><additional_pay_offi>0</additional_pay_offi><part_pay_offi>0</part_pay_offi><start_pay_offi>0</start_pay_offi><cash_pay>42445.43</cash_pay><official_pay>0</official_pay><self_pay_exceed>0</self_pay_exceed><official_pay_seg>0</official_pay_seg><self_pay>41609.2</self_pay><qfx_pay>1250</qfx_pay><db_pay>0</db_pay><acct_pay_seg>0</acct_pay_seg><total_pay>55108.94</total_pay><part_pay>825.59</part_pay><base_pay>12693.5</base_pay><declare_pay>13943.5</declare_pay><additional_pay>0</additional_pay><corp_pay>0</corp_pay><fund_pay>10154.8</fund_pay></row1></fund><info><row1><birthday>1990-02-12</birthday><sex>男</sex><fin_info></fin_info><office_grade>00</office_grade><disease>普通疾病</disease><in_dept_name>骨科II</in_dept_name><serial_no>56980204</serial_no><indi_id>1466173</indi_id><in_icd>00000</in_icd><end_date>2016-08-01</end_date><corp_type_name>行政机关</corp_type_name><name>T江浩</name><pers_type>1</pers_type><treatment_type>120</treatment_type><fin_date>2016-08-08 16:54:36</fin_date><serial_apply></serial_apply><in_disease>普通疾病</in_disease><treatment_name>普通住院</treatment_name><fin_disease>胫腓骨干开放性骨折</fin_disease><corp_name>T中国水利水电第四工程局有限公司(10000379)</corp_name><official_name>非公务员</official_name><hospital_name>T青海省人民医院</hospital_name><biz_type>12</biz_type><hosp_level_name>三级</hosp_level_name><reimburse_flag>2</reimburse_flag><fin_man>ZXZZJ</fin_man><hosp_grade_name>无</hosp_grade_name><begin_date>2016-04-28</begin_date><idcard>632521199002120612</idcard><corp_code>2010035</corp_code><ic_no>632521199002120612</ic_no><reg_info>其它</reg_info><last_balance>0</last_balance><days>95</days><center_name>青海省职工医疗保险管理局</center_name><hospital_id>81990013005</hospital_id><in_bed></in_bed><injury_borth_sn></injury_borth_sn><case_id>1</case_id><patient_id>00943812</patient_id><center_id>639900</center_id><telephone>15297211949</telephone><pers_name>在职</pers_name></row1></info></Program>";
    }
}
