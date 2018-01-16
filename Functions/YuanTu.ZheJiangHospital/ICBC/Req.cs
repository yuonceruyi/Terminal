using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace YuanTu.ZheJiangHospital.ICBC
{
    [XmlType("TransInfo")]
    public class Req充值 : IReq
    {
        public string ClassName => "充值";

        public string TransCode { get; set; } = "20100";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string Chanel { get; set; }
        public string AccountNo { get; set; }
        public string AccountId { get; set; }
        public string TradeMode { get; set; }
        public string MisposSerNo { get; set; }
        public string MisposDate { get; set; }
        public string MisposTime { get; set; }
        public string MisposTermNo { get; set; }
        public string MisposIndexNo { get; set; }
        public string MisposInfo { get; set; }
        public string Cash { get; set; }
        public string BankCardNo { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string Rsv1 { get; set; }
        public string Rsv2 { get; set; }
    }

    [XmlType("TransInfo")]
    public class Req查询虚拟账户余额 : IReq
    {
        public string ClassName => "查询虚拟账户余额";

        public string TransCode { get; set; } = "30100";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string Chanel { get; set; }
        public string AccountNo { get; set; }
        public string AccountId { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string Rsv1 { get; set; }
        public string Rsv2 { get; set; }
    }

    [XmlType("TransInfo")]
    public class Req查询充值退款交易明细 : IReq
    {
        public string ClassName => "查询充值退款交易明细";

        public string TransCode { get; set; } = "30101";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string Chanel { get; set; }
        public string QBeginDay { get; set; }
        public string QEndDay { get; set; }
        public string QAccountNo { get; set; }
        public string QAccountId { get; set; }
        public string QOperid { get; set; }
        public string QDeviceInfo { get; set; }
        public string QTradeSerial { get; set; }
        public string QTradeType { get; set; }
        public string QTradeCh { get; set; }
        public string QCType { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string Rsv1 { get; set; }
        public string Rsv2 { get; set; }
    }

    [XmlType("TransInfo")]
    public class Req轧账停机申请 : IReq
    {
        public string ClassName => "轧账停机申请";

        public string TransCode { get; set; } = "50101";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
    }

    [XmlType("TransInfo")]
    public class Req轧账开机申请 : IReq
    {
        public string ClassName => "轧账开机申请";

        public string TransCode { get; set; } = "50102";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
    }

    [XmlType("TransInfo")]
    public class Req轧账对账申请 : IReq
    {
        public string ClassName => "轧账对账申请";

        public string TransCode { get; set; } = "50103";
        public string HisCode { get; set; } = PConnection.HisCode;
        public string BankAccount { get; set; }
        public string CashCount { get; set; }
        public string CashTotal { get; set; }
        public string Cash100 { get; set; }
        public string Cash50 { get; set; }
        public string Cash20 { get; set; }
        public string Cash10 { get; set; }
        public string TransCount { get; set; }
        public string TransTotal { get; set; }
        public string RefundCount { get; set; }
        public string RefundTotal { get; set; }
        public string OperId { get; set; }
        public string DeviceInfo { get; set; }
        public string TradeSerial { get; set; }
        public string FileName { get; set; }
    }

}

// 17	20100:充值
// 8	30100:查询虚拟账户余额
// 16	30101:查询充值退款交易明细
// 3	50101:轧账停机申请
// 3	50102:轧账开机申请
// 15	50103:轧账对账申请
