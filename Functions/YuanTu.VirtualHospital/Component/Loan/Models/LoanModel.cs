using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.VirtualHospital.Component.Loan.Models
{
    public interface ILoanModel : IModel
    {
        PayMethod RepayMethod { get; set; }

        bool Valid { get; set; }
        decimal HospitalRemainingAmount { get; set; }

        借款账单详情 所选借款账单 { get; set; }

        req查询借款权限 Req查询借款权限 { get; set; }
        res查询借款权限 Res查询借款权限 { get; set; }

        req借款签署协议 Req借款签署协议 { get; set; }
        res借款签署协议 Res借款签署协议 { get; set; }

        req查询借款账单 Req查询借款账单 { get; set; }
        res查询借款账单 Res查询借款账单 { get; set; }

        req查询借款和还款流水 Req查询借款和还款流水 { get; set; }
        res查询借款和还款流水 Res查询借款和还款流水 { get; set; }

        req用户借款还款下单 Req用户借款还款下单 { get; set; }
        res用户借款还款下单 Res用户借款还款下单 { get; set; }

        req查询还款订单状态 Req查询还款订单状态 { get; set; }
        res查询还款订单状态 Res查询还款订单状态 { get; set; }

        req用户借款还款确认 Req用户借款还款确认 { get; set; }
        res用户借款还款确认 Res用户借款还款确认 { get; set; }
    }

    public class LoanModel:ModelBase, ILoanModel
    {
        public PayMethod RepayMethod { get; set; }
        public bool Valid { get; set; }
        public decimal HospitalRemainingAmount { get; set; }
        public 借款账单详情 所选借款账单 { get; set; }
        public req查询借款权限 Req查询借款权限 { get; set; }
        public res查询借款权限 Res查询借款权限 { get; set; }
        public req借款签署协议 Req借款签署协议 { get; set; }
        public res借款签署协议 Res借款签署协议 { get; set; }
        public req查询借款账单 Req查询借款账单 { get; set; }
        public res查询借款账单 Res查询借款账单 { get; set; }
        public req查询借款和还款流水 Req查询借款和还款流水 { get; set; }
        public res查询借款和还款流水 Res查询借款和还款流水 { get; set; }
        public req用户借款还款下单 Req用户借款还款下单 { get; set; }
        public res用户借款还款下单 Res用户借款还款下单 { get; set; }
        public req查询还款订单状态 Req查询还款订单状态 { get; set; }
        public res查询还款订单状态 Res查询还款订单状态 { get; set; }
        public req用户借款还款确认 Req用户借款还款确认 { get; set; }
        public res用户借款还款确认 Res用户借款还款确认 { get; set; }
    }
}
