namespace YuanTu.YuHangSecondHospital.NativeService.Dto
{
    /// <summary>
    /// 预结算响应结果
    /// </summary>
    public class PerRegisterPay
    {
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 付款总和(元)
        /// </summary>
        public string TotoalPay { get; set; }
        /// <summary>
        /// 实际付款(元)
        /// </summary>
        public string ActualPay { get; set; }
        /// <summary>
        /// 优惠金额(元)
        /// </summary>
        public string DiscountPay { get; set; }
        /// <summary>
        /// 医保金额(元)
        /// </summary>
        public string HealthCarePay { get; set; }
        /// <summary>
        /// 院内账户余额(元)
        /// </summary>
        public string HospitalBalance { get; set; }
        /// <summary>
        /// 市民卡余额(元)
        /// </summary>
        public string CitizenCardBalance { get; set; }
        /// <summary>
        /// 预结算ID
        /// </summary>
        public string PreRegisterId { get; set; }
    }
}