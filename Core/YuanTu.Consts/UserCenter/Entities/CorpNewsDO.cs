namespace YuanTu.Consts.UserCenter.Entities
{
    public class CorpNewsDO
    {
        /// <summary>
        /// </summary>
        public long id { get; set; }

        /// <summary>
        ///     ��ĿID
        /// </summary>
        public long classifyId { get; set; }

        /// <summary>
        ///     ҽԺid
        /// </summary>
        public long hospitalId { get; set; }

        /// <summary>
        ///     ҽ����ID
        /// </summary>
        public long unionId { get; set; }

        /// <summary>
        ///     ���� ���Բ�дtitle��ֱ��ͼƬ
        /// </summary>
        public string title { get; set; }

        /// <summary>
        ///     ����ͼƬ
        /// </summary>
        public string titleImg { get; set; }

        /// <summary>
        ///     ��ҳ�ֲ�ͼURL��ַ
        /// </summary>
        public string homeUrl { get; set; }

        /// <summary>
        ///     ��Ѷid ����Ϊ�գ�Ϊ�մ���û������
        /// </summary>
        public long newsId { get; set; }

        /// <summary>
        ///     �����ֶ�
        /// </summary>
        public int sort { get; set; }

        /// <summary>
        ///     1: �����Ϣ{get;set;} 2 ��ҽԺ������Ϣ��3:���Ŷ�̬��4��֪ͨ���棻
        /// </summary>
        public int type { get; set; }

        /// <summary>
        ///     1���ݸ壺2����ʽ�壻3��ɾ��
        /// </summary>
        public char status { get; set; }

        /// <summary>
        ///     ����ժҪ
        /// </summary>
        public string summary { get; set; }
    }
}