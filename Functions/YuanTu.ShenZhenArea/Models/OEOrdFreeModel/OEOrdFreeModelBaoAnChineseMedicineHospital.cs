using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShenZhenArea.Models
{
    //    [
    //     {
    //        "RecLocDesc":"耳鼻喉科门诊",
    //         "OEOrdItem":[
    //             {
    //                "UOM":"次",
    //                 "Qty":"1",
    //                 "InsuType":"",
    //                 "ArcmiDesc":"挂号费",
    //                 "OEOrdId":""
    //            },
    //            {
    //                "UOM":"次",
    //                "Qty":"1",
    //                "InsuType":"",
    //                "ArcmiDesc":"副主任医师门诊诊查费",
    //                "OEOrdId":""
    //            }
    //        ],
    //        "RecLocAddress":"门诊楼3楼",
    //        "RecLocCode":"191"
    //    },
    //    {
    //        "RecLocDesc":"门诊中药房",
    //        "TMPZYPDesc3":"取药号 : 229",
    //        "TMPZYPDesc4":"注意:请于7天内取药,过期不补,一经发出不得退换",
    //        "OEOrdItem":[
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"75",
    //                "InsuType":"",
    //                "ArcmiDesc":"山药[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"25",
    //                "InsuType":"",
    //                "ArcmiDesc":"辛夷[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"25",
    //                "InsuType":"",
    //                "ArcmiDesc":"炒苍耳子[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"50",
    //                "InsuType":"",
    //                "ArcmiDesc":"龙眼肉[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"25",
    //                "InsuType":"",
    //                "ArcmiDesc":"荆芥穗[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"75",
    //                "InsuType":"",
    //                "ArcmiDesc":"薏苡仁[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"25",
    //                "InsuType":"",
    //                "ArcmiDesc":"桔梗[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"50",
    //                "InsuType":"",
    //                "ArcmiDesc":"炒白扁豆[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"25",
    //                "InsuType":"",
    //                "ArcmiDesc":"炙甘草[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"50",
    //                "InsuType":"",
    //                "ArcmiDesc":"茯苓[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"50",
    //                "InsuType":"",
    //                "ArcmiDesc":"党参片[10g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            },
    //            {
    //                "UOM":"g",
    //                "PayorShareAmt":"0",
    //                "Price":".068",
    //                "Qty":"50",
    //                "InsuType":"",
    //                "ArcmiDesc":"麸炒白术[g](自煎)",
    //                "PatientShareAmt":"3.4",
    //                "TotalAmount":"3.4",
    //                "OEOrdId":"",
    //                "DiscAmount":"0"
    //            }
    //        ],
    //        "RecLocAddress":"门诊楼1楼",
    //        "RecLocCode":"613",
    //        "TMPZYPDesc1":"请到门诊中药房候药区,当显示屏显示你的姓名",
    //        "TMPZYPDesc2":"请到 草药发药3 号窗口取药"
    //    }
    //]

    /// <summary>
    /// 结算后凭条的Model
    /// </summary>
    public class OEOrdFreeModelBaoAnChineseMedicineHospital
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string RecLocAddress { set; get; }
        /// <summary>
        /// 接受科室
        /// </summary>
        public string RecLocDesc { set; get; }
        /// <summary>
        /// 科室编码
        /// </summary>
        public string RecLocCode { set; get; }

        /// <summary>
        /// 药品详细
        /// </summary>
        public List<OEOrdFreItemModelBaoAnChineseMedicineHospital> OEOrdItem { set; get; }

        /// <summary>
        /// 提醒
        /// </summary>
        public string TMPZYPDesc1 { set; get; }
        /// <summary>
        /// 提醒2
        /// </summary>
        public string TMPZYPDesc2 { set; get; }
        /// <summary>
        /// 提醒3  取药号
        /// </summary>
        public string TMPZYPDesc3 { set; get; }
        /// <summary>
        /// 提醒4  超过7天不发药的提醒
        /// </summary>
        public string TMPZYPDesc4 { set; get; }

        /// <summary>
        /// 中药的剂数
        /// </summary>
        public string PresNonum { set; get; }
        

    }

    /// <summary>
    /// 结算后凭条的子Model
    /// </summary>
    public class OEOrdFreItemModelBaoAnChineseMedicineHospital
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

        /// <summary>
        /// 项目类别
        /// </summary>
        public string xmlb { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public string tariCode { get; set; }

    }
}
