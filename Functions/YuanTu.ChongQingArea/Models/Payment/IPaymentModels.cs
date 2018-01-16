using System;
using System.Collections.Generic;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Payment;

namespace YuanTu.ChongQingArea.Models.Payment
{
    public  interface  IPaymentModels:YuanTu.Consts.Models.Payment.IPaymentModel
    {
        /// <summary>
        /// 公务员补助
        /// </summary>
        decimal CivilServant { get; set; }

        /// <summary>
        /// 统筹支付
        /// </summary>
        decimal FundPayment { get; set; }

        /// <summary>
        /// 账户支付
        /// </summary>
        decimal AccountPay { get; set; }

    }

    public class PaymentModel : ModelBase, IPaymentModels
    {
        public string Department { get; set; }
        public string Doctor { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public PayMethod PayMethod { get; set; }
        public bool NoPay { get; set; }
        public Func<Result> ConfirmAction { get; set; }
        public List<PayInfoItem> LeftList { get; set; }
        public List<PayInfoItem> RightList { get; set; }
        public List<PayInfoItem> MidList { get; set; }
        public decimal Self { get; set; }
        public decimal Insurance { get; set; }
        public decimal Total { get; set; }
        public decimal CivilServant { get; set; }
        public decimal FundPayment { get; set; }
        public decimal AccountPay { get; set; }
    }
}
