using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.YuHangArea.CitizenCard
{
  
public class Res初始化:IResBase
{
		private string myName =  "初始化";

		public string 交易码 = "1000";
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res初始化 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res初始化
			{        
			};
		   }
		}
  
public class Res检验卡是否插入:IResBase
{
		private string myName =  "检验卡是否插入";

		public string 交易码 = "1010";
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res检验卡是否插入 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res检验卡是否插入
			{        
			};
		   }
		}
  
public class Res读接触卡号:IResBase
{
		private string myName =  "读接触卡号";

		public string 交易码 = "1003";
		public string 卡号识别码;
		public string 卡类别;
		public string 卡号;
		public string 身份证号;
		public string 姓名;
		public string 性别;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("卡号识别码:" + 卡号识别码 + "\n");
		    sb.Append("卡类别:" + 卡类别 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
		    sb.Append("身份证号:" + 身份证号 + "\n");
		    sb.Append("姓名:" + 姓名 + "\n");
		    sb.Append("性别:" + 性别 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       卡号识别码,
			       卡类别,
			       卡号,
			       身份证号,
			       姓名,
			       性别,
          };
			return String.Join("|",list.ToArray());
		}
				public static Res读接触卡号 Deserilize(string text)
		       {
			     var list =text.Split('|');
			return new Res读接触卡号
			{        
				卡号识别码 = list[0],
				卡类别 = list[1],
				卡号 = list[2],
				身份证号 = list[3],
				姓名 = list[4],
				性别 = list[5],
			};
		   }
		}
  
public class Res读接触非接卡号:IResBase
{
		private string myName =  "读接触非接卡号";

		public string 交易码 = "1004";
		public string 卡识别码;
		public string 卡类型;
		public string 卡号;
		public string 身份证号;
		public string 姓名;
		public string 性别;
		public string 保留1;
		public string 保留2;
		public string 保留3;
		public string 保留4;
		public string PSAM卡终端机编号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("卡识别码:" + 卡识别码 + "\n");
		    sb.Append("卡类型:" + 卡类型 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
		    sb.Append("身份证号:" + 身份证号 + "\n");
		    sb.Append("姓名:" + 姓名 + "\n");
		    sb.Append("性别:" + 性别 + "\n");
		    sb.Append("保留1:" + 保留1 + "\n");
		    sb.Append("保留2:" + 保留2 + "\n");
		    sb.Append("保留3:" + 保留3 + "\n");
		    sb.Append("保留4:" + 保留4 + "\n");
		    sb.Append("PSAM卡终端机编号:" + PSAM卡终端机编号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       卡识别码,
			       卡类型,
			       卡号,
			       身份证号,
			       姓名,
			       性别,
			       保留1,
			       保留2,
			       保留3,
			       保留4,
			       PSAM卡终端机编号,
          };
			return String.Join("|",list.ToArray());
		}
				public static Res读接触非接卡号 Deserilize(string text)
		       {
			     var list =text.Split('|');
			return new Res读接触非接卡号
			{        
				卡识别码 = list[0],
				卡类型 = list[1],
				卡号 = list[2],
				身份证号 = list[3],
				姓名 = list[4],
				性别 = list[5],
				保留1 = list[6],
				保留2 = list[7],
				保留3 = list[8],
				保留4 = list[9],
				PSAM卡终端机编号 = list[10],
			};
		   }
		}
  
public class Res读证卡卡号:IResBase
{
		private string myName =  "读证卡卡号";

		public string 交易码 = "1005";
		public string 卡识别码;
		public string 卡类型;
		public string 卡号;
		public string 身份证号;
		public string 姓名;
		public string 性别;
		public string 保留1;
		public string 保留2;
		public string 保留3;
		public string 保留4;
		public string PSAM卡终端机编号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("卡识别码:" + 卡识别码 + "\n");
		    sb.Append("卡类型:" + 卡类型 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
		    sb.Append("身份证号:" + 身份证号 + "\n");
		    sb.Append("姓名:" + 姓名 + "\n");
		    sb.Append("性别:" + 性别 + "\n");
		    sb.Append("保留1:" + 保留1 + "\n");
		    sb.Append("保留2:" + 保留2 + "\n");
		    sb.Append("保留3:" + 保留3 + "\n");
		    sb.Append("保留4:" + 保留4 + "\n");
		    sb.Append("PSAM卡终端机编号:" + PSAM卡终端机编号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       卡识别码,
			       卡类型,
			       卡号,
			       身份证号,
			       姓名,
			       性别,
			       保留1,
			       保留2,
			       保留3,
			       保留4,
			       PSAM卡终端机编号,
          };
			return String.Join("|",list.ToArray());
		}
				public static Res读证卡卡号 Deserilize(string text)
		       {
			     var list =text.Split('|');
			return new Res读证卡卡号
			{        
				卡识别码 = list[0],
				卡类型 = list[1],
				卡号 = list[2],
				身份证号 = list[3],
				姓名 = list[4],
				性别 = list[5],
				保留1 = list[6],
				保留2 = list[7],
				保留3 = list[8],
				保留4 = list[9],
				PSAM卡终端机编号 = list[10],
			};
		   }
		}
  
