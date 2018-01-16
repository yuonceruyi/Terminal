  
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace YuanTu.YuHangArea.CYHIS.WebService
{
	public class YIYUANPBXX_IN:IReqBase
	{
		public YIYUANPBXX_IN(){}
		public YIYUANPBXX_IN(
			string PAIBANLX,
			string PAIBANRQ,
			string GUAHAOFS,
			string GUAHAOBC,
			string GUAHAOLB,
			string KESHIDM,
			string YISHENGDM,
			string HUOQUROW,
			string s=null):this()
		{
			this. PAIBANLX=PAIBANLX;
			this. PAIBANRQ=PAIBANRQ;
			this. GUAHAOFS=GUAHAOFS;
			this. GUAHAOBC=GUAHAOBC;
			this. GUAHAOLB=GUAHAOLB;
			this. KESHIDM=KESHIDM;
			this. YISHENGDM=YISHENGDM;
			this. HUOQUROW=HUOQUROW;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string PAIBANLX = "";
		public string PAIBANRQ = "";
		public string GUAHAOFS = "";
		public string GUAHAOBC = "";
		public string GUAHAOLB = "";
		public string KESHIDM = "";
		public string YISHENGDM = "";
		public string HUOQUROW = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(YIYUANPBXX_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "医院排班信息查询" + "\n"
				+ "PAIBANLX:" + PAIBANLX + "\n"
				+ "PAIBANRQ:" + PAIBANRQ + "\n"
				+ "GUAHAOFS:" + GUAHAOFS + "\n"
				+ "GUAHAOBC:" + GUAHAOBC + "\n"
				+ "GUAHAOLB:" + GUAHAOLB + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "YISHENGDM:" + YISHENGDM + "\n"
				+ "HUOQUROW:" + HUOQUROW + "\n"
            ;
		}
	}
	public class GUAHAOYSXX_IN:IReqBase
	{
		public GUAHAOYSXX_IN(){}
		public GUAHAOYSXX_IN(
			string GUAHAOFS,
			string RIQI,
			string GUAHAOBC,
			string KESHIDM,
			string GUAHAOLB,
			string s=null):this()
		{
			this. GUAHAOFS=GUAHAOFS;
			this. RIQI=RIQI;
			this. GUAHAOBC=GUAHAOBC;
			this. KESHIDM=KESHIDM;
			this. GUAHAOLB=GUAHAOLB;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string GUAHAOFS = "";
		public string RIQI = "";
		public string GUAHAOBC = "";
		public string KESHIDM = "";
		public string GUAHAOLB = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOYSXX_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "挂号医生信息" + "\n"
				+ "GUAHAOFS:" + GUAHAOFS + "\n"
				+ "RIQI:" + RIQI + "\n"
				+ "GUAHAOBC:" + GUAHAOBC + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "GUAHAOLB:" + GUAHAOLB + "\n"
            ;
		}
	}
	public class GUAHAOHYXX_IN:IReqBase
	{
		public GUAHAOHYXX_IN(){}
		public GUAHAOHYXX_IN(
			string GUAHAOFS,
			string RIQI,
			string GUAHAOBC,
			string KESHIDM,
			string YISHENGDM,
			string s=null):this()
		{
			this. GUAHAOFS=GUAHAOFS;
			this. RIQI=RIQI;
			this. GUAHAOBC=GUAHAOBC;
			this. KESHIDM=KESHIDM;
			this. YISHENGDM=YISHENGDM;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string GUAHAOFS = "";
		public string RIQI = "";
		public string GUAHAOBC = "";
		public string KESHIDM = "";
		public string YISHENGDM = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOHYXX_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "挂号号源信息" + "\n"
				+ "GUAHAOFS:" + GUAHAOFS + "\n"
				+ "RIQI:" + RIQI + "\n"
				+ "GUAHAOBC:" + GUAHAOBC + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "YISHENGDM:" + YISHENGDM + "\n"
            ;
		}
	}
	public class MENZHENFYMX_IN:IReqBase
	{
		public MENZHENFYMX_IN(){}
		public MENZHENFYMX_IN(
			string JIUZHENKLX,
			string JIUZHENKH,
			string BINGRENLB,
			string BINGRENXZ,
			string YIBAOKLX,
			string YIBAOKMM,
			string YIBAOKXX,
			string YIBAOBRXX,
			string YILIAOLB,
			string JIESUANLB,
			string HISBRXX,
			string GUAHAOID,
			string s=null):this()
		{
			this. JIUZHENKLX=JIUZHENKLX;
			this. JIUZHENKH=JIUZHENKH;
			this. BINGRENLB=BINGRENLB;
			this. BINGRENXZ=BINGRENXZ;
			this. YIBAOKLX=YIBAOKLX;
			this. YIBAOKMM=YIBAOKMM;
			this. YIBAOKXX=YIBAOKXX;
			this. YIBAOBRXX=YIBAOBRXX;
			this. YILIAOLB=YILIAOLB;
			this. JIESUANLB=JIESUANLB;
			this. HISBRXX=HISBRXX;
			this. GUAHAOID=GUAHAOID;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string JIUZHENKLX = "";
		public string JIUZHENKH = "";
		public string BINGRENLB = "";
		public string BINGRENXZ = "";
		public string YIBAOKLX = "";
		public string YIBAOKMM = "";
		public string YIBAOKXX = "";
		public string YIBAOBRXX = "";
		public string YILIAOLB = "";
		public string JIESUANLB = "";
		public string HISBRXX = "";
		public string GUAHAOID = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(MENZHENFYMX_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "门诊费用明细" + "\n"
				+ "JIUZHENKLX:" + JIUZHENKLX + "\n"
				+ "JIUZHENKH:" + JIUZHENKH + "\n"
				+ "BINGRENLB:" + BINGRENLB + "\n"
				+ "BINGRENXZ:" + BINGRENXZ + "\n"
				+ "YIBAOKLX:" + YIBAOKLX + "\n"
				+ "YIBAOKMM:" + YIBAOKMM + "\n"
				+ "YIBAOKXX:" + YIBAOKXX + "\n"
				+ "YIBAOBRXX:" + YIBAOBRXX + "\n"
				+ "YILIAOLB:" + YILIAOLB + "\n"
				+ "JIESUANLB:" + JIESUANLB + "\n"
				+ "HISBRXX:" + HISBRXX + "\n"
				+ "GUAHAOID:" + GUAHAOID + "\n"
            ;
		}
	}
	public class CLINICORDERD_IN:IReqBase
	{
		public CLINICORDERD_IN(){}
		public CLINICORDERD_IN(
			string PATIENTID,
			string OPERATOR,
			string REGDEPTID,
			string DOCTORID,
			string ORDERDATE,
			string CLINICTTYPE,
			string DUTYTYPE,
			string SEQUENCENUM,
			string TELNO,
			string s=null):this()
		{
			this. PATIENTID=PATIENTID;
			this. OPERATOR=OPERATOR;
			this. REGDEPTID=REGDEPTID;
			this. DOCTORID=DOCTORID;
			this. ORDERDATE=ORDERDATE;
			this. CLINICTTYPE=CLINICTTYPE;
			this. DUTYTYPE=DUTYTYPE;
			this. SEQUENCENUM=SEQUENCENUM;
			this. TELNO=TELNO;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string PATIENTID = "";
		public string OPERATOR = "";
		public string REGDEPTID = "";
		public string DOCTORID = "";
		public string ORDERDATE = "";
		public string CLINICTTYPE = "";
		public string DUTYTYPE = "";
		public string SEQUENCENUM = "";
		public string TELNO = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(CLINICORDERD_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "预约挂号处理" + "\n"
				+ "PATIENTID:" + PATIENTID + "\n"
				+ "OPERATOR:" + OPERATOR + "\n"
				+ "REGDEPTID:" + REGDEPTID + "\n"
				+ "DOCTORID:" + DOCTORID + "\n"
				+ "ORDERDATE:" + ORDERDATE + "\n"
				+ "CLINICTTYPE:" + CLINICTTYPE + "\n"
				+ "DUTYTYPE:" + DUTYTYPE + "\n"
				+ "SEQUENCENUM:" + SEQUENCENUM + "\n"
				+ "TELNO:" + TELNO + "\n"
            ;
		}
	}
	public class GUAHAOYYTH_IN:IReqBase
	{
		public GUAHAOYYTH_IN(){}
		public GUAHAOYYTH_IN(
			string JIUZHENKLX,
			string JIUZHENKH,
			string ZHENGJIANLX,
			string ZHENGJIANHM,
			string XINGMING,
			string YUYUELY,
			string QUHAOMM,
			string s=null):this()
		{
			this. JIUZHENKLX=JIUZHENKLX;
			this. JIUZHENKH=JIUZHENKH;
			this. ZHENGJIANLX=ZHENGJIANLX;
			this. ZHENGJIANHM=ZHENGJIANHM;
			this. XINGMING=XINGMING;
			this. YUYUELY=YUYUELY;
			this. QUHAOMM=QUHAOMM;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string JIUZHENKLX = "";
		public string JIUZHENKH = "";
		public string ZHENGJIANLX = "";
		public string ZHENGJIANHM = "";
		public string XINGMING = "";
		public string YUYUELY = "";
		public string QUHAOMM = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(GUAHAOYYTH_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "预约退号处理" + "\n"
				+ "JIUZHENKLX:" + JIUZHENKLX + "\n"
				+ "JIUZHENKH:" + JIUZHENKH + "\n"
				+ "ZHENGJIANLX:" + ZHENGJIANLX + "\n"
				+ "ZHENGJIANHM:" + ZHENGJIANHM + "\n"
				+ "XINGMING:" + XINGMING + "\n"
				+ "YUYUELY:" + YUYUELY + "\n"
				+ "QUHAOMM:" + QUHAOMM + "\n"
            ;
		}
	}
	public class CashInfo_IN:IReqBase
	{
		public CashInfo_IN(){}
		public CashInfo_IN(
			string OperatorNo,
			string Time,
			string CardNo,
			string Amount,
			string TranSeq,
			string s=null):this()
		{
			this. OperatorNo=OperatorNo;
			this. Time=Time;
			this. CardNo=CardNo;
			this. Amount=Amount;
			this. TranSeq=TranSeq;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string OperatorNo = "";
		public string Time = "";
		public string CardNo = "";
		public string Amount = "";
		public string TranSeq = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(CashInfo_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "现金投币" + "\n"
				+ "OperatorNo:" + OperatorNo + "\n"
				+ "Time:" + Time + "\n"
				+ "CardNo:" + CardNo + "\n"
				+ "Amount:" + Amount + "\n"
				+ "TranSeq:" + TranSeq + "\n"
            ;
		}
	}
	public class ZXPOSInfo_IN:IReqBase
	{
		public ZXPOSInfo_IN(){}
		public ZXPOSInfo_IN(
			string OperatorNo,
			string Amount,
			string TransSeq,
			string CardNo,
			string Type,
			string time,
			string s=null):this()
		{
			this. OperatorNo=OperatorNo;
			this. Amount=Amount;
			this. TransSeq=TransSeq;
			this. CardNo=CardNo;
			this. Type=Type;
			this. time=time;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string OperatorNo = "";
		public string Amount = "";
		public string TransSeq = "";
		public string CardNo = "";
		public string Type = "";
		public string time = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(ZXPOSInfo_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "银联交易" + "\n"
				+ "OperatorNo:" + OperatorNo + "\n"
				+ "Amount:" + Amount + "\n"
				+ "TransSeq:" + TransSeq + "\n"
				+ "CardNo:" + CardNo + "\n"
				+ "Type:" + Type + "\n"
				+ "time:" + time + "\n"
            ;
		}
	}
	public class RechargeInfo_IN:IReqBase
	{
		public RechargeInfo_IN(){}
		public RechargeInfo_IN(
			string OperatorNo,
			string CardNo,
			string RechargeMethod,
			string Amount,
			string BankCardNo,
			string BankTranSeq,
			string BankDate,
			string DealDate,
			string TransSeq,
			string RemainAmount,
			string s=null):this()
		{
			this. OperatorNo=OperatorNo;
			this. CardNo=CardNo;
			this. RechargeMethod=RechargeMethod;
			this. Amount=Amount;
			this. BankCardNo=BankCardNo;
			this. BankTranSeq=BankTranSeq;
			this. BankDate=BankDate;
			this. DealDate=DealDate;
			this. TransSeq=TransSeq;
			this. RemainAmount=RemainAmount;
		}

//		public string TradeCode = "HIS1";
		public BASEINFO BASEINFO;
		public string OperatorNo = "";
		public string CardNo = "";
		public string RechargeMethod = "";
		public string Amount = "";
		public string BankCardNo = "";
		public string BankTranSeq = "";
		public string BankDate = "";
		public string DealDate = "";
		public string TransSeq = "";
		public string RemainAmount = "";

	   public string Serilize()
		{
			return Utility.Serilize(typeof(RechargeInfo_IN));
		}
		public override string ToString()
		{
			return "NAME:" + "充值交易" + "\n"
				+ "OperatorNo:" + OperatorNo + "\n"
				+ "CardNo:" + CardNo + "\n"
				+ "RechargeMethod:" + RechargeMethod + "\n"
				+ "Amount:" + Amount + "\n"
				+ "BankCardNo:" + BankCardNo + "\n"
				+ "BankTranSeq:" + BankTranSeq + "\n"
				+ "BankDate:" + BankDate + "\n"
				+ "DealDate:" + DealDate + "\n"
				+ "TransSeq:" + TransSeq + "\n"
				+ "RemainAmount:" + RemainAmount + "\n"
            ;
		}
	}
}





