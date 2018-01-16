namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayType
    {
        /// <summary>
        ///     支付宝
        /// </summary>
        public PayProperties aliPay { get; set; }

        /// <summary>
        ///     微信
        /// </summary>
        public PayProperties wxPay { get; set; }

        /// <summary>
        ///     预交金
        /// </summary>
        public PayProperties accountPay { get; set; }

        /// <summary>
        ///     到院支付
        /// </summary>
        public PayProperties hospitalPay { get; set; }

        /// <summary>
        ///     微信公众号支付
        /// </summary>
        public PayProperties wxGzhPay { get; set; }

        /// <summary>
        ///     支付宝服务窗支付
        /// </summary>
        public PayProperties aliFwcPay { get; set; }

        /// <summary>
        ///     番禺医保支付
        /// </summary>
        public PayProperties pyMedicarePay { get; set; }
    }
}