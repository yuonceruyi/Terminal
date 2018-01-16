namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayProperties
    {
        /// <summary>
        ///     是否开通
        /// </summary>
        public bool open { get; set; } = false;

        /// <summary>
        ///     true 表示 打款到 医院的账号；false 表示打款到医联体的账号
        /// </summary>
        public bool selfAccount { get; set; } = true;
    }
}