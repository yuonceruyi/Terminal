using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class GetTicketCheckout:PerGetTicketCheckout
    {
        /// <summary>
        /// 就诊地点
        /// </summary>
        public string VisitingLocation { get; set; }
    }
}
