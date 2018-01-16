using System;

namespace YuanTu.Consts.Enums
{
    [Flags]
    public enum BanCardMediaType : byte
    {
        磁条 = 1,
        IC芯片 = 2,
        闪付 = 4,
    }
}