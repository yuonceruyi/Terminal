namespace YuanTu.NingXiaHospital.HisService
{
    public interface IHead
    {
        string yljgdm { get; set; }
    }

    public class Head : IHead
    {
        public string yljgdm { get; set; } = StaticResource.yljgdm; //"o3WJlghya5vRm4ZsQzchlg==";
    }
}