using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ResScheduleDeptList
    {
        /// <summary>
        ///     医院信息，只传入部分信息，隐私信息切勿放入
        /// </summary>
        public Dictionary<string, object> corp { get; set; }

        public List<DeptOutParams> depts { get; set; }

        public MultiDeptOutParams multiDeptOutParams { get; set; }
    }
}