public class Res获取密码分次:IResBase
{
		private string myName =  "获取密码分次";

		public string 交易码 = "9902";
		public string 密码;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("密码:" + 密码 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       密码,
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res获取密码分次 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res获取密码分次
			{        
				密码 = list[0],
			};
		   }
		}
  
public class Res获取密码分次十六进制:IResBase
{
		private string myName =  "获取密码分次十六进制";

		public string 交易码 = "9903";
		public string 密码;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("密码:" + 密码 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       密码,
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res获取密码分次十六进制 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res获取密码分次十六进制
			{        
				密码 = list[0],
			};
		   }
		}
  
public class Res读十进制卡号:IResBase
{
		private string myName =  "读十进制卡号";

		public string 交易码 = "9901";
		public string 卡号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       卡号,
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res读十进制卡号 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res读十进制卡号
			{        
				卡号 = list[0],
			};
		   }
		}
  
public class Res读非接触卡号:IResBase
{
		private string myName =  "读非接触卡号";

		public string 交易码 = "1001";
		public string 卡号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
		return myName;
  }

   	  public string Serilize()
		{
			var list = new List<string>
			{
			       卡号,
          };
			return String.Join(" ",list.ToArray());
		}
				public static Res读非接触卡号 Deserilize(string text)
		       {
			     var list =text.Split(' ');
			return new Res读非接触卡号
			{        
				卡号 = list[0],
			};
		   }
		}

  
 public class Res签到:IResBase
{
		private string myName =  "签到";

		public string 交易码 = "6215";
		public string 应答码;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
          };
			return String.Join(" ",list.ToArray());
		}
	public static Res签到 Deserilize(string text)
	{
			     if(!text.Contains(" "))
				 {
				   return new Res签到
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split(' ');
			return new Res签到
			{       
			  应答码 = list[0], 
			};
			}
	 }
}
  
 public class Res签退:IResBase
{
		private string myName =  "签退";

		public string 交易码 = "6225";
		public string 应答码;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
          };
			return String.Join(" ",list.ToArray());
		}
	public static Res签退 Deserilize(string text)
	{
			     if(!text.Contains(" "))
				 {
				   return new Res签退
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split(' ');
			return new Res签退
			{       
			  应答码 = list[0], 
			};
			}
	 }
}
  
 public class Res可扣查询_JKK:IResBase
{
		private string myName =  "可扣查询_JKK";

		public string 交易码 = "81025";
		public string 应答码;
		public string 返回金额;
		public string 卡面号;
		public string 智慧医疗开通;
		public string 姓名;
		public string 身份证;
		public string 手机号码;
		public string 地址;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("返回金额:" + 返回金额 + "\n");
		    sb.Append("卡面号:" + 卡面号 + "\n");
		    sb.Append("智慧医疗开通:" + 智慧医疗开通 + "\n");
		    sb.Append("姓名:" + 姓名 + "\n");
		    sb.Append("身份证:" + 身份证 + "\n");
		    sb.Append("手机号码:" + 手机号码 + "\n");
		    sb.Append("地址:" + 地址 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       返回金额,
			       卡面号,
			       智慧医疗开通,
			       姓名,
			       身份证,
			       手机号码,
			       地址,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res可扣查询_JKK Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res可扣查询_JKK
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res可扣查询_JKK
			{       
			  应答码 = list[0], 
				返回金额 = list[1],
				卡面号 = list[2],
				智慧医疗开通 = list[3],
				姓名 = list[4],
				身份证 = list[5],
				手机号码 = list[6],
				地址 = list[7],
			};
			}
	 }
}
  
 public class Res可扣查询_SMK:IResBase
{
		private string myName =  "可扣查询_SMK";

		public string 交易码 = "81025";
		public string 应答码;
		public string 返回金额;
		public string 卡面号;
		public string 智慧医疗开通;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("返回金额:" + 返回金额 + "\n");
		    sb.Append("卡面号:" + 卡面号 + "\n");
		    sb.Append("智慧医疗开通:" + 智慧医疗开通 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       返回金额,
			       卡面号,
			       智慧医疗开通,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res可扣查询_SMK Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res可扣查询_SMK
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res可扣查询_SMK
			{       
			  应答码 = list[0], 
				返回金额 = list[1],
				卡面号 = list[2],
				智慧医疗开通 = list[3],
			};
			}
	 }
}
  
 public class Res市民卡账户开通:IResBase
{
		private string myName =  "市民卡账户开通";

