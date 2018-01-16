using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class RegisterPayRequest:PerRegisterPayRequest
    {
        public override int BussinessType { get; set; } = 4;

        public string Account { get; set; }
    }
}
