using System;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    /// <summary>
    /// 自付比例上传
    /// </summary>
    public class SelfRatioRequest
    {
        /// <summary>
        /// 医嘱明细序号
        /// </summary>	
        public string DetailOrder { get; set; }
        /// <summary>
        /// 药品诊疗类型
        /// </summary>	
        public int TradeType { get; set; }
        /// <summary>
        /// 医保编码
        /// </summary>	
        public string InsuranceCode { get; set; }
        /// <summary>
        /// 医院编码
        /// </summary>	
        public string HospitalCode { get; set; }
        /// <summary>
        /// 限制类标志
        /// </summary>	
        public int RestrictType { get; set; }
        /// <summary>
        /// 自付比例
        /// </summary>	
        public decimal SelfRatio { get; set; }
        /// <summary>
        /// 进口自付比例
        /// </summary>	
        public decimal ImportSelfRatio { get; set; }
        /// <summary>
        /// 医嘱时间
        /// </summary>	
        public DateTime OrderDate { get; set; }
    }
}