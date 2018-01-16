  
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace YuanTu.ZheJiangZhongLiuHospital.ICBC
{ 
  
	[XmlType("TransInfo")]
	public class Res充值 : IRes
	{
		public string ClassName => "Res充值";

//		public string TradeCode { get; set; } = "20100";
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
		public string ClassName => "Res查询虚拟账户余额";

//		public string TradeCode { get; set; } = "30100";
		public string ResultFlag { get; set; }
		public string ResultMark { get; set; }
		public string Remain { get; set; }
		public string ComLimit { get; set; }
	}
  
	[XmlType("TransInfo")]
	public class Res查询充值退款交易明细 : IRes
	{
		public string ClassName => "Res查询充值退款交易明细";

//		public string TradeCode { get; set; } = "30101";
		public string ResultFlag { get; set; }
		public string ResultMark { get; set; }
		public string FileName { get; set; }
		public string Count { get; set; }
	}
  
	[XmlType("TransInfo")]
	public class Res轧账停机申请 : IRes
	{
		public string ClassName => "Res轧账停机申请";

//		public string TradeCode { get; set; } = "50101";
		public string ResultFlag { get; set; }
		public string ResultMark { get; set; }
		public string BankSerial { get; set; }
		public string BankWorkDate { get; set; }
	}
  
	[XmlType("TransInfo")]
	public class Res轧账开机申请 : IRes
	{
		public string ClassName => "Res轧账开机申请";

//		public string TradeCode { get; set; } = "50102";
		public string ResultFlag { get; set; }
		public string ResultMark { get; set; }
		public string BankSerial { get; set; }
		public string BankWorkDate { get; set; }
	}
  
	[XmlType("TransInfo")]
	public class Res轧账对账申请 : IRes
	{
		public string ClassName => "Res轧账对账申请";

//		public string TradeCode { get; set; } = "50103";
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
  
	[XmlType("TransInfo")]
	public class Res开户 : IRes
	{
		public string ClassName => "Res开户";

//		public string TradeCode { get; set; } = "10100";
		public string ResultFlag { get; set; }
		public string ResultMark { get; set; }
		public string BankSerial { get; set; }
		public string AccountNo { get; set; }
	}
}

// 4	20100:Res充值
// 2	30100:Res查询虚拟账户余额
// 2	30101:Res查询充值退款交易明细
// 2	50101:Res轧账停机申请
// 2	50102:Res轧账开机申请
// 8	50103:Res轧账对账申请
// 2	10100:Res开户
