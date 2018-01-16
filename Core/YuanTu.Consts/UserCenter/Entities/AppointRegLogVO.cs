using System;
using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class AppointRegLogVO : AppointRegLogDO
    {
        /// <summary>
        ///     ��������
        /// </summary>
        public long medDate { get; set; }

        /// <summary>
        ///     ����ʱ���
        /// </summary>
        public long medBegTime { get; set; }

        /// <summary>
        ///     ����ʱ���
        /// </summary>
        public long medEndTime { get; set; }

        /// <summary>
        ///     ����ʱ��
        /// </summary>
        public string medTime { get; set; }

        /// <summary>
        ///     ���ؽ���ԤԼ���ŵ��ֻ���
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     �ɷѵ�״̬
        /// </summary>
        public int paymentStatus { get; set; }

        public List<PaymentItemDO> prescribedReport { get; set; }

        /// <summary>
        ///     ֧����ˮ
        /// </summary>
        public PlatfomFeeLogDO platfomFeeLogDO { get; set; }

        /// <summary>
        ///     �˷Ѷ���
        /// </summary>
        public RefundLogDO refundLogDO { get; set; }

        /// <summary>
        ///     ״̬����
        /// </summary>
        public string statusDes { get; set; }

        /// <summary>
        ///     ֧����ʽ
        /// </summary>
        public string payTypeDesc { get; set; }

        /// <summary>
        ///     ����ʱ�䣬��<0ʱ�� ����
        /// </summary>
        public long expirationTime { get; set; }

        /// <summary>
        ///     �Ƿ��ܽ�������
        /// </summary>
        public bool canEvaluate { get; set; }

        /// <summary>
        ///     ������������
        /// </summary>
        public string channelTypeName { get; set; }

        /// <summary>
        ///     ԤԼ�Һ���������
        /// </summary>
        public string regModeName { get; set; }

        /// <summary>
        ///     ������������
        /// </summary>
        public string regTypeName { get; set; }

        /// <summary>
        ///     �Ա�
        /// </summary>
        public string sexDesc { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string illComplained { get; set; }

        /// <summary>
        ///     ҽ�����
        /// </summary>
        public string doctorDiagnosis { get; set; }

        /// <summary>
        ///     ҽ��
        /// </summary>
        public string doctorOrder { get; set; }

        /// <summary>
        ///     ������
        /// </summary>
        public string inspection { get; set; }

        /// <summary>
        ///     ��������ҩ
        /// </summary>
        public string prescription { get; set; }

        /// <summary>
        ///     ���� ����
        /// </summary>
        public string visitDoctor { get; set; }
    }
}