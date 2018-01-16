using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShenZhenArea.Models
{
    //[
    //    {
    //        "RecLocDesc":"肿瘤科门诊",
    //        "RecLocAddress":"门诊楼2楼",
    //        "RecLocCode":"103",
    //        "OEOrdItem":[
    //            {
    //            "UOM":"次",
    //                "Qty":"1",
    //                "InsuType":"",
    //                "ArcmiDesc":"普通门诊诊查费",
    //                "OEOrdId":""
    //        }
    //    ]
    //},
    //{
    //    "RecLocDesc":"西(中成)药房",
    //    "RecLocAddress":"门诊楼1楼",
    //    "RecLocCode":"614",
    //    "OEOrdItem":[
    //        {
    //            "UOM":"片",
    //            "PayorShareAmt":"0",
    //            "Price":".032",
    //            "Qty":"1",
    //            "InsuType":"",
    //            "ArcmiDesc":"维生素B2片(5mg*100片",
    //            "PatientShareAmt":".03",
    //            "TotalAmount":".03",
    //            "OEOrdId":"",
    //            "DiscAmount":"0"
    //        }
    //    ]
    //}
    //]
    /// <summary>
    /// 结算后凭条的Model
    /// </summary>
    public class OEOrdFreeModel
    {
        public string RecLocAddress { set; get; }
        public string RecLocDesc { set; get; }
        public string RecLocCode { set; get; }
        public List<OEOrdFreItemModel> OEOrdItem { set; get; }
    }

    /// <summary>
    /// 结算后凭条的子Model
    /// </summary>
    public class OEOrdFreItemModel
    {
        public string PatientShareAmt { set; get; }
        public string PayorShareAmt { set; get; }
        public string InsuType { set; get; }
        public string TotalAmount { set; get; }
        public string OEOrdId { set; get; }
        public string DiscAmount { set; get; }
        
        public string Qty { set; get; }
        public string Price { set; get; }
        public string ArcmiDesc { set; get; }
        public string UOM { set; get; }

    }
}
