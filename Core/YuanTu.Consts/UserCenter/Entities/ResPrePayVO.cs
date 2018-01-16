namespace YuanTu.Consts.UserCenter.Entities
{
    public class ResPrePayVO
    {
        /// <summary>
        ///     流水号-不再作为订单号
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     唯一订单号
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        ///     请使用 FeeChannelEnums  枚举类
        /// </summary>
        public int feeChannel { get; set; }

        /// <summary>
        ///     订单状态 101 处理中, * 200 支付成功, 201 支付失败, * 300 his处理成功,
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     支付状态   100 未支付，200 支付成功，201 支付失败，500 已退费
        /// </summary>
        public int payStatus { get; set; }
    }
}