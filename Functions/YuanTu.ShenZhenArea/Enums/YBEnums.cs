using System.Xml.Serialization;

namespace YuanTu.ShenZhenArea.Enums
{
    /// <summary>
    /// 参保类型
    ///0	医疗保险参保类型    不参加
    ///1	医疗保险参保类型    基本医疗保险（一档）
    ///2	医疗保险参保类型    基本医疗保险（二档）
    ///3	医疗保险参保类型    特殊
    ///4	医疗保险参保类型    基本医疗保险（三档）
    ///5	医疗保险参保类型    医疗保险二档（少儿）
    ///6	医疗保险参保类型    统筹保险
    /// </summary>
    public enum Cblx
    {
        [XmlEnum("0")]
        不参加 = 0,

        [XmlEnum("1")]
        基本医疗保险一档 = 1,

        [XmlEnum("2")]
        基本医疗保险二档 = 2,

        [XmlEnum("3")]
        特殊 = 3,

        [XmlEnum("4")]
        基本医疗保险三档 = 4,

        [XmlEnum("5")]
        医疗保险二档少儿 = 5,

        [XmlEnum("6")]
        统筹保险 = 6,
    }

    /// <summary>
    /// 病人类型
    /// 1	病人类型 医疗保险
    /// 2	病人类型 生育医疗
    /// 3	病人类型 离休医疗
    /// 4	病人类型 家属统筹医疗
    /// 5	病人类型 工伤医疗
    /// 6	病人类型 农民工医疗(三档）
    /// 7	病人类型 少儿医疗（二档少儿）
    /// 9	病人类型 自费
    /// </summary>
    public enum Brlx
    {
        [XmlEnum("1")]
        医疗保险 = 1,

        [XmlEnum("2")]
        生育医疗 = 2,

        [XmlEnum("3")]
        离休医疗 = 3,

        [XmlEnum("4")]
        家属统筹医疗 = 4,

        [XmlEnum("5")]
        工伤医疗 = 5,

        [XmlEnum("6")]
        少儿医保 = 6,

        [XmlEnum("7")]
        劳务工医保 = 7,

        [XmlEnum("9")]
        自费 = 9,
    }


}