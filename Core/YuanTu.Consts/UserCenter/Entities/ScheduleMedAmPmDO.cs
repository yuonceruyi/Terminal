namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScheduleMedAmPmDO
    {
        public string deptCode { get; set; }
        public string deptName { get; set; }

        /// <summary>
        ///     �ҺŰ��  �ɿ�  1���� 2 ����  ��ָ����������ʱ��Ч
        /// </summary>
        public int medAmPm { get; set; } = -1;

        /// <summary>
        ///     �Һŷ� 	 ��λΪ��  һ�����Ϊ0
        /// </summary>
        public string regFee { get; set; }

        /// <summary>
        ///     ���Ʒ�   	 ��λΪ�� �������
        /// </summary>
        public string treatFee { get; set; }

        /// <summary>
        ///     �ҺŽ��  ��λΪ�� �Һŷ�+���Ʒ�
        /// </summary>
        public string regAmount { get; set; }

        /// <summary>
        ///     �Ű�ID" //�Ű�ID  string  ���ɿ�
        /// </summary>
        public string scheduleId { get; set; }

        /// <summary>
        ///     ���
        /// </summary>
        public string restnum { get; set; }

        /// <summary>
        ///     ��ʹ����������Ƶ����ʹ��
        /// </summary>
        public string appointedNum { get; set; }

        /// <summary>
        ///     pc-web �� �¼�
        /// </summary>
        public string hosRegType { get; set; }

        /// <summary>
        ///     �Һ����  1��ͨ��2ר�ң�3��ҽ ��4 ���5 ����
        /// </summary>
        public int regType { get; set; } = -1;

        /// <summary>
        ///     ԤԼ�Һ�  1 ԤԼ   2 �Һ�
        /// </summary>
        public int regMode { get; set; } = -1;

        /// <summary>
        ///     ר�������ͣ�1ר������ҽʦ   2ר�Ҹ�����ҽʦ
        /// </summary>
        public int subRegType { get; set; }

        /// <summary>
        ///     ר�����������ƣ�����ҽʦ��������ҽʦ
        /// </summary>
        public string subRegTypeName { get; set; }
    }
}