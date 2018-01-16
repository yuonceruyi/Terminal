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
        "wininfo":"请到专科发药01号窗口取药",
        "oeorditem":[
            {
                "uom":"片",
                "xmlb":"医保",
                "taricode":"20095",
                "price":"0.05",
                "qty":"1",
                "arcmidesc":"维生素c片[0.1g*100片/瓶]#",
                "amt":".05元"
            },
            {
                "uom":"粒",
                "xmlb":"医保",
                "taricode":"20263",
                "price":"0.05",
                "qty":"1",
                "arcmidesc":"维生素b2片[5mg*100粒/瓶]#",
                "amt":".05元"
            },
            {
                "uom":"片",
                "xmlb":"医保",
                "taricode":"20102",
                "price":"0.06",
                "qty":"1",
                "arcmidesc":"维生素b1片[10mg*100片/瓶]#",
                "amt":".06元"
            }
        ],
        "adress":"综合楼1楼门诊专科药房-请到专科发药01号窗口取药",
        "locdesc":"门诊专科药房"
    }
]
*/

    /// <summary>
    /// 宝安人民医院的结算后凭条的Model
    /// </summary>
    public class OEOrdFreeModelBaoAnPeopleHospital
    {
        public string Adress { set; get; }
        public string locDesc { set; get; }
        public List<OEOrdFreItemModelBaoAnPeopleHospital> OEOrdItem { set; get; }

        public string WinInfo { set; get; }
    }

    /// <summary>
    /// 宝安人民医院的结算后凭条的子Model
    /// </summary>
    public class OEOrdFreItemModelBaoAnPeopleHospital
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



