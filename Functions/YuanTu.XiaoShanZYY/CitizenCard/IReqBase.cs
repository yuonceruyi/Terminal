namespace YuanTu.XiaoShanZYY.CitizenCard
{
    public interface IReqBase
    {
        string Serilize();
        string serviceName { get; set; }
        int transCode { get; set; }
        decimal amount { get; set; }
    }
}
