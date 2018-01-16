namespace YuanTu.YuHangSecondHospital.NativeService.Dto
{
    public class GetTicketCheckoutRequest:PerGetTicketCheckoutRequest
    {
        /// <summary>
        /// 业务类型，预约取号结算为6
        /// </summary>
        public override int BussinessType { get; } = 6;
    }
}
