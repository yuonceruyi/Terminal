using System;
using System.ComponentModel;

namespace YuanTu.Consts.Enums
{
    [Flags]
    public enum PayMethod
    {
        未知 = 0,

        [Description("CA")]
        现金 = 1,

        [Description("DB")]
        银联 = 2,

        [Description("OC")]
        预缴金 = 4,

        [Description("LOAN")]
        先诊疗后付费 = 8,

        [Description("MIC")]
        社保 = 16,

        [Description("ZFB")]
        支付宝 = 32,

        [Description("WX")]
        微信支付 = 64,

        [Description("AP")]
        苹果支付 = 128,

        [Description("ZHYL")]
        智慧医疗 = 256,

        [Description("SF")]
        银联闪付 = 512,
    }

    //[Flags]
    //public enum RechargeMethod
    //{
    //    未知 = 0,
    //    [Description("CA")]
    //    现金 = 1,

    //    [Description("DB")]
    //    银联 = 2,

    //    [Description("OC")]
    //    支付宝 = 4,

    //    [Description("WX")]
    //    微信 = 8,

    //    [Description("AP")]
    //    苹果 = 16
    //}
}