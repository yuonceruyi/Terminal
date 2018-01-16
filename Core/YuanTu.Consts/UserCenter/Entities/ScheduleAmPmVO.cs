using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScheduleAmPmVO
    {
        /// <summary>
        ///     上下午标志 1：上午  2：下午
        /// </summary>
        public string medAmPm { get; set; }

        public List<ScheduleMedAmPmDO> data { get; set; }
    }
}