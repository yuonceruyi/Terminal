namespace YuanTu.Consts.UserCenter.Entities
{
    public class RefundLogDO : BaseDO
    {
        /// <summary>
        ///     流水编号-不作为订单号
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     医院id
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     医联体id
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     用户id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     用户平台id
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        ///     医院用户就诊id
        /// </summary>
        public long hisId { get; set; }

        /// <summary>
        ///     外键业务关联id
        /// </summary>
        public long outId { get; set; }

        /// <summary>
        ///     账单号(billNo)
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     充值&缴费金额
        /// </summary>
        public long fee { get; set; }

        /// <summary>
        ///     请使用 RefundStatusEnums 枚举类
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     请使用 FeeChannelEnums  枚举类
        /// </summary>
        public int feeChannel { get; set; }

        /// <summary>
        ///     请使用 OptTypeEnums  枚举类
        /// </summary>
        public int optType { get; set; }

        /// <summary>
        ///     varchar(256) 支付完成流水号
        /// </summary>
        public string outPayNo { get; set; }

        /// <summary>
        ///     varchar(256) json结构{reason:xxx}
        /// </summary>
        public string outPayAttr { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        ///     商户订单号
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        ///     商户退款单号
        /// </summary>
        public string outRefundNo { get; set; }

        /// <summary>
        ///     商户退款原因
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        ///     来源,将来支持自助机退费
        /// </summary>
        public string source { get; set; } = "APP";

        /// <summary>
        ///     操作员
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     设备编号
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     请使用 RefundTypeEnums  枚举类
        /// </summary>
        public int refundType { get; set; }
    }
}