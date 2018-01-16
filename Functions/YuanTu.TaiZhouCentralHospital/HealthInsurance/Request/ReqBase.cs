namespace YuanTu.TaiZhouCentralHospital.HealthInsurance.Request
{
    public class ReqBase
    {
        /// <summary>
        ///     卡类型 0：无医保卡  1：医保卡  2：社会保障卡
        /// </summary>
        public int 卡类型 { get; set; } = 1;

        //医保卡、社会保障卡时：22号交易传空；非22号交易传IC卡号信息（参考3.4.1个人基本信息的第1个数据，22号交易时获得）
        /// <summary>
        /// IC卡信息 无医保卡时：与个人无关的交易传空
        /// </summary>
        public string Ic卡信息 { get; set; }

        /// <summary>
        ///     现金支付方式
        /// </summary>
        public int 现金支付方式 { get; set; }

        /// <summary>
        ///     银行卡信息
        /// </summary>
        public string 银行卡信息 { get; set; }
    }
}