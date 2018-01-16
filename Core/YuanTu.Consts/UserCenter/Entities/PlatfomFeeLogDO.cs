namespace YuanTu.Consts.UserCenter.Entities
{
    public class PlatfomFeeLogDO : BaseDO
    {
        /// <summary>
        ///     ��ˮ���-����Ϊ������
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     ҽԺid                   ���������ӡ�ҽԺid�����Ϊ����ҽԺid��  Ϊ������ʡ�ж�Ժ�����˵�����
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     ҽ����id
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     �û�id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     �û�ƽ̨id
        /// </summary>
        public long patientId { get; set; } = -1l;

        /// <summary>
        ///     ҽԺ�û�����id
        /// </summary>
        public string hisId { get; set; }

        /// <summary>
        ///     ���ҵ�����id
        /// </summary>
        public long outId { get; set; }

        /// <summary>
        ///     �˵���(billNo)
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     ��ֵ&�ɷѽ��  �Էѽ��
        /// </summary>
        public long fee { get; set; }

        /// <summary>
        ///     ���˷ѽ��
        /// </summary>
        public long refundFee { get; set; }

        /// <summary>
        ///     �ɷ��ܶ�
        /// </summary>
        public int billFee { get; set; }

        // ״̬��101 ������,
        // 200 ֧���ɹ�, 201 ֧��ʧ��,
        // 300 his����ɹ�, 301 his����ʧ��,
        // 400 ʧЧ����,
        // 500 �˷ѳɹ�, 501 �˿�ʧ��
        // 600 ȡ��������601 ȡ������-�رճɹ���602 ȡ������-�ر�ʧ�ܣ�603 ȡ������-�˷ѳɹ���604 ȡ������-�˷�ʧ��
        /// <summary>
        ///     ��ʹ�� PayStatusEnums ����״̬ö����
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     ��ʹ�� FeeChannelEnums  ö����
        /// </summary>
        public int feeChannel { get; set; }

        /// <summary>
        ///     1����ֵ 2���ɷ� 3�� �Һ�    4 סԺ��ֵ  ����ʹ�� OptTypeEnums  ö����
        /// </summary>
        public int optType { get; set; }

        /// <summary>
        ///     varchar(256) ֧�������ˮ��
        /// </summary>
        public string outPayNo { get; set; }

        /// <summary>
        ///     his�����ˮ��
        /// </summary>
        public string receiptNo { get; set; }

        /// <summary>
        ///     varchar(256) json�ṹ{reason:xxx}
        /// </summary>
        public string outPayAttr { get; set; }

        /// <summary>
        ///     varchar(256) hisjson�ṹ{reason:xxx}
        /// </summary>
        public string hisAttr { get; set; }

        /// <summary>
        ///     ����
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string tradeType { get; set; }

        /// <summary>
        ///     ���ױ��-�̻�������
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        ///     ��ʹ��ProductTypeEnums SCANCODEPAY ɨ��֧��
        /// </summary>
        public string productType { get; set; }

        /// <summary>
        ///     ������ �˻���֧����Ϊ�˻���΢��Ϊ openid
        /// </summary>
        public string buyerAccount { get; set; }

        /// <summary>
        ///     �û�����ʱ��
        /// </summary>
        public string paymentTime { get; set; }

        /// <summary>
        ///     ��չ��Ϣ
        /// </summary>
        public string extendBalanceInfo { get; set; }

        /// <summary>
        ///     ������Դ��0�����ڣ�1����������2���������3:app
        /// </summary>
        public int orderSource { get; set; } = 3;

        /// <summary>
        ///     �û�Ԥ�ɽ����ͷ���-�û���Ϣ��ѯϵͳ��չʾ
        /// </summary>
        public long balance { get; set; } = 0L;

        /// <summary>
        ///     ҵ�����  1 ���2סԺ  ʹ��  BusinessCategoryEnums ��
        /// </summary>
        public int businessCategory { get; set; } = 1;

        /// <summary>
        ///     �̻���
        /// </summary>
        public string mchId { get; set; }

        /// <summary>
        ///     ������  ����������ͨ��Զͼ������
        /// </summary>
        public string vendor { get; set; }

        #region ɨ��֧�� �����ֶ�

        /// <summary>
        ///     ���֤��	�ַ���	��Ϊ��
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     ֤������	�ַ���	���ɿգ�Ŀǰֻ֧�����֤��Ĭ�ϴ���1
        /// </summary>
        public int idType { get; set; } = 1;

        /// <summary>
        ///     ����	�ַ���	���ɿ�
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     �໤��id	�ַ���	��idNo �ش���һ
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     ����Ա
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     �豸���	�ַ���	���ɿգ�32λ��������
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     ��Դ  �������������� �ȵ�
        /// </summary>
        public string source { get; set; }

        /// <summary>
        ///     ��ά���ַ���
        /// </summary>
        public string qrCode { get; set; }

        #endregion ɨ��֧�� �����ֶ�
    }
}