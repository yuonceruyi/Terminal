using System;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class AppointRegLogDO : BaseDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     ������(ԤԼ�Ż��߹Һ�ID)
        /// </summary>
        public string orderNo { get; set; }

        /// <summary>
        ///     ��ʹ�� AppointRegStatusEnums  ö����   ����״̬
        ///     ״̬(100 ��֧����101 ֧���ɹ�-Hisʧ�ܣ�200 ԤԼ�ɹ���201 �Һųɹ�    ԤԼ��¼�ϴ� 301 �Һ�ʧ�ܣ��˿���...302 �Һ�ʧ�ܣ��˿�ɹ�, 303 �Һ�ʧ�ܣ��˿�ʧ�ܣ� 400 ��ȡ��,401
        ///     �ѹ��ڣ�402 ������)
        /// </summary>
        public int status { get; set; }

        /// <summary>
        ///     �Һ�����(1��ͨ�Һţ�2ר�ҹҺţ�3��ҽ�Һţ�4��ͨԤԼ��5ר��ԤԼ��6��ҽԤԼ)
        /// </summary>
        public int type { get; set; }

        /// <summary>
        ///     �Һŷ�ʽ 1 ԤԼ��2 �Һ�
        /// </summary>
        public int regMode { get; set; }

        /// <summary>
        ///     �Һ����  1��ͨ��2ר�ң�3��ҽ     6��Ƶ����
        /// </summary>
        public int regType { get; set; }

        /// <summary>
        ///     ҽԺ�ĹҺ����
        /// </summary>
        public string hosRegType { get; set; }

        /// <summary>
        ///     �Һ�����
        /// </summary>
        public long createDate { get; set; }

        /// <summary>
        ///     ƽ̨�û�id
        /// </summary>
        public long userId { get; set; }

        /// <summary>
        ///     ƽ̨����ID
        /// </summary>
        public long patientId { get; set; }

        /// <summary>
        ///     �����
        /// </summary>
        public string hisId { get; set; }

        /// <summary>
        ///     ��������
        /// </summary>
        public string patientName { get; set; }

        /// <summary>
        ///     ���ߵ绰
        /// </summary>
        public string patientPhone { get; set; }

        /// <summary>
        ///     ֤������: 1 ���֤, 2 ����֤, 3 ����, 4 ѧ��֤, 5 ����֤, 6 ��ʻ֤, 7 ̨��֤, 9 ����
        /// </summary>
        public int idType { get; set; }

        /// <summary>
        ///     ƽ̨�������֤
        /// </summary>
        public string idNo { get; set; }

        /// <summary>
        ///     '�໤��id'
        /// </summary>
        public string guarderIdNo { get; set; }

        /// <summary>
        ///     �Һ����  int ��Դ
        /// </summary>
        public string appoNo { get; set; }

        /// <summary>
        ///     ���￪ʼʱ��    ����ʱ��� ��ʽyyyy-MM-dd HH:mm:ss
        /// </summary>
        public long medDateBeg { get; set; }

        /// <summary>
        ///     �������ʱ��    ����ʱ��� ��ʽyyyy-MM-dd HH:mm:ss
        /// </summary>
        public long medDateEnd { get; set; }

        /// <summary>
        ///     �ҺŰ������/����
        /// </summary>
        public int medAmPm { get; set; }

        /// <summary>
        ///     ����ص�
        /// </summary>
        public string address { get; set; }

        /// <summary>
        ///     �ҺŽ�� = �Һŷ�+���Ʒ�
        /// </summary>
        public int regAmount { get; set; }

        /// <summary>
        ///     �Żݺ�ҺŽ�� = �Һŷ�+���Ʒ�
        /// </summary>
        public int benefitRegAmount { get; set; }

        /// <summary>
        ///     ��չ��Ϣ
        /// </summary>
        public string extend { get; set; }

        /// <summary>
        ///     ҽԺID
        /// </summary>
        public long corpId { get; set; }

        /// <summary>
        ///     ҽԺ����
        /// </summary>
        public string corpName { get; set; }

        /// <summary>
        ///     ҽ����ID
        /// </summary>
        public long corpUnionId { get; set; }

        /// <summary>
        ///     ����code
        /// </summary>
        public string deptCode { get; set; }

        /// <summary>
        ///     ������
        /// </summary>
        public string deptName { get; set; }

        /// <summary>
        ///     ҽ��code
        /// </summary>
        public string doctCode { get; set; }

        /// <summary>
        ///     ҽ������
        /// </summary>
        public string doctName { get; set; }

        /// <summary>
        ///     �޸�ʱ��
        /// </summary>
        public long updateTime { get; set; }

        /// <summary>
        ///     Ժ������  �ɿ�  �ɿգ����������Ժ������贫��
        /// </summary>
        public string hospCode { get; set; }

        /// <summary>
        ///     �Ű�ID" //�Ű�ID  string  ���ɿ�
        /// </summary>
        public string scheduleId { get; set; }

        /// <summary>
        ///     ����Id
        /// </summary>
        public string lockId { get; set; }

        /// <summary>
        ///     ԤԼ�Һ�����  1, "APP"  2, "������" 3, "΢��" 4, "����" 9, "���"
        /// </summary>
        public int channelType { get; set; }

        /// <summary>
        ///     ״̬��������
        /// </summary>
        public int statusChangeChannel { get; set; }

        public string diseaseDesc { get; set; }

        public string diseaseImageUrl { get; set; }

        public string doctAdvise { get; set; }

        public string billNo { get; set; }

        public string billDate { get; set; }

        public string billType { get; set; }

        public long billFee { get; set; }
        //��Ƶ���� ��ӣ���֪���ô���ע��֮
        //    public String password{get;set;}

        //����״̬��������ʶ��Ƶ�����Ƿ��Ѿ�����1,���ߴ������ѿ� 2
        public int extraStatus { get; set; }

        /// <summary>
        ///     �Ա�
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        ///     ����
        /// </summary>
        public int age { get; set; }

        /// <summary>
        ///     �ջ��绰
        /// </summary>
        public string phone { get; set; }

        /// <summary>
        ///     ��ַ
        /// </summary>
        public string expressAddress { get; set; }

        /// <summary>
        ///     �ռ���
        /// </summary>
        public string recipient { get; set; }

        /// <summary>
        ///     �ʱ�
        /// </summary>
        public string postcode { get; set; }

        /// <summary>
        ///     ȡҩ���� 0Ϊ��ȡ 1Ϊ���
        /// </summary>
        public int getType { get; set; }

        /// <summary>
        ///     �˵���
        /// </summary>
        public string expressCode { get; set; }

        /// <summary>
        ///     ��ݹ�˾
        /// </summary>
        public string expressCompany { get; set; }

        /// <summary>
        ///     ��ݷ���
        /// </summary>
        public int expressCost { get; set; }
    }
}