		public string 交易码 = "57005";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 账户类型2;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("账户类型2:" + 账户类型2 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       账户类型2,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res市民卡账户开通 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res市民卡账户开通
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res市民卡账户开通
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				账户类型2 = list[6],
				交易日期 = list[7],
				交易时间 = list[8],
				交易参考号 = list[9],
				批次号 = list[10],
			};
			}
	 }
}
  
 public class Res账户医疗开通:IResBase
{
		private string myName =  "账户医疗开通";

		public string 交易码 = "57005";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 账户类型2;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("账户类型2:" + 账户类型2 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       账户类型2,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res账户医疗开通 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res账户医疗开通
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res账户医疗开通
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				账户类型2 = list[6],
				交易日期 = list[7],
				交易时间 = list[8],
				交易参考号 = list[9],
				批次号 = list[10],
			};
			}
	 }
}
  
 public class Res智慧医疗开通:IResBase
{
		private string myName =  "智慧医疗开通";

		public string 交易码 = "57005";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 账户类型2;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("账户类型2:" + 账户类型2 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       账户类型2,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res智慧医疗开通 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res智慧医疗开通
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res智慧医疗开通
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				账户类型2 = list[6],
				交易日期 = list[7],
				交易时间 = list[8],
				交易参考号 = list[9],
				批次号 = list[10],
			};
			}
	 }
}
  
 public class Res儿童医疗开通:IResBase
{
		private string myName =  "儿童医疗开通";

		public string 交易码 = "57005";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 账户类型2;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("账户类型2:" + 账户类型2 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       账户类型2,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res儿童医疗开通 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res儿童医疗开通
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res儿童医疗开通
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				账户类型2 = list[6],
				交易日期 = list[7],
				交易时间 = list[8],
				交易参考号 = list[9],
				批次号 = list[10],
			};
			}
	 }
}
  
 public class Res市民卡账户充值:IResBase
{
		private string myName =  "市民卡账户充值";

		public string 交易码 = "7010";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 物理卡号;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
		public string 凭证号;
		public string 金额;
		public string 卡面号凹码;
		public string 卡号;
		public string 账户余额;
		public string 小票余额打印限额;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("物理卡号:" + 物理卡号 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
		    sb.Append("凭证号:" + 凭证号 + "\n");
		    sb.Append("金额:" + 金额 + "\n");
		    sb.Append("卡面号凹码:" + 卡面号凹码 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
		    sb.Append("账户余额:" + 账户余额 + "\n");
		    sb.Append("小票余额打印限额:" + 小票余额打印限额 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       物理卡号,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
			       凭证号,
			       金额,
			       卡面号凹码,
			       卡号,
			       账户余额,
			       小票余额打印限额,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res市民卡账户充值 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res市民卡账户充值
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res市民卡账户充值
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				物理卡号 = list[5],
				交易日期 = list[6],
				交易时间 = list[7],
				交易参考号 = list[8],
				批次号 = list[9],
				凭证号 = list[10],
				金额 = list[11],
				卡面号凹码 = list[12],
				卡号 = list[13],
				账户余额 = list[14],
				小票余额打印限额 = list[15],
			};
			}
	 }
}
  
 public class Res消费:IResBase
{
		private string myName =  "消费";

		public string 交易码 = "81105";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 物理卡号;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
		public string 凭证号;
		public string 金额;
		public string 卡面凹码;
		public string 卡号;
		public string 账户余额;
		public string 小票余额打印限额;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("物理卡号:" + 物理卡号 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
		    sb.Append("凭证号:" + 凭证号 + "\n");
		    sb.Append("金额:" + 金额 + "\n");
		    sb.Append("卡面凹码:" + 卡面凹码 + "\n");
		    sb.Append("卡号:" + 卡号 + "\n");
		    sb.Append("账户余额:" + 账户余额 + "\n");
		    sb.Append("小票余额打印限额:" + 小票余额打印限额 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       物理卡号,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
			       凭证号,
			       金额,
			       卡面凹码,
			       卡号,
			       账户余额,
			       小票余额打印限额,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res消费 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res消费
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res消费
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				物理卡号 = list[5],
				交易日期 = list[6],
				交易时间 = list[7],
				交易参考号 = list[8],
				批次号 = list[9],
				凭证号 = list[10],
				金额 = list[11],
				卡面凹码 = list[12],
				卡号 = list[13],
				账户余额 = list[14],
				小票余额打印限额 = list[15],
			};
			}
	 }
}
  
 public class Res密码修改或重置:IResBase
{
		private string myName =  "密码修改或重置";

