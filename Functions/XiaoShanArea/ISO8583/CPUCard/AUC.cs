using System;

namespace YuanTu.YuHangArea.ISO8583.CPUCard
{
    [Flags]
    public enum AUC : byte
    {
        国内现金交易有效 = 0x80,
        国际现金交易有效 = 0x40,
        国内商品有效 = 0x20,
        国际商品有效 = 0x10,
        国内服务有效 = 0x08,
        国际服务有效 = 0x04,
        ATM有效 = 0x02,
        在非ATM终端上有效 = 0x01
    }
}