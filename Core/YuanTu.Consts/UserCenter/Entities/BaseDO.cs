using System;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class BaseDO
    {
        public long id { get; set; }

        public DateTime gmtModify { get; set; }

        public DateTime gmtCreate { get; set; }
    }
}