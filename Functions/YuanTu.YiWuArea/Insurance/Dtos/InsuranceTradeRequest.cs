using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;

namespace YuanTu.YiWuArea.Insurance.Dtos
{
    /// <summary>
    /// 社保交易信息
    /// </summary>
    public class InsuranceTradeRequest
    {
        /// <summary>
        /// 交易名称
        /// </summary>
        public string TradeName { get; set; }
        /// <summary>
        /// 交易单据号
        /// </summary>
        public string OrderNo { get; set; }
        /// <summary>
        /// 社保操作员编号
        /// </summary>
        public string SiOperatorId { get; set; }
        /// <summary>
        /// 对账流水号
        /// </summary>
        public string SiToken { get; set; }
        /// <summary>
        /// 每次交易的交易号
        /// </summary>
        public string TradeCode { get; set; }
        /// <summary>
        /// 病人Ic卡信息
        /// </summary>
        public string IcInfo { get; set; }
        /// <summary>
        /// 病人卡号
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 机器操作员编号
        /// </summary>
        public string OperationNo { get; set; } = FrameworkConst.OperatorId;
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
