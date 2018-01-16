using System;

namespace YuanTu.ChongQingArea.SiHandler
{
    internal static class SiExtentins
    {
        public static decimal SumNonSelf(this Res预结算 res)
        {
            var 统筹支付 = Convert.ToDecimal(res.统筹支付);
            var 帐户支付 = Convert.ToDecimal(res.帐户支付);
            var 公务员补助 = Convert.ToDecimal(res.公务员补助);
            var 大额理赔金额 = Convert.ToDecimal(res.大额理赔金额);
            var 历史起付线公务员返还 = Convert.ToDecimal(res.历史起付线公务员返还);
            var 单病种定点医疗机构垫支 = Convert.ToDecimal(res.单病种定点医疗机构垫支);
            var 民政救助金额 = Convert.ToDecimal(res.民政救助金额);
            return 统筹支付 + 帐户支付 + 公务员补助 + 大额理赔金额 + 历史起付线公务员返还 + 单病种定点医疗机构垫支 + 民政救助金额;
        }
    }
}