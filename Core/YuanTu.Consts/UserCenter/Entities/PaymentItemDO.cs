namespace YuanTu.Consts.UserCenter.Entities
{
    public class PaymentItemDO : BaseDO
    {
        private long id { get; set; }

        /// <summary>
        ///     ҽԺid
        /// </summary>
        private long corpId { get; set; } = -1;

        /// <summary>
        ///     ҽԺ����
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     ƽ̨�û�id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     ƽ̨����ID
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     ֤������: 1 ���֤, 2 ����֤, 3 ����, 4 ѧ��֤, 5 ����֤, 6 ��ʻ֤, 7 ̨��֤, 9 ����
        /// </summary>
        public int idType { get; set; } = 1;

        /// <summary>
        ///     ƽ̨�������֤
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     '�໤��id'
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     ״̬(100 ��֧����101 ֧���ɹ�-Hisʧ�ܣ�200 �ɹ���401 �ѹ��ڣ�402 ������)
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     ���㵥��
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     �������� YYYY-MM-DD
        /// </summary>
        public string billDate { get; set; }

        /// <summary>
        ///     ��Ŀ���
        /// </summary>
        public string itemNo { get; set; }

        /// <summary>
        ///     ��Ŀ��Ʒ����
        /// </summary>
        public string productCode { get; set; }

        /// <summary>
        ///     ��Ŀ����
        /// </summary>
        public string itemName { get; set; }

        /// <summary>
        ///     ��Ŀ���
        /// </summary>
        public string itemSpecs { get; set; }

        /// <summary>
        ///     ��Ŀ����
        /// </summary>
        public string itemLiquid { get; set; }

        /// <summary>
        ///     ��Ŀ��λ
        /// </summary>
        public string itemUnits { get; set; }

        /// <summary>
        ///     ���� ����С������������ʹ���ַ���
        /// </summary>
        public string itemQty { get; set; } = "0";

        /// <summary>
        ///     ����,��λ��,��С����
        /// </summary>
        public string itemPrice { get; set; }

        /// <summary>
        ///     �ܶ�
        /// </summary>
        public string billFee { get; set; }
    }
}