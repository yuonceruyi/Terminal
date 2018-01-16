using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices.Dto
{
    public class PatientBalanceRequest:RequestBase
    {
        public override int BussinessType { get; set; } = 9;

    }
}
