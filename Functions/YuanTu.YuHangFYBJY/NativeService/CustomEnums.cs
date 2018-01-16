using System;

namespace YuanTu.YuHangFYBJY.NativeService
{
    public enum DayTimeFlag
    {
        上午 = 0,
        下午 = 1
    }

    public enum PayMedhodFlag
    {
        院内账户 = 1,
        市民卡 = 2,
        支付宝代扣=3,
        支付宝扫码=4,
        [Obsolete("联众HIS不支持")]
        银联 = 7,
    }

    public enum RegisterType : byte
    {
        普通=1,
        急诊=2,
        专家=4
    }
}

