namespace YuanTu.Consts.UserCenter.Entities
{
    public class PaymentItemDO : BaseDO
    {
        private long id { get; set; }

        /// <summary>
        ///     医院id
        /// </summary>
        private long corpId { get; set; } = -1;

        /// <summary>
        ///     医院名称
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     平台用户id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     平台患者ID
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        ///     患者姓名
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     证件类型: 1 身份证, 2 军人证, 3 护照, 4 学生证, 5 回乡证, 6 驾驶证, 7 台胞证, 9 其它
        /// </summary>
        public int idType { get; set; } = 1;

        /// <summary>
        ///     平台患者身份证
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     '监护人id'
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     状态(100 待支付，101 支付成功-His失败，200 成功，401 已过期，402 已作废)
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     结算单号
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     开单日期 YYYY-MM-DD
        /// </summary>
        public string billDate { get; set; }

        /// <summary>
        ///     项目序号
        /// </summary>
        public string itemNo { get; set; }

        /// <summary>
        ///     项目产品代码
        /// </summary>
        public string productCode { get; set; }

        /// <summary>
        ///     项目名称
        /// </summary>
        public string itemName { get; set; }

        /// <summary>
        ///     项目规格
        /// </summary>
        public string itemSpecs { get; set; }

        /// <summary>
        ///     项目剂型
        /// </summary>
        public string itemLiquid { get; set; }

        /// <summary>
        ///     项目单位
        /// </summary>
        public string itemUnits { get; set; }

        /// <summary>
        ///     数量 存在小数点的情况，故使用字符串
        /// </summary>
        public string itemQty { get; set; } = "0";

        /// <summary>
        ///     单价,单位分,含小数点
        /// </summary>
        public string itemPrice { get; set; }

        /// <summary>
        ///     总额
        /// </summary>
        public string billFee { get; set; }
    }
}