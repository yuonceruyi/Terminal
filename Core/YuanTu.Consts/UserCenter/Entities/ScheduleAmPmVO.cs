using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScheduleAmPmVO
    {
        /// <summary>
        ///     �������־ 1������  2������
        /// </summary>
        public string medAmPm { get; set; }

        public List<ScheduleMedAmPmDO> data { get; set; }
    }
}