using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.HealthInsurance
{
    public class SiConfig
    {
        public static string HospitalCode { get; set; }
        public static int TransSeq { get; set; }

        public static bool InitializeSuccess { get; set; }
        public static DateTime InitializeDate { get; set; }
    } 
}
