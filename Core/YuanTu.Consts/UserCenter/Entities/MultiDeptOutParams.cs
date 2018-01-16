using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class MultiDeptOutParams
    {
        public List<DeptOutParams> deptOutParams { get; set; }
        public int MultiDept { get; set; }
    }
}