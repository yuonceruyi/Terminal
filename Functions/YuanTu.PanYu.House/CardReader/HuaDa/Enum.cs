

namespace YuanTu.PanYu.House.CardReader.HuaDa
{
    public enum cardType:byte
    {
        M1=0x41,
        TypeA=0x41,
        TypeB=0x42
    }

    public enum mode : byte
    {
        keyA = 0,
        keyB = 4,
    }

    public enum mode2 : byte
    {
        keyA = 0x60,
        keyB = 0x61,
    }
    public enum slotNo : byte
    {
        大卡座=0x01,
        副卡座 =0x02,
        SAM1卡座 =0x11,
        SAM2卡座 =0x12,
        SAM3卡座 =0x13,
        SAM4卡座 =0x14
    }

    public enum magMode : int
    {
        软件控制读取=1,
        模拟键盘=2
    }

    public enum voiceType : byte
    {
        请插卡=1,
        请刷卡 =2,
        读卡错误 =3,
        请输入密码 =4,
        密码错误 =5,
        操作成功 =6,
        操作超时 =7,
        操作失败 =8,
        请取回卡 =9,
        请重新输入密码 =10,
        请再次输入密码 =11,
        请输入新密码 =12,
        请输入旧密码 =13,
        请确认新密码 =14
    }

    public enum voiceMode : byte
    {
        //键盘和设备集成一体
        内置键盘语音=0,
        //键盘通过线外接
        外置键盘语音=1
    }

    public enum m1Addr : byte
    {
         零扇区0块=0,
         零扇区1块 = 1,
         零扇区2块 = 2,
         零扇区3块 = 3,
         一扇区0块 = 4,
         一扇区1块 = 5,
         一扇区2块 = 6,
         一扇区3块 = 7,
         二扇区0块 = 8,
         二扇区1块 = 9,
         二扇区2块 = 10,
         二扇区3块 = 11,
         三扇区0块 = 12,
         三扇区1块 = 13,
         三扇区2块 = 14,
         三扇区3块 = 15

    }
}
