using System;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583.CPUCard
{
    [Flags]
    public enum TSI : byte
    {
        脱机数据认证已进行 = 0x80,
        持卡人验证已进行 = 0x40,
        卡片风险管理已进行 = 0x20,
        发卡行认证已进行 = 0x10,
        终端风险管理已进行 = 0x08,
        脚本处理已进行 = 0x04
    }
}