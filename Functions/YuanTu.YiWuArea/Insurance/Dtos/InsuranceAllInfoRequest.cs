using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    public class InsuranceAllInfoRequest
    {


        /// <summary>
        /// 交易名称
        /// </summary>
        public string TradeName { get; set; }
        /// <summary>
        /// 每次交易的交易号
        /// </summary>
        public string TradeCode { get; set; }
        /// <summary>
        /// 交易入参
        /// </summary>
        public string TradeInput { get; set; }
        /// <summary>
        /// 交易返回码
        /// </summary>
        public int TradeRet { get; set; }
        /// <summary>
        /// 交易出参
        /// </summary>
        public string TradeResult { get; set; }
        /// <summary>
        /// 发起操作的IP
        /// </summary>
        public string IpAddress { get; set; }
    }
}
