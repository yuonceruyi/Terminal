using System;

namespace YuanTu.ISO8583.CPUCard
{
    [Flags]
    public enum AIP : byte
    {
        SDA = 0x40,
        DDA = 0x20,
        持卡人认证 = 0x10,
        执行终端风险管理 = 0x08,
        发卡行认证 = 0x04,
        CDA = 0x01
    }
}