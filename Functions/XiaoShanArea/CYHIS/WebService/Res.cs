  
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace YuanTu.YuHangArea.CYHIS.WebService
{ 
  
	[XmlType("YIYUANPBXX_OUT")]
	public class YIYUANPBXX_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;
		public List<PAIBANMX> PAIBANLB;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(YIYUANPBXX_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" YIYUANPBXX"+ "\n";
				text+= "List<PAIBANMX> PAIBANLB"+"\n"
				+ PAIBANLB.Aggregate("",(s,one)=>s+=one.ToString());
            return text;
		}
	}
  
	[XmlType("GUAHAOYSXX_OUT")]
	public class GUAHAOYSXX_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;
		public List<YISHENGXX> YISHENGMX;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOYSXX_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" GUAHAOYSXX"+ "\n";
				text+= "List<YISHENGXX> YISHENGMX"+"\n"
				+ YISHENGMX.Aggregate("",(s,one)=>s+=one.ToString());
            return text;
		}
	}
  
	[XmlType("GUAHAOHYXX_OUT")]
	public class GUAHAOHYXX_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;
		public List<HAOYUANXX> HAOYUANMX;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOHYXX_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" GUAHAOHYXX"+ "\n";
				text+= "List<HAOYUANXX> HAOYUANMX"+"\n"
				+ HAOYUANMX.Aggregate("",(s,one)=>s+=one.ToString());
            return text;
		}
	}
  
	[XmlType("MENZHENFYMX_OUT")]
	public class MENZHENFYMX_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;
		public string YILIAOLB;
		public List<JIBINGXX> JIBINGMX;
		public string FEIYONGMXTS;
		public List<MENZHENFYXX> FEIYONGMX;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(MENZHENFYMX_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" MENZHENFYMX"+ "\n";
				text+= "YILIAOLB:" + YILIAOLB + "\n";
				text+= "List<JIBINGXX> JIBINGMX"+"\n"
				+ JIBINGMX.Aggregate("",(s,one)=>s+=one.ToString());
				text+= "FEIYONGMXTS:" + FEIYONGMXTS + "\n";
				text+= "List<MENZHENFYXX> FEIYONGMX"+"\n"
				+ FEIYONGMX.Aggregate("",(s,one)=>s+=one.ToString());
            return text;
		}
	}
  
	[XmlType("CLINICORDERD_OUT")]
	public class CLINICORDERD_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;
		public string ORDERNUM;
		public string CLINICTTIME;
		public string SEQUENCENUM;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(CLINICORDERD_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" CLINICORDERD"+ "\n";
				text+= "ORDERNUM:" + ORDERNUM + "\n";
				text+= "CLINICTTIME:" + CLINICTTIME + "\n";
				text+= "SEQUENCENUM:" + SEQUENCENUM + "\n";
            return text;
		}
	}
  
	[XmlType("GUAHAOYYTH_OUT")]
	public class GUAHAOYYTH_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOYYTH_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" GUAHAOYYTH"+ "\n";
            return text;
		}
	}
  
	[XmlType("CashInfo_OUT")]
	public class CashInfo_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(CashInfo_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" CashInfo"+ "\n";
            return text;
		}
	}
  
	[XmlType("ZXPOSInfo_OUT")]
	public class ZXPOSInfo_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(ZXPOSInfo_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" ZXPOSInfo"+ "\n";
            return text;
		}
	}
  
	[XmlType("RechargeInfo_OUT")]
	public class RechargeInfo_OUT:IResBase
	{

//		public string TradeCode = "HIS1.Biz.";
		public OUTMSG OUTMSG;

		  public string Serilize()
		{
			return Utility.Serilize(typeof(RechargeInfo_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" RechargeInfo"+ "\n";
            return text;
		}
	}
}

// 1	HIS1.Biz.:YIYUANPBXX
// 1	HIS1.Biz.:GUAHAOYSXX
// 1	HIS1.Biz.:GUAHAOHYXX
// 4	HIS1.Biz.:MENZHENFYMX
// 3	HIS1.Biz.:CLINICORDERD
// 0	HIS1.Biz.:GUAHAOYYTH
// 0	HIS1.Biz.:CashInfo
// 0	HIS1.Biz.:ZXPOSInfo
// 0	HIS1.Biz.:RechargeInfo
