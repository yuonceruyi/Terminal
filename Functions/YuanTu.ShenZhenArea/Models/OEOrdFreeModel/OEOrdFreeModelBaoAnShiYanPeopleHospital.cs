using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShenZhenArea.Models
{


/*
    [
        {
            "WinInfo":"请到专科发药01号窗口取药",
            "OEOrdItem":[
                {
                    "UOM":"片",
                    "xmlb":"医保",
                    "tariCode":"20095",
                    "Price":"0.05",
                    "Qty":"1",
                    "ArcmiDesc":"维生素C片[0.1g*100片/瓶]#",
                    "Amt":".05元"
                },
                {
                    "UOM":"粒",
                    "xmlb":"医保",
                    "tariCode":"20263",
                    "Price":"0.05",
                    "Qty":"1",
                    "ArcmiDesc":"维生素B2片[5mg*100粒/瓶]#",
                    "Amt":".05元"
                },
                {
                    "UOM":"片",
                    "xmlb":"医保",
                    "tariCode":"20102",
                    "Price":"0.06",
                    "Qty":"1",
                    "ArcmiDesc":"维生素B1片[10mg*100片/瓶]#",
                    "Amt":".06元"
                }
            ],
            "Adress":"综合楼1楼门诊专科药房-请到专科发药01号窗口取药",
            "locDesc":"门诊专科药房"
        }
    ]

*/

    /// <summary>
    /// 结算后凭条的Model
    /// </summary>
    public class OEOrdFreeModelBaoAnShiYanPeopleHospital
    {
        public string Adress { set; get; }
        public string locDesc { set; get; }
        public List<OEOrdFreItemModelBaoAnShiYanPeopleHospital> OEOrdItem { set; get; }

        public string WinInfo { set; get; }
    }

    /// <summary>
    /// 结算后凭条的子Model
    /// </summary>
    public class OEOrdFreItemModelBaoAnShiYanPeopleHospital
    {
        public string Amt { set; get; }
        public string Qty { set; get; }
        public string Price { set; get; }
        public string ArcmiDesc { set; get; }
        public string UOM { set; get; }
        public string tariCode { set; get; }

        public string xmlb { get; set; }
    }
}
