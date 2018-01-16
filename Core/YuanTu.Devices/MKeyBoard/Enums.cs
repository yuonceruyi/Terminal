namespace YuanTu.Devices.MKeyBoard
{
    public enum KMode : short
    {
        PEA_DES = 1,//8字节的工作秘钥DES运算
        PEA_TDES = 2,//16字节的工作秘钥TDES运算
        PEA_TDES2 = 3,//24字节的工作秘钥
        PEA_MDES = 4,//8字节的主密钥DES运算
        PEA_MTDES = 5//16字节的主密钥TDES运算
    }
    public enum MacMode : short
    {
        x9算法 = 1,//x9.9/x9.19算法
        ECB算法 = 2,
        PBOC = 3,
        银联算法 = 4,
        CBC算法 = 5
    }
    public enum KeyEnum
    {
        Number,
        Confirm,
        Clear,
        Cancel,
        BackSpace,
        Timeout,
    }
}
