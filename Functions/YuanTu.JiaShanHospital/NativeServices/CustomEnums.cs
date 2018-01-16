using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanTu.JiaShanHospital.NativeServices
{
    public enum DayTimeFlag
    {
        上午 = 0,
        下午 = 1
    }

    public enum PayMedhodFlag
    {
        银联 = 0,
        院内账户 = 1,
        市民卡 = 2,
        支付宝=3,
        微信=4,
    }

    public enum RegisterType : byte
    {
        普通=1,
        急诊=2,
        专家=4
    }
}

