using System;

namespace YuanTu.TaiZhouCentralHospital.HealthInsurance
{
    public class SiConfig
    {
        public static string HospitalCode { get; set; }
        public static int TransSeq { get; set; }

        public static bool InitializeSuccess { get; set; }
        public static DateTime InitializeDate { get; set; }
    } 
}
