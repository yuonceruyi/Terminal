namespace YuanTu.YuHangFYBJY.NativeService.Dto
{
    /// <summary>
    /// 预结算请求
    /// </summary>
    public class PerRegisterPayRequest: RequestBase
    {
       
        /// <summary>
        /// 排班ID
        /// </summary>
        public string PaiBanId { get; set; }
        /// <summary>
        /// 上下午标志
        /// </summary>
        public DayTimeFlag TimeFlag { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public PayMedhodFlag PayFlag { get; set; }
       

        public override int BussinessType { get; } = 3;
    }
}