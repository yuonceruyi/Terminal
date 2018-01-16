using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YiWuFuBao.Models
{
    public class ChuYuanPayFee
    {
        /// <summary>
        /// 预交金充值总额
        /// </summary>
        public string prepaidAmount { get; set; }
        /// <summary>
        /// 费用总额
        /// </summary>
        public string amount { get; set; }
        public string cashAmount { get; set; }
        public string discountAmount { get; set; }
        /// <summary>
        /// 报销
        /// </summary>
        public string expenseAmount { get; set; }
        /// <summary>
        /// 冻结
        /// </summary>
        public string freezeAmount { get; set; }
        public int hosAccBalance { get; set; }
        /// <summary>
        /// 医院承担
        /// </summary>
        public string hospitalPay { get; set; }
        public int insuranceAmount { get; set; }
        public int selfAmount { get; set; }
        /// <summary>
        /// 自理
        /// </summary>
        public string selffee { get; set; }
        /// <summary>
        /// 自付
        /// </summary>
        public string selfpay { get; set; }
    }
}

