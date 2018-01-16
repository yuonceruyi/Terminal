  
using System;
using System.Collections.Generic;
using System.Text;

namespace YuanTu.YuHangArea.CitizenCard
{
	public class Req初始化:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req初始化()
		{
		  serviceName =  "初始化";
		  transCode =  1000;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req初始化 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req初始化
			{
			};
		}
    }
	public class Req签到:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req签到()
		{
		  serviceName =  "签到";
		  transCode =  6215;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req签到 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req签到
			{
			};
		}
    }
	public class Req签退:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req签退()
		{
		  serviceName =  "签退";
		  transCode =  6225;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req签退 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req签退
			{
			};
		}
    }
	public class Req检验卡是否插入:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req检验卡是否插入()
		{
		  serviceName =  "检验卡是否插入";
		  transCode =  1010;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req检验卡是否插入 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req检验卡是否插入
			{
			};
		}
    }
	public class Req读非接触卡号:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req读非接触卡号()
		{
		  serviceName =  "读非接触卡号";
		  transCode =  1001;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req读非接触卡号 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req读非接触卡号
			{
			};
		}
    }
	public class Req读接触卡号:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req读接触卡号()
		{
		  serviceName =  "读接触卡号";
		  transCode =  1003;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req读接触卡号 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req读接触卡号
			{
			};
		}
    }
	public class Req读接触非接卡号:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req读接触非接卡号()
		{
		  serviceName =  "读接触非接卡号";
		  transCode =  1004;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req读接触非接卡号 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req读接触非接卡号
			{
			};
		}
    }
	public class Req读证卡卡号:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req读证卡卡号()
		{
		  serviceName =  "读证卡卡号";
		  transCode =  1005;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req读证卡卡号 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req读证卡卡号
			{
			};
		}
    }
	public class Req可扣查询_JKK:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req可扣查询_JKK()
		{
		  serviceName =  "可扣查询_JKK";
		  transCode =  81025;
		}
	    		public string 卡类型;
		public string 卡号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("卡类型:" + 卡类型 + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    卡类型,
			    卡号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req可扣查询_JKK Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req可扣查询_JKK
			{
				卡类型 = list[1],
				卡号 = list[2],
			};
		}
    }
	public class Req可扣查询_SMK:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req可扣查询_SMK()
		{
		  serviceName =  "可扣查询_SMK";
		  transCode =  81025;
		}
	    		public string 卡类型;
		public string 卡号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("卡类型:" + 卡类型 + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    卡类型,
			    卡号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req可扣查询_SMK Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req可扣查询_SMK
			{
				卡类型 = list[1],
				卡号 = list[2],
			};
		}
    }
	public class Req健康卡信息查询:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req健康卡信息查询()
		{
		  serviceName =  "健康卡信息查询";
		  transCode =  7020;
		}
	    		public string 标志位;
		public string 后面信息长度;
		public string 卡号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("标志位:" + 标志位 + "\n");
			sb.Append("后面信息长度:" + 后面信息长度 + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    标志位,
			    后面信息长度,
			    卡号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req健康卡信息查询 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req健康卡信息查询
			{
				标志位 = list[1],
				后面信息长度 = list[2],
				卡号 = list[3],
			};
		}
    }
	public class Req市民卡账户开通:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req市民卡账户开通()
		{
		  serviceName =  "市民卡账户开通";
		  transCode =  57005;
		}
	    		public string 标志位;
		public string 非接卡号;
		public string 手机号;
		public string 操作员号;
		public string 发卡标志;
		public string 姓名;
		public string 身份证号;
		public string 代理人姓名_20;
		public string 代理人身份证号码_18;
		public string 家庭住址_60;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("标志位:" + 标志位 + "\n");
			sb.Append("非接卡号:" + 非接卡号 + "\n");
			sb.Append("手机号:" + 手机号 + "\n");
			sb.Append("操作员号:" + 操作员号 + "\n");
			sb.Append("发卡标志:" + 发卡标志 + "\n");
			sb.Append("姓名:" + 姓名 + "\n");
			sb.Append("身份证号:" + 身份证号 + "\n");
			sb.Append("代理人姓名_20:" + 代理人姓名_20 + "\n");
			sb.Append("代理人身份证号码_18:" + 代理人身份证号码_18 + "\n");
			sb.Append("家庭住址_60:" + 家庭住址_60 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    标志位,
			    非接卡号,
			    手机号,
			    操作员号,
			    发卡标志,
			    姓名,
			    身份证号,
			    代理人姓名_20,
			    代理人身份证号码_18,
			    家庭住址_60,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req市民卡账户开通 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req市民卡账户开通
			{
				标志位 = list[1],
				非接卡号 = list[2],
				手机号 = list[3],
				操作员号 = list[4],
				发卡标志 = list[5],
				姓名 = list[6],
				身份证号 = list[7],
				代理人姓名_20 = list[8],
				代理人身份证号码_18 = list[9],
				家庭住址_60 = list[10],
			};
		}
    }
	public class Req智慧医疗开通:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req智慧医疗开通()
		{
		  serviceName =  "智慧医疗开通";
		  transCode =  57005;
		}
	    		public string 标志位;
		public string 手机号;
		public string 账户开通;
		public string 网上支付;
		public string 短信提醒;
		public string 智慧医院;
		public string 身份证号;
		public string 交易密码;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("标志位:" + 标志位 + "\n");
			sb.Append("手机号:" + 手机号 + "\n");
			sb.Append("账户开通:" + 账户开通 + "\n");
			sb.Append("网上支付:" + 网上支付 + "\n");
			sb.Append("短信提醒:" + 短信提醒 + "\n");
			sb.Append("智慧医院:" + 智慧医院 + "\n");
			sb.Append("身份证号:" + 身份证号 + "\n");
			sb.Append("交易密码:" + 交易密码 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    标志位,
			    手机号,
			    账户开通,
			    网上支付,
			    短信提醒,
			    智慧医院,
			    身份证号,
			    交易密码,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req智慧医疗开通 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req智慧医疗开通
			{
				标志位 = list[1],
				手机号 = list[2],
				账户开通 = list[3],
				网上支付 = list[4],
				短信提醒 = list[5],
				智慧医院 = list[6],
				身份证号 = list[7],
				交易密码 = list[8],
			};
		}
    }
	public class Req账户医疗开通:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req账户医疗开通()
		{
		  serviceName =  "账户医疗开通";
		  transCode =  57005;
		}
	    		public string 标志位;
		public string 手机号;
		public string 账户开通;
		public string 网上支付;
		public string 短信提醒;
		public string 智慧医院;
		public string 身份证号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("标志位:" + 标志位 + "\n");
			sb.Append("手机号:" + 手机号 + "\n");
			sb.Append("账户开通:" + 账户开通 + "\n");
			sb.Append("网上支付:" + 网上支付 + "\n");
			sb.Append("短信提醒:" + 短信提醒 + "\n");
			sb.Append("智慧医院:" + 智慧医院 + "\n");
			sb.Append("身份证号:" + 身份证号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    标志位,
			    手机号,
			    账户开通,
			    网上支付,
			    短信提醒,
			    智慧医院,
			    身份证号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req账户医疗开通 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req账户医疗开通
			{
				标志位 = list[1],
				手机号 = list[2],
				账户开通 = list[3],
				网上支付 = list[4],
				短信提醒 = list[5],
				智慧医院 = list[6],
				身份证号 = list[7],
			};
		}
    }
	public class Req儿童医疗开通:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req儿童医疗开通()
		{
		  serviceName =  "儿童医疗开通";
		  transCode =  57005;
		}
	    		public string 标志位;
		public string 手机号;
		public string 身份证号;
		public string 外卡号;
		public string 芯片号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("标志位:" + 标志位 + "\n");
			sb.Append("手机号:" + 手机号 + "\n");
			sb.Append("身份证号:" + 身份证号 + "\n");
			sb.Append("外卡号:" + 外卡号 + "\n");
			sb.Append("芯片号:" + 芯片号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    标志位,
			    手机号,
			    身份证号,
			    外卡号,
			    芯片号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req儿童医疗开通 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req儿童医疗开通
			{
				标志位 = list[1],
				手机号 = list[2],
				身份证号 = list[3],
				外卡号 = list[4],
				芯片号 = list[5],
			};
		}
    }
	public class Req市民卡账户充值:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req市民卡账户充值()
		{
		  serviceName =  "市民卡账户充值";
		  transCode =  7010;
		}
	    		public string 渠道;
		public string 银行卡号;
		public string 银行卡流水;
		public string 卡类型;
		public string 卡号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("渠道:" + 渠道 + "\n");
			sb.Append("银行卡号:" + 银行卡号 + "\n");
			sb.Append("银行卡流水:" + 银行卡流水 + "\n");
			sb.Append("卡类型:" + 卡类型 + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    渠道,
			    银行卡号,
			    银行卡流水,
			    卡类型,
			    卡号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req市民卡账户充值 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req市民卡账户充值
			{
				渠道 = list[1],
				银行卡号 = list[2],
				银行卡流水 = list[3],
				卡类型 = list[4],
				卡号 = list[5],
			};
		}
    }
	public class Req消费:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req消费()
		{
		  serviceName =  "消费";
		  transCode =  81105;
		}
	    		public string 社保卡芯片号_32;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("社保卡芯片号_32:" + 社保卡芯片号_32 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    社保卡芯片号_32,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req消费 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req消费
			{
				社保卡芯片号_32 = list[1],
			};
		}
    }
	public class Req密码回显:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req密码回显()
		{
		  serviceName =  "密码回显";
		  transCode =  9904;
		}
	    		public string 卡号;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    卡号,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req密码回显 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req密码回显
			{
				卡号 = list[1],
			};
		}
    }
	public class Req读十进制卡号:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req读十进制卡号()
		{
		  serviceName =  "读十进制卡号";
		  transCode =  9901;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req读十进制卡号 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req读十进制卡号
			{
			};
		}
    }
	public class Req获取密码分次:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req获取密码分次()
		{
		  serviceName =  "获取密码分次";
		  transCode =  9902;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req获取密码分次 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req获取密码分次
			{
			};
		}
    }
	public class Req获取密码分次十六进制:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req获取密码分次十六进制()
		{
		  serviceName =  "获取密码分次十六进制";
		  transCode =  9903;
		}
	    
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
            };
			return string.Join("",list.ToArray());
		}

		public static Req获取密码分次十六进制 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req获取密码分次十六进制
			{
			};
		}
    }
	public class Req密码修改或重置:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req密码修改或重置()
		{
		  serviceName =  "密码修改或重置";
		  transCode =  1325;
		}
	    		public string 修改类型;
		public string 修改对象;
		public string 姓名;
		public string 身份证号码;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("修改类型:" + 修改类型 + "\n");
			sb.Append("修改对象:" + 修改对象 + "\n");
			sb.Append("姓名:" + 姓名 + "\n");
			sb.Append("身份证号码:" + 身份证号码 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    修改类型,
			    修改对象,
			    姓名,
			    身份证号码,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req密码修改或重置 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req密码修改或重置
			{
				修改类型 = list[1],
				修改对象 = list[2],
				姓名 = list[3],
				身份证号码 = list[4],
			};
		}
    }
	public class Req密码修改或重置分次:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req密码修改或重置分次()
		{
		  serviceName =  "密码修改或重置分次";
		  transCode =  81325;
		}
	    		public string 修改类型;
		public string 修改对象;
		public string 姓名;
		public string 身份证号码;
		public string 原密码;
		public string 新密码;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("修改类型:" + 修改类型 + "\n");
			sb.Append("修改对象:" + 修改对象 + "\n");
			sb.Append("姓名:" + 姓名 + "\n");
			sb.Append("身份证号码:" + 身份证号码 + "\n");
			sb.Append("原密码:" + 原密码 + "\n");
			sb.Append("新密码:" + 新密码 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    修改类型,
			    修改对象,
			    姓名,
			    身份证号码,
			    原密码,
			    新密码,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req密码修改或重置分次 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req密码修改或重置分次
			{
				修改类型 = list[1],
				修改对象 = list[2],
				姓名 = list[3],
				身份证号码 = list[4],
				原密码 = list[5],
				新密码 = list[6],
			};
		}
    }
	public class Req密码修改或重置分次十六进制:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req密码修改或重置分次十六进制()
		{
		  serviceName =  "密码修改或重置分次十六进制";
		  transCode =  91325;
		}
	    		public string 修改类型;
		public string 修改对象;
		public string 姓名;
		public string 身份证号码;
		public string 原密码;
		public string 新密码;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("修改类型:" + 修改类型 + "\n");
			sb.Append("修改对象:" + 修改对象 + "\n");
			sb.Append("姓名:" + 姓名 + "\n");
			sb.Append("身份证号码:" + 身份证号码 + "\n");
			sb.Append("原密码:" + 原密码 + "\n");
			sb.Append("新密码:" + 新密码 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    修改类型,
			    修改对象,
			    姓名,
			    身份证号码,
			    原密码,
			    新密码,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req密码修改或重置分次十六进制 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req密码修改或重置分次十六进制
			{
				修改类型 = list[1],
				修改对象 = list[2],
				姓名 = list[3],
				身份证号码 = list[4],
				原密码 = list[5],
				新密码 = list[6],
			};
		}
    }
	public class Req余额查询密码外入版:IReqBase
	{
	    public string serviceName { get; set; }
        public decimal transCode { get; set; }
        public decimal amount { get; set; }
	    public Req余额查询密码外入版()
		{
		  serviceName =  "余额查询密码外入版";
		  transCode =  91025;
		}
	    		public string 密码;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("serviceName:"+ serviceName + "\n");
			sb.Append("transCode:" + transCode + "\n");
			sb.Append("密码:" + 密码 + "\n");
            return sb.ToString();
		}

	    public string Serilize()
		{
			var list = new List<string>
			{
        	    密码,
		    };
			return string.Join("",list.ToArray());
		}

		public static Req余额查询密码外入版 Deserilize(string text)
		{
			var list = text.Split(' ');
			return new Req余额查询密码外入版
			{
				密码 = list[1],
			};
		}
    }
}

