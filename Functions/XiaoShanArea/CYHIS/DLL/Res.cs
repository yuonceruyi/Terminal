using System.Xml.Serialization;

namespace YuanTu.YuHangArea.CYHIS.DLL
{ 
  
	[XmlType("查询建档_OUT")]
	public class 查询建档_OUT:IResBase
	{

//		public string TradeCode = "HIS1";
		public OUTMSG OUTMSG;
		public string 结算结果;
		public string 病人姓名;
		public string 就诊卡号;
		public string 病人性别;
		public string 身份证号;
		public string 出生日期;
		public string 病人类别;
		public string 当年账户余额;
		public string 历年账户余额;
		public string 市民卡余额;


     
		  public string Serilize()
		{
			return Utility.Serilize(typeof(查询建档_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" 查询建档"+ "\n";
				text+= "结算结果:" + 结算结果 + "\n";
				text+= "病人姓名:" + 病人姓名 + "\n";
				text+= "就诊卡号:" + 就诊卡号 + "\n";
				text+= "病人性别:" + 病人性别 + "\n";
				text+= "身份证号:" + 身份证号 + "\n";
				text+= "出生日期:" + 出生日期 + "\n";
				text+= "病人类别:" + 病人类别 + "\n";
				text+= "当年账户余额:" + 当年账户余额 + "\n";
				text+= "历年账户余额:" + 历年账户余额 + "\n";
				text+= "市民卡余额:" + 市民卡余额 + "\n";
            return text;
		}

		public static 查询建档_OUT Deserilize(string text)
		{
			var list = text.Split('|');
			return new 查询建档_OUT
			{
				结算结果 = list[0],
				病人姓名 = list[1],
				就诊卡号 = list[2],
				病人性别 = list[3],
				身份证号 = list[4],
				出生日期 = list[5],
				病人类别 = list[6],
				当年账户余额 = list[7],
				历年账户余额 = list[8],
				市民卡余额 = list[9],
			};
	     }
	}
   
  
	[XmlType("挂号取号_OUT")]
	public class 挂号取号_OUT:IResBase
	{

//		public string TradeCode = "HIS1";
		public OUTMSG OUTMSG;
		public string 结算结果;
		public string 病人姓名;
		public string 就诊卡号;
		public string 挂号日期;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 医保支付;
		public string 市民卡账户支付;
		public string 惠民减免金额;
		public string 记账金额;
		public string 科室名称;
		public string 科室位置;
		public string 医生名称;
		public string 挂号序号;
		public string 就诊号码;
		public string 候诊时间;


     
		  public string Serilize()
		{
			return Utility.Serilize(typeof(挂号取号_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" 挂号取号"+ "\n";
				text+= "结算结果:" + 结算结果 + "\n";
				text+= "病人姓名:" + 病人姓名 + "\n";
				text+= "就诊卡号:" + 就诊卡号 + "\n";
				text+= "挂号日期:" + 挂号日期 + "\n";
				text+= "诊疗费加收:" + 诊疗费加收 + "\n";
				text+= "诊疗费:" + 诊疗费 + "\n";
				text+= "医保支付:" + 医保支付 + "\n";
				text+= "市民卡账户支付:" + 市民卡账户支付 + "\n";
				text+= "惠民减免金额:" + 惠民减免金额 + "\n";
				text+= "记账金额:" + 记账金额 + "\n";
				text+= "科室名称:" + 科室名称 + "\n";
				text+= "科室位置:" + 科室位置 + "\n";
				text+= "医生名称:" + 医生名称 + "\n";
				text+= "挂号序号:" + 挂号序号 + "\n";
				text+= "就诊号码:" + 就诊号码 + "\n";
				text+= "候诊时间:" + 候诊时间 + "\n";
            return text;
		}

		public static 挂号取号_OUT Deserilize(string text)
		{
			var list = text.Split('|');
			return new 挂号取号_OUT
			{
				结算结果 = list[0],
				病人姓名 = list[1],
				就诊卡号 = list[2],
				挂号日期 = list[3],
				诊疗费加收 = list[4],
				诊疗费 = list[5],
				医保支付 = list[6],
				市民卡账户支付 = list[7],
				惠民减免金额 = list[8],
				记账金额 = list[9],
				科室名称 = list[10],
				科室位置 = list[11],
				医生名称 = list[12],
				挂号序号 = list[13],
				就诊号码 = list[14],
				候诊时间 = list[15],
			};
	     }
	}
   
  
	[XmlType("缴费预结算_OUT")]
	public class 缴费预结算_OUT:IResBase
	{

//		public string TradeCode = "HIS1";
		public OUTMSG OUTMSG;
		public string 结算结果;
		public string 病人姓名;
		public string 单据总金额;
		public string 医保报销金额;
		public string 应付金额;


     
		  public string Serilize()
		{
			return Utility.Serilize(typeof(缴费预结算_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" 缴费预结算"+ "\n";
				text+= "结算结果:" + 结算结果 + "\n";
				text+= "病人姓名:" + 病人姓名 + "\n";
				text+= "单据总金额:" + 单据总金额 + "\n";
				text+= "医保报销金额:" + 医保报销金额 + "\n";
				text+= "应付金额:" + 应付金额 + "\n";
            return text;
		}

		public static 缴费预结算_OUT Deserilize(string text)
		{
			var list = text.Split('|');
			return new 缴费预结算_OUT
			{
				结算结果 = list[0],
				病人姓名 = list[1],
				单据总金额 = list[2],
				医保报销金额 = list[3],
				应付金额 = list[4],
			};
	     }
	}
   
  
	[XmlType("缴费结算_OUT")]
	public class 缴费结算_OUT:IResBase
	{

//		public string TradeCode = "HIS1";
		public OUTMSG OUTMSG;
		public string 结算结果;
		public string 病人姓名;
		public string 成功提示信息;
		public string 电脑号;
		public string 单据总金额;
		public string 医保报销金额;
		public string 应付金额;
		public string 医保本年账户余额;
		public string 医保历年账户余额;
		public string 智慧医疗账户余额;
		public string 结算日期;
		public string 打印发票;
		public string 取药窗口;


     
		  public string Serilize()
		{
			return Utility.Serilize(typeof(缴费结算_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" 缴费结算"+ "\n";
				text+= "结算结果:" + 结算结果 + "\n";
				text+= "病人姓名:" + 病人姓名 + "\n";
				text+= "成功提示信息:" + 成功提示信息 + "\n";
				text+= "电脑号:" + 电脑号 + "\n";
				text+= "单据总金额:" + 单据总金额 + "\n";
				text+= "医保报销金额:" + 医保报销金额 + "\n";
				text+= "应付金额:" + 应付金额 + "\n";
				text+= "医保本年账户余额:" + 医保本年账户余额 + "\n";
				text+= "医保历年账户余额:" + 医保历年账户余额 + "\n";
				text+= "智慧医疗账户余额:" + 智慧医疗账户余额 + "\n";
				text+= "结算日期:" + 结算日期 + "\n";
				text+= "打印发票:" + 打印发票 + "\n";
				text+= "取药窗口:" + 取药窗口 + "\n";
            return text;
		}

		public static 缴费结算_OUT Deserilize(string text)
		{
			var list = text.Split('|');
			return new 缴费结算_OUT
			{
				结算结果 = list[0],
				病人姓名 = list[1],
				成功提示信息 = list[2],
				电脑号 = list[3],
				单据总金额 = list[4],
				医保报销金额 = list[5],
				应付金额 = list[6],
				医保本年账户余额 = list[7],
				医保历年账户余额 = list[8],
				智慧医疗账户余额 = list[9],
				结算日期 = list[10],
				打印发票 = list[11],
				取药窗口 = list[12],
			};
	     }
	}
   
  
	[XmlType("签退_OUT")]
	public class 签退_OUT:IResBase
	{

//		public string TradeCode = "HIS1";
		public OUTMSG OUTMSG;
		public string 签退结果;
		public string 错误类型;
		public string 错误信息;


     
		  public string Serilize()
		{
			return Utility.Serilize(typeof(签退_OUT));
		 }
		public override string ToString()
		{
			string text="";
			text+= "NAME:" +" 签退"+ "\n";
				text+= "签退结果:" + 签退结果 + "\n";
				text+= "错误类型:" + 错误类型 + "\n";
				text+= "错误信息:" + 错误信息 + "\n";
            return text;
		}

		public static 签退_OUT Deserilize(string text)
		{
			var list = text.Split('|');
			return new 签退_OUT
			{
				签退结果 = list[0],
				错误类型 = list[1],
				错误信息 = list[2],
			};
	     }
	}
   
}


