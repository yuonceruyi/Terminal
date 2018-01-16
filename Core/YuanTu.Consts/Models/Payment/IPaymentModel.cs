using System;
using System.Collections.Generic;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Payment
{
    public interface IPaymentModel : IModel
    {
        ///// <summary>
        ///// 科室名称
        ///// </summary>
        //string Department { get; set; }
        ///// <summary>
        ///// 医生名称
        ///// </summary>
        //string Doctor { get; set; }
        ///// <summary>
        ///// 就诊日期
        ///// </summary>
        //string Date { get; set; }
        ///// <summary>
        ///// 就诊时间
        ///// </summary>
        //string Time { get; set; }
        /// <summary>
        ///     自费金额
        /// </summary>
        decimal Self { get; set; }

        /// <summary>
        ///     医保报销
        /// </summary>
        decimal Insurance { get; set; }

        /// <summary>
        ///     总额
        /// </summary>
        decimal Total { get; set; }

        /// <summary>
        ///     支付方式
        /// </summary>
        PayMethod PayMethod { get; set; }

        /// <summary>
        ///     控制是否实际支付
        /// </summary>
        bool NoPay { get; set; }

        /// <summary>
        ///     具体业务
        /// </summary>
        Func<Result> ConfirmAction { get; set; }

        List<PayInfoItem> LeftList { get; set; }
        List<PayInfoItem> RightList { get; set; }
        List<PayInfoItem> MidList { get; set; }
    }

    public class PaymentModel : ModelBase, IPaymentModel
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
    }
}