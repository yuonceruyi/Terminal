using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace YuanTu.ZheJiangHospital.ICBC
{ 
  
    [XmlType("TransInfo")]
    public class Res充值 : IRes
    {
        public string ClassName => "充值";

        //public string TradeCode { get; set; } = "20100";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string BankSerial { get; set; }
        public string Cash { get; set; }
        public string BankWorkDT { get; set; }
        public string Remain { get; set; }
    }
  
    [XmlType("TransInfo")]
    public class Res查询虚拟账户余额 : IRes
    {
        public string ClassName => "查询虚拟账户余额";

        //public string TradeCode { get; set; } = "30100";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string Remain { get; set; }
        public string ComLimit { get; set; }
    }
  
    [XmlType("TransInfo")]
    public class Res查询充值退款交易明细 : IRes
    {
        public string ClassName => "查询充值退款交易明细";

        //public string TradeCode { get; set; } = "30101";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string FileName { get; set; }
        public string Count { get; set; }
    }
  
    [XmlType("TransInfo")]
    public class Res轧账停机申请 : IRes
    {
        public string ClassName => "轧账停机申请";

        //public string TradeCode { get; set; } = "50101";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string BankSerial { get; set; }
        public string BankWorkDate { get; set; }
    }
  
    [XmlType("TransInfo")]
    public class Res轧账开机申请 : IRes
    {
        public string ClassName => "轧账开机申请";

        //public string TradeCode { get; set; } = "50102";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string BankSerial { get; set; }
        public string BankWorkDate { get; set; }
    }
  
    [XmlType("TransInfo")]
    public class Res轧账对账申请 : IRes
    {
        public string ClassName => "轧账对账申请";

        //public string TradeCode { get; set; } = "50103";
        public string ResultFlag { get; set; }
        public string ResultMark { get; set; }
        public string BankSerial { get; set; }
        public string CashCount { get; set; }
        public string CashTotal { get; set; }
        public string TransCount { get; set; }
        public string TransTotal { get; set; }
        public string RefundCount { get; set; }
        public string RefundTotal { get; set; }
        public string BankMemo { get; set; }
    }
}

// 4	20100:充值
// 2	30100:查询虚拟账户余额
// 2	30101:查询充值退款交易明细
// 2	50101:轧账停机申请
// 2	50102:轧账开机申请
// 8	50103:轧账对账申请
