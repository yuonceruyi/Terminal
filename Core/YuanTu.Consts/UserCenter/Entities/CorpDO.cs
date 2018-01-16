namespace YuanTu.Consts.UserCenter.Entities
{
    public class CorpDO : BaseDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     �ֶ����� ҽԺ��id
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     �ֶ����� ҽԺ���������
        /// </summary>
        public long serialNum { get; set; }

        /// <summary>
        ///     ҽԺ���� 1��ʡ��ҽԺ�� 2���м�ҽԺ
        /// </summary>
        public byte type { get; set; }

        /// <summary>
        ///     ҽԺ����
        /// </summary>
        public string name { get; set; }

        /// <summary>
        ///     ҽԺ��ǩ
        /// </summary>
        public string corpTags { get; set; }

        /// <summary>
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// </summary>
        public string province { get; set; }

        /// <summary>
        ///     ҽԺ���� ���ϳ��� �³���
        /// </summary>
        public string area { get; set; }

        /// <summary>
        ///     ��ϸ��ַ
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public int bankCode { get; set; }

        /// <summary>
        ///     �����˻�
        /// </summary>
        public string bankAccount { get; set; }

        /// <summary>
        ///     ��֯�ṹ����
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     ҽԺ��д���
        /// </summary>
        public string abbreviationName { get; set; }

        /// <summary>
        ///     ҽ����id
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     ҽԺlogo
        /// </summary>
        public string corpLogo { get; set; }

        /// <summary>
        ///     ҽԺ����ip
        /// </summary>
        public string corpIp { get; set; }

        /// <summary>
        ///     ҽԺ�˿�
        /// </summary>
        public string corpPort { get; set; }

        /// <summary>
        ///     ���״���
        /// </summary>
        public string transCode { get; set; }

        /// <summary>
        ///     ��Ժ���
        /// </summary>
        public string hisCode { get; set; }

        /// <summary>
        ///     ������ID
        /// </summary>
        public string operId { get; set; }

        /// <summary>
        ///     �ն��豸��Ϣ
        /// </summary>
        public string deviceInfo { get; set; }

        /// <summary>
        ///     �������ݣ�json ���飬���������� �� [1,2] �������ÿ�����ִ�����幦�� 1 : �Һţ�2��ԤԼ�ҺŽṹ��ѯ��3����ֵ��4���ɷѣ� 5�� ȡ���浥��6��ҽԺ������7���Ű�кţ�8:סԺԤ��{get;set;}
        ///     9:סԺ�嵥
        /// </summary>
        public string corpFunction { get; set; }

        /// <summary>
        ///     �ֶ����õ�json���ݣ���APP����
        /// </summary>
        public string functionJson { get; set; }

        /// <summary>
        ///     ԤԼjson����
        /// </summary>
        public string appointmentJson { get; set; }

        /// <summary>
        ///     �Һ�json����
        /// </summary>
        public string registerJson { get; set; }

        /// <summary>
        ///     ֧����ʽ��json ���飬���������� �� [1,2,3]��1��֧������2��΢��֧����3�����
        /// </summary>
        public string payType { get; set; }

        /// <summary>
        ///     ����˳��1����֧����Һţ�2���ȹҺź�֧��
        /// </summary>
        public int paySequence { get; set; }

        /// <summary>
        ///     1��ͨ�Һţ�2ר�ҹҺţ�3��ҽ�Һţ�4��ͨԤԼ��5ר��ԤԼ��6��ҽԤԼ
        /// </summary>
        public string corpRegister { get; set; }

        /// <summary>
        ///     �Һ����� 1���Һ����գ�2����������
        /// </summary>
        public int corpGuaEntrust { get; set; }

        /// <summary>
        ///     ҽԺ�������ӣ�ͨ��ƴ���ӷ���
        /// </summary>
        public long corpNewsId { get; set; }

        /// <summary>
        ///     1: ������2��������
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     0��δ����{get;set;}1�����ߣ�
        /// </summary>
        public int online { get; set; }

        /// <summary>
        ///     ԤԼʱ���İ�
        /// </summary>
        public string timeCopyJson { get; set; }

        /// <summary>
        ///     ���ϲ��Կ��ң��Զ��ŷָ���5������
        /// </summary>
        public string testDepts { get; set; }

        /// <summary>
        ///     �Ƿ�֧������ȡ��,1��Ҫ���룬2����Ҫ
        /// </summary>
        public int needPassword { get; set; } = 1;

        /// <summary>
        ///     �Ű��ȡ����,1ͨ�����һ�ȡ,2ͨ��ҽ����ȡ
        /// </summary>
        public int scheduleRule { get; set; } = 1;

        /// <summary>
        ///     ����ҽԺ��Źҿ���ҽԺid�����Ϊ0�����ڷ�Ժ
        /// </summary>
        public long parentCorpId { get; set; } = 0;

        /// <summary>
        ///     �Һ��İ�
        /// </summary>
        public string registerTimeCopy { get; set; }

        /// <summary>
        ///     ԤԼ�İ�
        /// </summary>
        public string appointmentTimeCopy { get; set; }

        /// <summary>
        ///     ҽԺ�绰
        /// </summary>
        public string corpPhone { get; set; }

        /// <summary>
        ///     ��ʾ��APP�ϵ��������İ���Ϣ
        /// </summary>
        public string guideCopyJson { get; set; }

        /// <summary>
        ///     ����  Ĭ��Ϊ���ַ���
        /// </summary>
        public string lng { get; set; }

        /// <summary>
        ///     γ��  Ĭ��Ϊ���ַ���
        /// </summary>
        public string lat { get; set; }
    }
}