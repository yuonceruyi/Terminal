using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;

namespace YuanTu.QDKFQRM.Component.BillPay.Services
{
    public class ClinicEqualityComparer : IEqualityComparer<缴费概要信息>
    {

        public bool Equals(缴费概要信息 x, 缴费概要信息 y)
        {
            if (x.billNo == y.billNo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(缴费概要信息 obj)
        {
            return 0;
        }
    }
}
