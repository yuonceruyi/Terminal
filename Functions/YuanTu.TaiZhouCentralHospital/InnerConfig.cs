namespace YuanTu.TaiZhouCentralHospital
{
    public class InnerConfig
    {
        
        public static 发卡类型 发卡类型 { get; set; } = 发卡类型.就诊卡;
    }

    public enum 发卡类型
    {
        就诊卡,
        健康卡
    }
}