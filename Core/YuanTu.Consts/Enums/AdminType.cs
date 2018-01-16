using System.ComponentModel;

namespace YuanTu.Consts.Enums
{
    public enum AdminType
    {
        [Description("清钱箱")]
        清钱箱 = 0,
        [Description("系统升级")]
        自动更新 = 1,
        [Description("进入维护")]
        进入维护 = 2,
        [Description("设置卡数量")]
        设置卡数量 = 3,
        [Description("退出系统")]
        退出系统 = 4,
        [Description("发卡器退卡")]
        发卡器退卡 = 5,
        [Description("读卡器退卡")]
        读卡器退卡 = 6
    }
}