namespace YuanTu.Consts.UserCenter.Entities
{
    public class PayInfoAccount : PayInfoItem
    {
        /// <summary>
        ///     ���
        /// </summary>
        public string balance { get; set; }

        /// <summary>
        ///     ����
        /// </summary>
        public string cardNo { get; set; }

        /// <summary>
        ///     ������
        /// </summary>
        public string cardType { get; set; }

        /// <summary>
        ///     �Ƿ���Ҫ��
        /// </summary>
        public string isTiedCard { get; set; }

        /// <summary>
        ///     ������
        /// </summary>
        public string name { get; set; }
    }
}