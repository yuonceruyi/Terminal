namespace YuanTu.Consts.UserCenter.Entities
{
    public class PatientVO
    {
        /// <summary>
        ///     id
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     unionId
        /// </summary>
        public long unionId { get; set; }

        /// <summary>
        ///     corpId
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     监护人身份证
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     身份证
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     就诊人类型
        /// </summary>
        public int idType { get; set; }

        /// <summary>
        ///     就诊人名称
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     手机号
        /// </summary>
        public string phoneNum { get; set; }

        /// <summary>
        ///     生日
        /// </summary>
        public string birthday { get; set; }

        /// <summary>
        ///     是否是默认就诊人
        /// </summary>
        public bool isDefault { get; set; }

        /// <summary>
        ///     性别
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        ///     账户余额
        /// </summary>
        public long balance { get; set; }

        /// <summary>
        ///     绑定的用户id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        /// 是否绑卡
        /// </summary>
        public bool bindCard { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public string cardNo { get; set; }

        /// <summary>
        /// 卡类型
        /// </summary>
        public int cardType { get; set; }

        /// <summary>
        /// 就诊人id
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        /// 是否是默认就诊人
        /// </summary>
        public bool @default { get; set; }
    }
}