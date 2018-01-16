using System;

namespace YuanTu.NingXiaHospital.CardReader.BankCard.CPUCard
{
    [Flags]
    public enum DevCap0 : byte
    {
        手工键盘输入 = 0x80,
        磁条 = 0x40,
        接触式IC卡 = 0x20
    }

    [Flags]
    public enum DevCap1 : byte
    {
        IC卡明文PIN验证 = 0x80,
        加密PIN联机验证 = 0x40,
        签名_纸 = 0x20,
        无需CVM = 0x08,
        持卡人证件验证 = 0x01
    }

    [Flags]
    public enum DevCap2 : byte
    {
        静态数据认证_SDA = 0x80,
        动态数据认证_DDA = 0x40,
        吞卡 = 0x20,
        复合动态数据认证_应用密码生成_CDA = 0x08
    }
}