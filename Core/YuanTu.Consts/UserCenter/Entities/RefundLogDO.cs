namespace YuanTu.Consts.UserCenter.Entities
{
    public class RefundLogDO : BaseDO
    {
        /// <summary>
        ///     ��ˮ���-����Ϊ������
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     ҽԺid
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
        public long patientId { get; set; }

        /// <summary>
        ///     ҽԺ�û�����id
        /// </summary>
        public long hisId { get; set; }

        /// <summary>
        ///     ���ҵ�����id
        /// </summary>
        public long outId { get; set; }

        /// <summary>
        ///     �˵���(billNo)
        /// </summary>
        public string billNo { get; set; }

        /// <summary>
        ///     ��ֵ&�ɷѽ��
        /// </summary>
        public long fee { get; set; }

        /// <summary>
        ///     ��ʹ�� RefundStatusEnums ö����
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     ��ʹ�� FeeChannelEnums  ö����
        /// </summary>
        public int feeChannel { get; set; }

        /// <summary>
        ///     ��ʹ�� OptTypeEnums  ö����
        /// </summary>
        public int optType { get; set; }

        /// <summary>
        ///     varchar(256) ֧�������ˮ��
        /// </summary>
        public string outPayNo { get; set; }

        /// <summary>
        ///     varchar(256) json�ṹ{reason:xxx}
        /// </summary>
        public string outPayAttr { get; set; }

        /// <summary>
        ///     ����
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        ///     �̻�������
        /// </summary>
        public string outTradeNo { get; set; }

        /// <summary>
        ///     �̻��˿��
        /// </summary>
        public string outRefundNo { get; set; }

        /// <summary>
        ///     �̻��˿�ԭ��
        /// </summary>
        public string reason { get; set; }

        /// <summary>
        ///     ��Դ,����֧���������˷�
        /// </summary>
        public string source { get; set; } = "APP";

        /// <summary>
        ///     ����Ա
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     �豸���
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     ��ʹ�� RefundTypeEnums  ö����
        /// </summary>
        public int refundType { get; set; }
    }
}