using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;

namespace YuanTu.BJJingDuETYY.Component.Register.Services
{
    public static class DocExtendDic
    {
        public static Dictionary<string, 医生介绍> DocDictionary =
            new Dictionary<string, 医生介绍>();
    }
    public class DocEqualityComparer : IEqualityComparer<排班信息>
    {

        public bool Equals(排班信息 x, 排班信息 y)
        {
            if (x.doctCode == y.doctCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(排班信息 obj)
        {
            return 0;
        }
    }
    public class DeptEqualityComparer : IEqualityComparer<排班信息>
    {

        public bool Equals(排班信息 x, 排班信息 y)
        {
            if (x.deptCode == y.deptCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(排班信息 obj)
        {
            return 0;
        }
    }
}
