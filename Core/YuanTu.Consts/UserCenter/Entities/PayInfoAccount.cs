namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayInfoAccount : PayInfoItem
    {
        /// <summary>
        ///     余额
        /// </summary>
        public string balance { get; set; }

        /// <summary>
        ///     卡号
        /// </summary>
        public string cardNo { get; set; }

        /// <summary>
        ///     卡类型
        /// </summary>
        public string cardType { get; set; }

        /// <summary>
        ///     是否需要绑卡
        /// </summary>
        public string isTiedCard { get; set; }

        /// <summary>
        ///     卡名称
        /// </summary>
        public string name { get; set; }
    }
}