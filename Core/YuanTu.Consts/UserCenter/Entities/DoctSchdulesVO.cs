using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class DoctSchdulesVO
    {
        /// <summary>
        ///     �������� ��ǰʱ��  ��ʽ  '2016-04-25'
        /// </summary>
        public string today { get; set; }

        /// <summary>
        ///     �Ű����ʼʱ�䣬ָ���� ���컹�� �ڶ���  '2016-04-25'(��������õ��ǿɲ�ѯ7����Ű࣬��ô����ֶ�ָ�ľ���7���еĵ�һ���ʱ��)
        /// </summary>
        public string schStartDate { get; set; }

        /// <summary>
        ///     �Ű��������ָ���� ĳҽԺ ֧�ֲ�ѯ�� �Ű����������� 7��
        /// </summary>
        public string schDays { get; set; }

        /// <summary>
        /// </summary>
        public Doct doct { get; set; }

        public List<ScheduleVO> schdule { get; set; }
    }
}