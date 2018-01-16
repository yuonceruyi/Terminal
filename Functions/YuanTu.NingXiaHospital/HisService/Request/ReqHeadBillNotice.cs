using System.Collections.Generic;
using YuanTu.NingXiaHospital.HisService.Response;

namespace YuanTu.NingXiaHospital.HisService.Request
{
    public class RequestBillNotice : Request
    {
        public List<ReqDeatilBillNotice> detail { get; set; }
    }

    //结算结果通知 M3
    public class ReqHeadBillNotice : Head
    {
        public string jsid { get; set; }

        public string tzms { get; set; }

        public string yblsh { get; set; }

        public string fy { get; set; }
    }

    public class ReqDeatilBillNotice : IDetail
    {
        public string yblsh { get; set; }

        public string yylsh { get; set; }

        public TradeInfo xjjyfy { get; set; }
    }

    //xjjyfy:{zf_type:(0-支付宝，1-微信，2-银联，3-平安付，4-银行卡),xj_id:"现金支付订单ID",yh_kfh:"银行代码",yh_zh:"银行账号",bz:"备注"}

    public class TradeInfo
    {
        public string bz { get; set; }
        public string xj_id { get; set; }
        public string yh_kfh { get; set; }
        public string yh_zh { get; set; }
        public string zf_type { get; set; }
    }
}