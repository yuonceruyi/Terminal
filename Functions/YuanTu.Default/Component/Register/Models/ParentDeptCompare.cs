using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;

namespace YuanTu.Default.Component.Register.Models
{
    public class ParentDeptEqualityComparer : IEqualityComparer<排班科室信息>
    {

        public bool Equals(排班科室信息 x, 排班科室信息 y)
        {
            if (x.parentDeptCode == y.parentDeptCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(排班科室信息 obj)
        {
            return 0;
        }
    }
}
