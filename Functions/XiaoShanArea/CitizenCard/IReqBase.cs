namespace YuanTu.YuHangArea.CitizenCard
{
    public interface IReqBase
    {
         string Serilize();
         string serviceName { get; set; }
         decimal transCode { get; set; }
         decimal amount { get; set; }
    }
}
