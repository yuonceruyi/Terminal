namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayTypesDO
    {
        /// <summary>
        ///     �����ֵ
        /// </summary>
        public PayType recharge { get; set; }

        /// <summary>
        ///     �Һ�
        /// </summary>
        public PayType reg { get; set; }

        /// <summary>
        ///     �ɷ�
        /// </summary>
        public PayType billPay { get; set; }

        /// <summary>
        ///     סԺ��ֵ
        /// </summary>
        public PayType residentRecharge { get; set; }

        /// <summary>
        ///     ԤԼ
        /// </summary>
        public PayType appoint { get; set; }

        /// <summary>
        ///     ԤԼȡ��
        /// </summary>
        public PayType takeNo { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public PayType issueCard { get; set; }

        /// <summary>
        ///     ��Ժ����
        /// </summary>
        public PayType outhosSettlement { get; set; }
    }
}