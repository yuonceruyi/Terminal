using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ScheduleVO
    {
        public string corpName { get; set; }
        public string date { get; set; }
        public string deptName { get; set; }
        public bool panYuMode { get; set; }
        public List<ScheduleTypeVO> scheduleTypeVOList { get; set; }
    }
}