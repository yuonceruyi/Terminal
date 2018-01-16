namespace YuanTu.YuHangSecondHospital.NativeService.Dto
{
    public class PerCheckout
    {
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 费用合计(元)
        /// </summary>
        public string TotalPay { get; set; }
        /// <summary>
        /// 医保支付金额(元)
        /// </summary>
        public string HealthCarePay { get; set; }
        /// <summary>
        /// 优惠金额(元)
        /// </summary>
        public string DiscountPay { get; set; }
        /// <summary>
        /// 实际支付(元)
        /// </summary>
        public string ActualPay { get; set; }
        /// <summary>
        /// 舍入金额(元)
        /// </summary>
        public string RoundingPay { get; set; }
        /// <summary>
        /// 院内账户余额(元)
        /// </summary>
        public string HospitalBalance { get; set; }
        /// <summary>
        /// 市民卡余额(元)
        /// </summary>
        public string CitienCardBalance { get; set; }
        /// <summary>
        /// 医技Id
        /// </summary>
        public string[] DoctorTechIds { get; set; }
        /// <summary>
        /// 处方Id
        /// </summary>
        public string PrescriptionId { get; set; }
    }
}
