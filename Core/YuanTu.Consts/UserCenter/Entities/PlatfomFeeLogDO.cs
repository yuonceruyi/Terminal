namespace YuanTu.Consts.UserCenter.Entities
{
    public class PlatfomFeeLogDO : BaseDO
    {
        /// <summary>
        ///     流水编号-不作为订单号
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     医院id                   意义变更，从“医院id”变更为“子医院id”  为了满足省中多院区对账的问题
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
        public long patientId { get; set; } = -1l;

        /// <summary>
        ///     医院用户就诊id
        /// </summary>
        public string hisId { get; set; }

        /// <summary>
        ///     外键业务关联id
        /// </summary>
        public long outId { get; set; }

        /// <summary>
        ///     账单号(billNo)
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     充值&缴费金额  自费金额
        /// </summary>
        public long fee { get; set; }

        /// <summary>
        ///     已退费金额
        /// </summary>
        public long refundFee { get; set; }

        /// <summary>
        ///     缴费总额
        /// </summary>
        public int billFee { get; set; }

        // 状态：101 处理中,
        // 200 支付成功, 201 支付失败,
        // 300 his处理成功, 301 his处理失败,
        // 400 失效订单,
        // 500 退费成功, 501 退款失败
        // 600 取消订单，601 取消订单-关闭成功，602 取消订单-关闭失败，603 取消订单-退费成功，604 取消订单-退费失败
        /// <summary>
        ///     请使用 PayStatusEnums 订单状态枚举类
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     请使用 FeeChannelEnums  枚举类
        /// </summary>
        public int feeChannel { get; set; }

        /// <summary>
        ///     1、充值 2、缴费 3、 挂号    4 住院充值  ，请使用 OptTypeEnums  枚举类
        /// </summary>
        public int optType { get; set; }

        /// <summary>
        ///     varchar(256) 支付完成流水号
        /// </summary>
        public string outPayNo { get; set; }

        /// <summary>
        ///     his完成流水号
        /// </summary>
        public string receiptNo { get; set; }

        /// <summary>
        ///     varchar(256) json结构{reason:xxx}
        /// </summary>
        public string outPayAttr { get; set; }

        /// <summary>
        ///     varchar(256) hisjson结构{reason:xxx}
        /// </summary>
        public string hisAttr { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        ///     交易类型
        /// </summary>
        public string tradeType { get; set; }

        /// <summary>
        ///     交易编号-商户订单号
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        ///     请使用ProductTypeEnums SCANCODEPAY 扫码支付
        /// </summary>
        public string productType { get; set; }

        /// <summary>
        ///     付款人 账户，支付宝为账户，微信为 openid
        /// </summary>
        public string buyerAccount { get; set; }

        /// <summary>
        ///     用户付款时间
        /// </summary>
        public string paymentTime { get; set; }

        /// <summary>
        ///     扩展信息
        /// </summary>
        public string extendBalanceInfo { get; set; }

        /// <summary>
        ///     订单来源，0：窗口，1：自助机，2：诊间屏，3:app
        /// </summary>
        public int orderSource { get; set; } = 3;

        /// <summary>
        ///     用户预缴金余额，客服部-用户信息查询系统中展示
        /// </summary>
        public long balance { get; set; } = 0L;

        /// <summary>
        ///     业务类别  1 门诊，2住院  使用  BusinessCategoryEnums 类
        /// </summary>
        public int businessCategory { get; set; } = 1;

        /// <summary>
        ///     商户号
        /// </summary>
        public string mchId { get; set; }

        /// <summary>
        ///     服务商  如中信威富通，远图。。。
        /// </summary>
        public string vendor { get; set; }

        #region 扫码支付 特有字段

        /// <summary>
        ///     身份证号	字符串	可为空
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     证件类型	字符串	不可空，目前只支持身份证，默认传入1
        /// </summary>
        public int idType { get; set; } = 1;

        /// <summary>
        ///     姓名	字符串	不可空
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     监护人id	字符串	与idNo 必存其一
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     操作员
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     设备编号	字符串	不可空，32位长度以内
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     来源  自助机，第三方 等等
        /// </summary>
        public string source { get; set; }

        /// <summary>
        ///     二维码字符串
        /// </summary>
        public string qrCode { get; set; }

        #endregion 扫码支付 特有字段
    }
}