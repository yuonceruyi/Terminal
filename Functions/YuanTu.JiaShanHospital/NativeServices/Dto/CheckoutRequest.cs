using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class CheckoutRequest:PerCheckoutRequest
    {
        public override int BussinessType { get; set; } = 2;

        public string Account { get; set; }
    }
}
