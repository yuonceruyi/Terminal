using System;

namespace YuanTu.ShengZhouZhongYiHospital.ISO8583.CPUCard
{
    [Flags]
    public enum TVR0 : byte
    {
        未进行脱机数据认证 = 0x80,
        脱机静态数据认证失败 = 0x40,
        IC卡数据缺失 = 0x20,
        卡片出现在终端异常文件中 = 0x10,
        脱机动态数据认证失败 = 0x08,
        复合动态数据认证应用密码生成失败 = 0x04
    }

    [Flags]
    public enum TVR1 : byte
    {
        IC卡和终端应用版本不一致 = 0x80,
        应用已过期 = 0x40,
        应用尚未生效 = 0x20,
        卡片不允许所请求的服务 = 0x10,
        新卡 = 0x08
    }

    [Flags]
    public enum TVR2 : byte
    {
        持卡人验证失败 = 0x80,
        未知的CVM = 0x40,
        PIN重试次数超限 = 0x20,
        要求输入PIN_但密码键盘不存在或工作不正常 = 0x10,
        要求输入PIN_密码键盘存在_但未输入PIN = 0x08,
        输入联机PIN = 0x04
    }

    [Flags]
    public enum TVR3 : byte
    {
        交易超过最低限额 = 0x80,
        超过连续脱机交易下限 = 0x40,
        超过连续脱机交易上限 = 0x20,
        交易被随机选择联机处理 = 0x10,
        商户要求联机交易 = 0x08
    }

    [Flags]
    public enum TVR4 : byte
    {
        使用缺省TDOL = 0x80,
        发卡行认证失败 = 0x40,
        最后一次GENERATE_AC命令之前脚本处理失败 = 0x20,
        最后一次GENERATE_AC命令之后脚本处理失败 = 0x10
    }
}