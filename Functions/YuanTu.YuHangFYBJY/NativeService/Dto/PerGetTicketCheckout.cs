namespace YuanTu.YuHangFYBJY.NativeService.Dto
{
    public class PerGetTicketCheckout
    {
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string PatientName { get; set; }
        /// <summary>
        /// 合计费用(元)
        /// </summary>
        public string TotalPay { get; set; }
        /// <summary>
        /// 自付金额(元)
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
        ///// <summary>
        ///// 舍入金额(元)
        ///// </summary>
        //public string RoundingPay { get; set; }
        /// <summary>
        /// 院内账户余额(元)
        /// </summary>
        public string HospitalBalance { get; set; }
        /// <summary>
        /// 市民卡余额(元)
        /// </summary>
        public string CitizenCardBalance { get; set; }
        /// <summary>
        /// 挂号ID
        /// </summary>
        public string RegisterId { get; set; }
        /// <summary>
        /// 挂号序号
        /// </summary>
        public string RegisterOrder { get; set; }
    }
}
