using System.Xml.Serialization;

namespace YuanTu.YuHangArea.CYHIS.DLL
{
   
	[XmlType("BASEINFO")]
	public class BASEINFO
	{
		private string myName =  "BASEINFO";

		public string CAOZUOYDM;
		public string CAOZUOYXM;
		public string CAOZUORQ;
		public string XITONGBS;
		public string FENYUANDM;
		public string JIGOUDM;
		public string JESHOUJGDM;
		public string ZHONGDUANJBH;
		public string ZHONGDUANLSH;
		public string MessageID;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "CAOZUOYDM:" + CAOZUOYDM + "\n"
				+ "CAOZUOYXM:" + CAOZUOYXM + "\n"
				+ "CAOZUORQ:" + CAOZUORQ + "\n"
				+ "XITONGBS:" + XITONGBS + "\n"
				+ "FENYUANDM:" + FENYUANDM + "\n"
				+ "JIGOUDM:" + JIGOUDM + "\n"
				+ "JESHOUJGDM:" + JESHOUJGDM + "\n"
				+ "ZHONGDUANJBH:" + ZHONGDUANJBH + "\n"
				+ "ZHONGDUANLSH:" + ZHONGDUANLSH + "\n"
				+ "MessageID:" + MessageID + "\n"
            ;
		}
	}
  
	[XmlType("OUTMSG")]
	public class OUTMSG
	{
		private string myName =  "OUTMSG";

		public string ERRNO;
		public string ERRMSG;
		public string ZHONGDUANJBH;
		public string ZHONGDUANLSH;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "ERRNO:" + ERRNO + "\n"
				+ "ERRMSG:" + ERRMSG + "\n"
				+ "ZHONGDUANJBH:" + ZHONGDUANJBH + "\n"
				+ "ZHONGDUANLSH:" + ZHONGDUANLSH + "\n"
            ;
		}
	}
  
	[XmlType("PAIBANMX")]
	public class PAIBANMX
	{
		private string myName =  "PAIBANMX";

		public string KESHIDM;
		public string KESHIMC;
		public string JIUZHENDD;
		public string KESHIJS;
		public string YISHENGDM;
		public string YISHENGXM;
		public string YISHENGZC;
		public string YISHENGTC;
		public string YISHENGJS;
		public string SHANGWUHYZS;
		public string SHANGWUHYSYS;
		public string XIAWUHYZS;
		public string XIAWUHYSYS;
		public string ZHENLIAOJSF;
		public string ZHENLIAOF;
		public string PAIBANRQ;
		public string GUAHAOBC;
		public string GUAHAOLB;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "KESHIMC:" + KESHIMC + "\n"
				+ "JIUZHENDD:" + JIUZHENDD + "\n"
				+ "KESHIJS:" + KESHIJS + "\n"
				+ "YISHENGDM:" + YISHENGDM + "\n"
				+ "YISHENGXM:" + YISHENGXM + "\n"
				+ "YISHENGZC:" + YISHENGZC + "\n"
				+ "YISHENGTC:" + YISHENGTC + "\n"
				+ "YISHENGJS:" + YISHENGJS + "\n"
				+ "SHANGWUHYZS:" + SHANGWUHYZS + "\n"
				+ "SHANGWUHYSYS:" + SHANGWUHYSYS + "\n"
				+ "XIAWUHYZS:" + XIAWUHYZS + "\n"
				+ "XIAWUHYSYS:" + XIAWUHYSYS + "\n"
				+ "ZHENLIAOJSF:" + ZHENLIAOJSF + "\n"
				+ "ZHENLIAOF:" + ZHENLIAOF + "\n"
				+ "PAIBANRQ:" + PAIBANRQ + "\n"
				+ "GUAHAOBC:" + GUAHAOBC + "\n"
				+ "GUAHAOLB:" + GUAHAOLB + "\n"
            ;
		}
	}
  
	[XmlType("YISHENGXX")]
	public class YISHENGXX
	{
		private string myName =  "YISHENGXX";

		public string YISHENGDM;
		public string YISHENGXM;
		public string KESHIDM;
		public string KESHIMC;
		public string YISHENGZC;
		public string YISHENGTC;
		public string YISHENGJS;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "YISHENGDM:" + YISHENGDM + "\n"
				+ "YISHENGXM:" + YISHENGXM + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "KESHIMC:" + KESHIMC + "\n"
				+ "YISHENGZC:" + YISHENGZC + "\n"
				+ "YISHENGTC:" + YISHENGTC + "\n"
				+ "YISHENGJS:" + YISHENGJS + "\n"
            ;
		}
	}
  
	[XmlType("JIBINGXX")]
	public class JIBINGXX
	{
		private string myName =  "JIBINGXX";

		public string JIBINGDM;
		public string JIBINGICD;
		public string JIBINGMC;
		public string JIBINGMS;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "JIBINGDM:" + JIBINGDM + "\n"
				+ "JIBINGICD:" + JIBINGICD + "\n"
				+ "JIBINGMC:" + JIBINGMC + "\n"
				+ "JIBINGMS:" + JIBINGMS + "\n"
            ;
		}
	}
  
	[XmlType("FEIYONGXX")]
	public class FEIYONGXX
	{
		private string myName =  "FEIYONGXX";

		public string CHUFANGLX;
		public string CHUFANGXH;
		public string MINGXIXH;
		public string FEIYONGLX;
		public string XIANGMUXH;
		public string XIANGMUCDDM;
		public string XIANGMUMC;
		public string XIANGMUGL;
		public string XIANGMUGLMC;
		public string XIANGMUGG;
		public string XIANGMUJX;
		public string XIANGMUDW;
		public string XIANGMUCDMC;
		public string ZHONGCAOYTS;
		public string DANJIA;
		public string SHULIANG;
		public string JINE;
		public string KAIDANKSDM;
		public string KAIDANKSMC;
		public string KAIDANYSDM;
		public string KAIDANYSXM;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "CHUFANGLX:" + CHUFANGLX + "\n"
				+ "CHUFANGXH:" + CHUFANGXH + "\n"
				+ "MINGXIXH:" + MINGXIXH + "\n"
				+ "FEIYONGLX:" + FEIYONGLX + "\n"
				+ "XIANGMUXH:" + XIANGMUXH + "\n"
				+ "XIANGMUCDDM:" + XIANGMUCDDM + "\n"
				+ "XIANGMUMC:" + XIANGMUMC + "\n"
				+ "XIANGMUGL:" + XIANGMUGL + "\n"
				+ "XIANGMUGLMC:" + XIANGMUGLMC + "\n"
				+ "XIANGMUGG:" + XIANGMUGG + "\n"
				+ "XIANGMUJX:" + XIANGMUJX + "\n"
				+ "XIANGMUDW:" + XIANGMUDW + "\n"
				+ "XIANGMUCDMC:" + XIANGMUCDMC + "\n"
				+ "ZHONGCAOYTS:" + ZHONGCAOYTS + "\n"
				+ "DANJIA:" + DANJIA + "\n"
				+ "SHULIANG:" + SHULIANG + "\n"
				+ "JINE:" + JINE + "\n"
				+ "KAIDANKSDM:" + KAIDANKSDM + "\n"
				+ "KAIDANKSMC:" + KAIDANKSMC + "\n"
				+ "KAIDANYSDM:" + KAIDANYSDM + "\n"
				+ "KAIDANYSXM:" + KAIDANYSXM + "\n"
            ;
		}
	}
  
	[XmlType("HAOYUANXX")]
	public class HAOYUANXX
	{
		private string myName =  "HAOYUANXX";

		public string RIQI;
		public string GUAHAOBC;
		public string GUAHAOLB;
		public string KESHIDM;
		public string YISHENGDM;
		public string GUAHAOXH;
		public string JIUZHENSJ;
		public string YIZHOUPBID;
		public string DANGTIANPBID;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "RIQI:" + RIQI + "\n"
				+ "GUAHAOBC:" + GUAHAOBC + "\n"
				+ "GUAHAOLB:" + GUAHAOLB + "\n"
				+ "KESHIDM:" + KESHIDM + "\n"
				+ "YISHENGDM:" + YISHENGDM + "\n"
				+ "GUAHAOXH:" + GUAHAOXH + "\n"
				+ "JIUZHENSJ:" + JIUZHENSJ + "\n"
				+ "YIZHOUPBID:" + YIZHOUPBID + "\n"
				+ "DANGTIANPBID:" + DANGTIANPBID + "\n"
            ;
		}
	}
  
	[XmlType("MENZHENFYXX")]
	public class MENZHENFYXX
	{
		private string myName =  "MENZHENFYXX";

		public string CHUFANGLX;
		public string CHUFANGXH;
		public string MINGXIXH;
		public string FEIYONGLX;
		public string XIANGMUXH;
		public string XIANGMUCDDM;
		public string XIANGMUMC;
		public string XIANGMUGL;
		public string XIANGMUGLMC;
		public string XIANGMUGG;
		public string XIANGMUJX;
		public string XIANGMUDW;
		public string XIANGMUCDMC;
		public string ZHONGCAOYTS;
		public string BAOZHUANGDW;
		public string ZUIXIAOJLDW;
		public string DANCIYL;
		public string YONGLIANGDW;
		public string MEITIANCS;
		public string YONGYAOTS;
		public string DANFUFBZ;
		public string DANJIA;
		public string SHULIANG;
		public string JINE;
		public string KAIDANKSDM;
		public string KAIDANKSMC;
		public string KAIDANYSDM;
		public string KAIDANYSXM;

		public string GetMyName()
		{
			return myName;
		}
		public override string ToString()
		{
			return "NAME:" + myName + "\n"
				+ "CHUFANGLX:" + CHUFANGLX + "\n"
				+ "CHUFANGXH:" + CHUFANGXH + "\n"
				+ "MINGXIXH:" + MINGXIXH + "\n"
				+ "FEIYONGLX:" + FEIYONGLX + "\n"
				+ "XIANGMUXH:" + XIANGMUXH + "\n"
				+ "XIANGMUCDDM:" + XIANGMUCDDM + "\n"
				+ "XIANGMUMC:" + XIANGMUMC + "\n"
				+ "XIANGMUGL:" + XIANGMUGL + "\n"
				+ "XIANGMUGLMC:" + XIANGMUGLMC + "\n"
				+ "XIANGMUGG:" + XIANGMUGG + "\n"
				+ "XIANGMUJX:" + XIANGMUJX + "\n"
				+ "XIANGMUDW:" + XIANGMUDW + "\n"
				+ "XIANGMUCDMC:" + XIANGMUCDMC + "\n"
				+ "ZHONGCAOYTS:" + ZHONGCAOYTS + "\n"
				+ "BAOZHUANGDW:" + BAOZHUANGDW + "\n"
				+ "ZUIXIAOJLDW:" + ZUIXIAOJLDW + "\n"
				+ "DANCIYL:" + DANCIYL + "\n"
				+ "YONGLIANGDW:" + YONGLIANGDW + "\n"
				+ "MEITIANCS:" + MEITIANCS + "\n"
				+ "YONGYAOTS:" + YONGYAOTS + "\n"
				+ "DANFUFBZ:" + DANFUFBZ + "\n"
				+ "DANJIA:" + DANJIA + "\n"
				+ "SHULIANG:" + SHULIANG + "\n"
				+ "JINE:" + JINE + "\n"
				+ "KAIDANKSDM:" + KAIDANKSDM + "\n"
				+ "KAIDANKSMC:" + KAIDANKSMC + "\n"
				+ "KAIDANYSDM:" + KAIDANYSDM + "\n"
				+ "KAIDANYSXM:" + KAIDANYSXM + "\n"
            ;
		}
	}
}