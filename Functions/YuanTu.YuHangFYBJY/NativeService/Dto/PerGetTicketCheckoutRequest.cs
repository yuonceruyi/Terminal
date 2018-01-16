using System;

namespace YuanTu.YuHangFYBJY.NativeService.Dto
{
    public class PerGetTicketCheckoutRequest:RequestBase
    {
        /// <summary>
        /// 业务类型，预约取号预结算为5
        /// </summary>
        public override int BussinessType { get; } = 5;

        /// <summary>
        /// 预约ID
        /// </summary>
        public string AppointmentId { get; set; }

        /// <summary>
        /// 预约时间，限定格式为:yyyy-MM-dd
        /// </summary>
        public DateTime AppointmentTime { get; set; }

        /// <summary>
        /// 上下午标志
        /// </summary>
        public DayTimeFlag DayTimeFlag { get; set; }
        /// <summary>
        /// 挂号序号
        /// </summary>
        public string RegisterOrder { get; set; }

        /// <summary>
        /// 挂号类别
        /// </summary>
        public RegisterType RegisterType { get; set; } = RegisterType.普通;
        /// <summary>
        /// 结算方式
        /// </summary>
        public PayMedhodFlag PayFlag { get; set; }


    }
}
