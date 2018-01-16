using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.YuHangZYY.Component.Models
{
    public class PaymentModel: YuanTu.Consts.Models.Payment.PaymentModel
    {
        /// <summary>
        /// 智慧医疗余额
        /// </summary>
        public decimal CitizenBlance { get; set; }
    }
}
