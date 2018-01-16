using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class Dept : BaseDO
    {
        /// <summary>
        ///     ҽԺid
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     ҽԺid  ���ֶβ���⣬ֻ���ӿ��ϵķ���ֵʹ��
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     ҽԺid���Ű�ϵͳ�еı��
        /// </summary>
        public string corpCode { get; set; }

        /// <summary>
        ///     �����Ҵ���
        /// </summary>
        public string parentDeptCode { get; set; }

        /// <summary>
        ///     ����������
        /// </summary>
        public string parentDeptName { get; set; }

        /// <summary>
        ///     ���������Ƶ�ȫƴ
        /// </summary>
        public string parentDeptPY { get; set; }

        /// <summary>
        ///     ���������Ƶļ�ƴ
        /// </summary>
        public string parentDeptSimplePY { get; set; }

        /// <summary>
        ///     ���Ҵ���
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     �������Ƶ�ȫƴ
        /// </summary>
        public string deptPY { get; set; }

        /// <summary>
        ///     �������Ƶļ�ƴ
        /// </summary>
        public string deptSimplePY { get; set; }

        /// <summary>
        ///     ������ϵ�绰
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     ���ҵ�ַ
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     ���ҽ���
        /// </summary>
        public string deptIntro { get; set; }

        /// <summary>
        ///     �ֶ����� �������
        /// </summary>
        public int serialNum { get; set; } = 999;

        /// <summary>
        ///     ״̬ 1����  2 ɾ��
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     �����Ҫ�õ�һ�����ұ���,��ʹ��baseDeptCode����
        ///     @since v2.3.4
        ///     �����code
        /// </summary>
        public long bigDeptCode { get; set; }

        /// <summary>
        ///     �����name
        /// </summary>
        public string bigDeptName { get; set; }

        /// <summary>
        ///     ��������Ժ���ı�ʶ����Ϊ���� ��ѯҽԺ���п��� �����ص����ݰ�������Ժ���ģ�������Ҫ���ι���
        /// </summary>
        public string hospitalPartQY { get; set; }

        /// <summary>
        ///     һ�����ұ���  �����滻��ǰ�õ� bigDeptCode ��Ӧһ�����ҵ�deptCode
        /// </summary>

        public string baseDeptCode { get; set; }

        /// <summary>
        ///     �ӿ���
        /// </summary>
        public List<Dept> children { get; set; }

        /// <summary>
        ///     �Һŵĺ�Դ���� list
        /// </summary>
        public List<RegTypeOutParams> regConfigList { get; set; }

        /// <summary>
        ///     ԤԼ�ĺ�Դ���� list
        /// </summary>
        public List<RegTypeOutParams> appoConfigList { get; set; }

        /// <summary>
        ///     �Ա����� 0:������  1:��  2:Ů
        /// </summary>
        public int genderLimit { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string ageLimit { get; set; }
    }
}