		public string 交易码 = "1325";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
		public string POS机流水号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
		    sb.Append("POS机流水号:" + POS机流水号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
			       POS机流水号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res密码修改或重置 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res密码修改或重置
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res密码修改或重置
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				交易日期 = list[6],
				交易时间 = list[7],
				交易参考号 = list[8],
				批次号 = list[9],
				POS机流水号 = list[10],
			};
			}
	 }
}
  
 public class Res密码修改或重置分次:IResBase
{
		private string myName =  "密码修改或重置分次";

		public string 交易码 = "81325";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
		public string POS机流水号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
		    sb.Append("POS机流水号:" + POS机流水号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
			       POS机流水号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res密码修改或重置分次 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res密码修改或重置分次
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res密码修改或重置分次
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				交易日期 = list[6],
				交易时间 = list[7],
				交易参考号 = list[8],
				批次号 = list[9],
				POS机流水号 = list[10],
			};
			}
	 }
}
  
 public class Res密码修改或重置分次十六进制:IResBase
{
		private string myName =  "密码修改或重置分次十六进制";

		public string 交易码 = "91325";
		public string 应答码;
		public string 商户编号;
		public string 终端编号;
		public string 账户类型;
		public string 交易类型;
		public string 市民卡卡面号;
		public string 交易日期;
		public string 交易时间;
		public string 交易参考号;
		public string 批次号;
		public string POS机流水号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("商户编号:" + 商户编号 + "\n");
		    sb.Append("终端编号:" + 终端编号 + "\n");
		    sb.Append("账户类型:" + 账户类型 + "\n");
		    sb.Append("交易类型:" + 交易类型 + "\n");
		    sb.Append("市民卡卡面号:" + 市民卡卡面号 + "\n");
		    sb.Append("交易日期:" + 交易日期 + "\n");
		    sb.Append("交易时间:" + 交易时间 + "\n");
		    sb.Append("交易参考号:" + 交易参考号 + "\n");
		    sb.Append("批次号:" + 批次号 + "\n");
		    sb.Append("POS机流水号:" + POS机流水号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       商户编号,
			       终端编号,
			       账户类型,
			       交易类型,
			       市民卡卡面号,
			       交易日期,
			       交易时间,
			       交易参考号,
			       批次号,
			       POS机流水号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res密码修改或重置分次十六进制 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res密码修改或重置分次十六进制
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res密码修改或重置分次十六进制
			{       
			  应答码 = list[0], 
				商户编号 = list[1],
				终端编号 = list[2],
				账户类型 = list[3],
				交易类型 = list[4],
				市民卡卡面号 = list[5],
				交易日期 = list[6],
				交易时间 = list[7],
				交易参考号 = list[8],
				批次号 = list[9],
				POS机流水号 = list[10],
			};
			}
	 }
}
  
 public class Res余额查询密码外入版:IResBase
{
		private string myName =  "余额查询密码外入版";

		public string 交易码 = "91025";
		public string 应答码;
		public string 金额;
		public string 卡面号;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("金额:" + 金额 + "\n");
		    sb.Append("卡面号:" + 卡面号 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       金额,
			       卡面号,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res余额查询密码外入版 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res余额查询密码外入版
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res余额查询密码外入版
			{       
			  应答码 = list[0], 
				金额 = list[1],
				卡面号 = list[2],
			};
			}
	 }
}
  
 public class Res密码回显:IResBase
{
		private string myName =  "密码回显";

		public string 交易码 = "9904";
		public string 应答码;
		public string 首位标志;
		public string 密码个数;
		public string 密码值;
	public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+myName+"\n");
			sb.Append("交易码:" + 交易码 + "\n");
			sb.Append("应答码:"+应答码+"\n");
		    sb.Append("首位标志:" + 首位标志 + "\n");
		    sb.Append("密码个数:" + 密码个数 + "\n");
		    sb.Append("密码值:" + 密码值 + "\n");
  return sb.ToString();
   }

   public string GetMyName()
  {
			return myName;
  }

   	  public string Serilize()
	 {
			var list = new List<string>
			{
			       首位标志,
			       密码个数,
			       密码值,
          };
			return String.Join("#",list.ToArray());
		}
	public static Res密码回显 Deserilize(string text)
	{
			     if(!text.Contains("#"))
				 {
				   return new Res密码回显
				   {
				      应答码 = text,
				   };
				 }
		   else
			{
               var list =text.Split('#');
			return new Res密码回显
			{       
			  应答码 = list[0], 
				首位标志 = list[1],
				密码个数 = list[2],
				密码值 = list[3],
			};
			}
	 }
}
}


