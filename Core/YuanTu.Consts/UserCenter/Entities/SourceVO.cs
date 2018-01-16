using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class SourceVO
    {
        public bool needSource { get; set; } = true;
        public List<SourceDO> sourceList { get; set; }
    }
}