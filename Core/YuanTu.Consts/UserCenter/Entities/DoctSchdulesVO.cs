using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class DoctSchdulesVO
    {
        /// <summary>
        ///     服务器的 当前时间  格式  '2016-04-25'
        /// </summary>
        public string today { get; set; }

        /// <summary>
        ///     排班的起始时间，指的是 当天还是 第二天  '2016-04-25'(如果，配置的是可查询7天的排班，那么这个字段指的就是7天中的第一天的时间)
        /// </summary>
        public string schStartDate { get; set; }

        /// <summary>
        ///     排班的天数，指的是 某医院 支持查询的 排班天数，比如 7天
        /// </summary>
        public string schDays { get; set; }

        /// <summary>
        /// </summary>
        public Doct doct { get; set; }

        public List<ScheduleVO> schdule { get; set; }
    }
}