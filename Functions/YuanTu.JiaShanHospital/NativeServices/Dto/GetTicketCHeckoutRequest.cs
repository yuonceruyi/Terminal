using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class GetTicketCheckoutRequest:PerGetTicketCheckoutRequest
    {
        /// <summary>
        /// 业务类型，预约取号结算为6
        /// </summary>
        public override int BussinessType { get; set; } = 6;

        public string Account { get; set; }
    }
}
