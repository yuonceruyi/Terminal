using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuArea.Insurance.Models.Base
{
    /// <summary>
    /// 读卡请求
    /// </summary>
    public abstract class InsuranceRequestBase
    {
        public abstract string TradeName { get; }
        /// <summary>
        /// 0：无医保卡    1：医保卡  3：一卡通
        /// </summary>
        public int 是否有医保卡 { get; set; } = 1;

        public abstract string Ic信息 { get; set; }

        /// <summary>
        /// 1现金 2电子钱包 3银行卡 其它值默认为1
        /// </summary>
        public int 现金支付方式 { get; set; }

        public string 银行卡信息 { get; set; }

        public string BuildRequest()
        {
            return $"$${是否有医保卡}~{Ic信息}~{现金支付方式}~{银行卡信息}~{DataFormat()}$$";
        }

        public abstract string DataFormat();

    }
}
