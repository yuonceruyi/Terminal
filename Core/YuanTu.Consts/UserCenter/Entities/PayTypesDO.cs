namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayTypesDO
    {
        /// <summary>
        ///     门诊充值
        /// </summary>
        public PayType recharge { get; set; }

        /// <summary>
        ///     挂号
        /// </summary>
        public PayType reg { get; set; }

        /// <summary>
        ///     缴费
        /// </summary>
        public PayType billPay { get; set; }

        /// <summary>
        ///     住院充值
        /// </summary>
        public PayType residentRecharge { get; set; }

        /// <summary>
        ///     预约
        /// </summary>
        public PayType appoint { get; set; }

        /// <summary>
        ///     预约取号
        /// </summary>
        public PayType takeNo { get; set; }

        /// <summary>
        ///     建档发卡
        /// </summary>
        public PayType issueCard { get; set; }

        /// <summary>
        ///     出院结算
        /// </summary>
        public PayType outhosSettlement { get; set; }
    }
}