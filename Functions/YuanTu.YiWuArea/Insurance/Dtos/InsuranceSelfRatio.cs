using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    public class InsuranceSelfRatio
    {
        /// <summary>
        /// Id
        /// </summary>	
        public Guid Id { get; set; }
        /// <summary>
        /// DetailOrder
        /// </summary>	
        public string DetailOrder { get; set; }
        /// <summary>
        /// TradeType
        /// </summary>	
        public int TradeType { get; set; }
        /// <summary>
        /// InsuranceCode
        /// </summary>	
        public string InsuranceCode { get; set; }
        /// <summary>
        /// HospitalCode
        /// </summary>	
        public string HospitalCode { get; set; }
        /// <summary>
        /// RestrictType
        /// </summary>	
        public int RestrictType { get; set; }
        /// <summary>
        /// SelfRatio
        /// </summary>	
        public decimal SelfRatio { get; set; }
        /// <summary>
        /// ImportSelfRatio
        /// </summary>	
        public decimal ImportSelfRatio { get; set; }
        /// <summary>
        /// CreateDate
        /// </summary>	
        public DateTime CreatedTime { get; set; }
    }
}
