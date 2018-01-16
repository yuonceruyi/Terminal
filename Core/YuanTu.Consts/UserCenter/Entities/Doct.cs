namespace YuanTu.Consts.UserCenter.Entities
{
    public class Doct : BaseDO
    {
        /// <summary>
        ///     �ֶ����� �������
        /// </summary>
        public int serialNum { get; set; } = 999;

        /// <summary>
        ///     ҽ������
        /// </summary>
        public string doctCode { get; set; }

        /// <summary>
        ///     ҽ������
        /// </summary>
        public string doctName { get; set; }

        /// <summary>
        ///     �������Ƶ�ȫƴ
        /// </summary>
        public string doctPY { get; set; }

        /// <summary>
        ///     �������Ƶļ�ƴ
        /// </summary>
        public string doctSimplePY { get; set; }

        /// <summary>
        ///     �Ա�  ���֣��С�Ů
        /// </summary>
        public string sex { get; set; }

        /// <summary>
        ///     ҽ��ͷ��  ��·��
        /// </summary>
        public string doctLogo { get; set; }

        /// <summary>
        ///     ҽ������ ����
        /// </summary>
        public string doctLevel { get; set; }

        /// <summary>
        ///     ҽ��ְ��  ��Ӧ HisScheduleInfoItemDO  �� ScheduleInfoItemDO ��� doctTech
        /// </summary>
        public string doctProfe { get; set; }

        /// <summary>
        ///     ҽ���س�
        /// </summary>
        public string doctSpec { get; set; }

        /// <summary>
        ///     ҽ������
        /// </summary>
        public string doctIntro { get; set; }

        /// <summary>
        ///     ҽԺid
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     ҽԺid���Ű�ϵͳ�еı��
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     ҽԺ����   �������޴��ֶΣ�ֻ�ڽӿڷ�������ʱ�����ϸ��ֶε����ݼ���
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     ���Ҵ���
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     ״̬ 1��������2��������
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     ҽ���绰
        /// </summary>
        public string doctPhoneNum { get; set; }

        /// <summary>
        ///     ҽ������
        /// </summary>
        public string doctEmployeeNum { get; set; }

        /// <summary>
        ///     ҽ��ͷ��ȫ·��  �ӿڷ���ʱʹ��  �����Ƶ�ַ
        /// </summary>
        public string doctPictureUrl { get; set; }

        /// <summary>
        ///     ͼƬ�����ַ������ �������޷����� ���������
        ///     ����ʹ�ã�����ҽ�����������һ�� nginx ����
        ///     ҽԺ������ ���� nginx �Ĵ����ַ��nginx �������ת����ָ�����Ƶ�ͼƬ������
        ///     ҽ��ͷ��ȫ·��  �ӿڷ���ʱʹ��  nginx �����ַ
        /// </summary>
        public string doctPictureIntranetUrl { get; set; }

        /// <summary>
        ///     ְ�ƣ�Ϊ��ȡ�����ص����ݣ����Լ�����ֶΣ���doctProfe����һ��
        /// </summary>
        public string doctTech { get; set; }
    }
}