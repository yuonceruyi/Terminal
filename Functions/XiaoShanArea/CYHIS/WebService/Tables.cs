  
  
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace YuanTu.YuHangArea.CYHIS.WebService
{
   
	[XmlType("BASEINFO")]
	public class BASEINFO
	{
		private string myName =  "BASEINFO";

		public string CAOZUOYDM{get;set;}
		public string CAOZUOYXM{get;set;}
		public string CAOZUORQ{get;set;}
		public string XITONGBS{get;set;}
		public string FENYUANDM{get;set;}
		public string JIGOUDM{get;set;}
		public string JESHOUJGDM{get;set;}
		public string ZHONGDUANJBH{get;set;}
		public string ZHONGDUANLSH{get;set;}
		public string MessageID{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("OUTMSG")]
	public class OUTMSG
	{
		private string myName =  "OUTMSG";

		public string ERRNO{get;set;}
		public string ERRMSG{get;set;}
		public string ZHONGDUANJBH{get;set;}
		public string ZHONGDUANLSH{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("PAIBANMX")]
	public class PAIBANMX
	{
		private string myName =  "PAIBANMX";

		public string KESHIDM{get;set;}
		public string KESHIMC{get;set;}
		public string JIUZHENDD{get;set;}
		public string KESHIJS{get;set;}
		public string YISHENGDM{get;set;}
		public string YISHENGXM{get;set;}
		public string YISHENGZC{get;set;}
		public string YISHENGTC{get;set;}
		public string YISHENGJS{get;set;}
		public string SHANGWUHYZS{get;set;}
		public string SHANGWUHYSYS{get;set;}
		public string XIAWUHYZS{get;set;}
		public string XIAWUHYSYS{get;set;}
		public string ZHENLIAOJSF{get;set;}
		public string ZHENLIAOF{get;set;}
		public string PAIBANRQ{get;set;}
		public string GUAHAOBC{get;set;}
		public string GUAHAOLB{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("YISHENGXX")]
	public class YISHENGXX
	{
		private string myName =  "YISHENGXX";

		public string YISHENGDM{get;set;}
		public string YISHENGXM{get;set;}
		public string KESHIDM{get;set;}
		public string KESHIMC{get;set;}
		public string YISHENGZC{get;set;}
		public string YISHENGTC{get;set;}
		public string YISHENGJS{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("JIBINGXX")]
	public class JIBINGXX
	{
		private string myName =  "JIBINGXX";

		public string JIBINGDM{get;set;}
		public string JIBINGICD{get;set;}
		public string JIBINGMC{get;set;}
		public string JIBINGMS{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("FEIYONGXX")]
	public class FEIYONGXX
	{
		private string myName =  "FEIYONGXX";

		public string CHUFANGLX{get;set;}
		public string CHUFANGXH{get;set;}
		public string MINGXIXH{get;set;}
		public string FEIYONGLX{get;set;}
		public string XIANGMUXH{get;set;}
		public string XIANGMUCDDM{get;set;}
		public string XIANGMUMC{get;set;}
		public string XIANGMUGL{get;set;}
		public string XIANGMUGLMC{get;set;}
		public string XIANGMUGG{get;set;}
		public string XIANGMUJX{get;set;}
		public string XIANGMUDW{get;set;}
		public string XIANGMUCDMC{get;set;}
		public string ZHONGCAOYTS{get;set;}
		public string DANJIA{get;set;}
		public string SHULIANG{get;set;}
		public string JINE{get;set;}
		public string KAIDANKSDM{get;set;}
		public string KAIDANKSMC{get;set;}
		public string KAIDANYSDM{get;set;}
		public string KAIDANYSXM{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("HAOYUANXX")]
	public class HAOYUANXX
	{
		private string myName =  "HAOYUANXX";

		public string RIQI{get;set;}
		public string GUAHAOBC{get;set;}
		public string GUAHAOLB{get;set;}
		public string KESHIDM{get;set;}
		public string YISHENGDM{get;set;}
		public string GUAHAOXH{get;set;}
		public string JIUZHENSJ{get;set;}
		public string YIZHOUPBID{get;set;}
		public string DANGTIANPBID{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
  
	[XmlType("MENZHENFYXX")]
	public class MENZHENFYXX
	{
		private string myName =  "MENZHENFYXX";

		public string CHUFANGLX{get;set;}
		public string CHUFANGXH{get;set;}
		public string MINGXIXH{get;set;}
		public string FEIYONGLX{get;set;}
		public string XIANGMUXH{get;set;}
		public string XIANGMUCDDM{get;set;}
		public string XIANGMUMC{get;set;}
		public string XIANGMUGL{get;set;}
		public string XIANGMUGLMC{get;set;}
		public string XIANGMUGG{get;set;}
		public string XIANGMUJX{get;set;}
		public string XIANGMUDW{get;set;}
		public string XIANGMUCDMC{get;set;}
		public string ZHONGCAOYTS{get;set;}
		public string BAOZHUANGDW{get;set;}
		public string ZUIXIAOJLDW{get;set;}
		public string DANCIYL{get;set;}
		public string YONGLIANGDW{get;set;}
		public string MEITIANCS{get;set;}
		public string YONGYAOTS{get;set;}
		public string DANFUFBZ{get;set;}
		public string DANJIA{get;set;}
		public string SHULIANG{get;set;}
		public string JINE{get;set;}
		public string KAIDANKSDM{get;set;}
		public string KAIDANKSMC{get;set;}
		public string KAIDANYSDM{get;set;}
		public string KAIDANYSXM{get;set;}

		public string GetMyName()
		{
			return myName;
		}
		
	}
}