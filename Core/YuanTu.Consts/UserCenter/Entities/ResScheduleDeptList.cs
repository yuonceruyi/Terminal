using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter.Entities
{
    public class ResScheduleDeptList
    {
        /// <summary>
        ///     ҽԺ��Ϣ��ֻ���벿����Ϣ����˽��Ϣ�������
        /// </summary>
        public Dictionary<string, object> corp { get; set; }

        public List<DeptOutParams> depts { get; set; }

        public MultiDeptOutParams multiDeptOutParams { get; set; }
    }
}