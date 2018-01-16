 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.TongXiangHospitals.HealthInsurance.Response
{

    public class Res获取参保人信息结果:ResBase
    {
        public string 个人基本信息串 { get; set; }
        public string 身份验证 { get; set; }
        public string 工伤身份验证结果 { get; set; }
        public string 生育身份验证结果 { get; set; }
        public string 预留位 { get; set; }
        public override string ToString()
	    {
		  var baseSb=base.ToString();
	      var sb=new StringBuilder();
		  sb.Append($"个人基本信息串:{个人基本信息串}\n");
		  sb.Append($"身份验证:{身份验证}\n");
		  sb.Append($"工伤身份验证结果:{工伤身份验证结果}\n");
		  sb.Append($"生育身份验证结果:{生育身份验证结果}\n");
		  sb.Append($"预留位:{预留位}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }

    public class Res交易确认结果:ResBase
    {
        public string 交易流水号 { get; set; }
        public override string ToString()
	    {
		  var baseSb=base.ToString();
	      var sb=new StringBuilder();
		  sb.Append($"交易流水号:{交易流水号}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }

    public class Res交易结果查询结果:ResBase
    {
        public string 用户交易是否成功 { get; set; }
        public string 交易时间 { get; set; }
        public string 交易结算流水号 { get; set; }
        public string 交易处于阶段 { get; set; }
        public string 该用户交易出口参数 { get; set; }
        public string 基金分段信息结构体 { get; set; }
        public string 费用汇总信息结构体 { get; set; }
        public override string ToString()
	    {
		  var baseSb=base.ToString();
	      var sb=new StringBuilder();
		  sb.Append($"用户交易是否成功:{用户交易是否成功}\n");
		  sb.Append($"交易时间:{交易时间}\n");
		  sb.Append($"交易结算流水号:{交易结算流水号}\n");
		  sb.Append($"交易处于阶段:{交易处于阶段}\n");
		  sb.Append($"该用户交易出口参数:{该用户交易出口参数}\n");
		  sb.Append($"基金分段信息结构体:{基金分段信息结构体}\n");
		  sb.Append($"费用汇总信息结构体:{费用汇总信息结构体}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }


}