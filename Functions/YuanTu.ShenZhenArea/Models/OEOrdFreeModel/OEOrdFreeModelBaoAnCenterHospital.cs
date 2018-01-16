using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.ShenZhenArea.Models
{
        //[
        //    {
        //    "OEOrdItem":[
        //            {
        //            "UOM":"次",
        //                "Price":"0.00",
        //                "Qty":"1",
        //                "ArcmiDesc":"挂号费",
        //                "Amt":"0元"
        //        }
        //    ],
        //    "Adress":"门诊楼2楼-",
        //    "locDesc":"眼科门诊"
        //},
        //{
        //    "WinInfo":"请到发药3号窗口取药",
        //    "OEOrdItem":[
        //        {
        //            "UOM":"粒",
        //            "Price":"0.06",
        //            "Qty":"1",
        //            "ArcmiDesc":"碳酸氢钠片[0.5g*100粒/盒]",
        //            "Amt":".06元",
        //            "XYYBrecDepDesc":"未发药"
        //        }
        //    ],
        //    "Adress":"门诊楼1楼-请到发药3号窗口取药",
        //    "locDesc":"西药房"
        //}
        //]


    /// <summary>
    /// 结算后凭条的Model
    /// </summary>
    public class OEOrdFreeModelBaoAnCenterHospital
    {
        /// <summary>
        /// 所在地址
        /// </summary>
        public string Adress { set; get; }
        /// <summary>
        /// 接受科室
        /// </summary>
        public string locDesc { set; get; }

        /// <summary>
        /// 发药窗口
        /// </summary>
        public string WinInfo { set; get; }

        
        public List<OEOrdFreItemModelBaoAnCenterHospital> OEOrdItem { set; get; }
    }

    /// <summary>
    /// 结算后凭条的子Model
    /// </summary>
    public class OEOrdFreItemModelBaoAnCenterHospital
    {
        /// <summary>
        /// 单位
        /// </summary>
        public string UOM { set; get; }
        /// <summary>
        /// 单价
        /// </summary>
        public string Price { set; get; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Qty { set; get; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ArcmiDesc { set; get; }
        /// <summary>
        /// 总价
        /// </summary>
        public string Amt { set; get; }
        /// <summary>
        /// 发药状态
        /// </summary>
        public string XYYBrecDepDesc { set; get; }

    }
}
