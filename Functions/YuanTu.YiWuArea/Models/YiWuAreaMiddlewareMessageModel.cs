using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.DB;

namespace YuanTu.YiWuArea.Models
{
    public class YiWuAreaMiddlewareMessageModel: Table
    {
        public string FullUrl { get; set; }
        public string HttpMethod { get; set; }
        public string Content { get; set; }
        public string FailedMessage { get; set; }

    }

    public class YiWuAreaInsuranceConfirmFailedModel : Table
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

        public string MiddlareFailedMessage { get; set; }
    }
}
