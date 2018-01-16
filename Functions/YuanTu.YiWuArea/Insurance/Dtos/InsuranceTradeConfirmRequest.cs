using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    /// <summary>
    /// 交易结果确认请求
    /// </summary>
    public class InsuranceTradeConfirmRequest
    {
        /// <summary>
        /// 交易号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 交易确认结果 0未确认 1交易成功 -1交易失败
        /// </summary>
        public int ConfirmResult { get; set; }

        /// <summary>
        /// 确认交易请求
        /// </summary>
        public string ConfirmRequest { get; set; }
        /// <summary>
        /// 确认交易返回结果
        /// </summary>
        public string ConfirmResponse { get; set; }
        /// <summary>
        /// 确认交易的原因
        /// </summary>
        public string Reason { get; set; }
    }
}
