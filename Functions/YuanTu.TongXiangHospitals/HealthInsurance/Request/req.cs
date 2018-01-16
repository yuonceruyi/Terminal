 using System;
using System.Collections.Generic;
using System.Text;
namespace YuanTu.TongXiangHospitals.HealthInsurance.Request
{

    public class Req获取参保人信息:ReqBase
    {
        public string 读卡方式 { get; set; }
        public override string ToString()
	    {
	      var baseSb = base.ToString();
		  var sb=new StringBuilder();
		  sb.Append($"读卡方式:{读卡方式}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }

    public class Req交易确认:ReqBase
    {
        public string 交易类型 { get; set; }
        public string 医保交易流水号 { get; set; }
        public string HIS事务结果 { get; set; }
        public string 附加信息 { get; set; }
        public override string ToString()
	    {
	      var baseSb = base.ToString();
		  var sb=new StringBuilder();
		  sb.Append($"交易类型:{交易类型}\n");
		  sb.Append($"医保交易流水号:{医保交易流水号}\n");
		  sb.Append($"HIS事务结果:{HIS事务结果}\n");
		  sb.Append($"附加信息:{附加信息}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }

    public class Req交易结果查询:ReqBase
    {
        public string 交易类型 { get; set; }
        public string 医保交易流水号 { get; set; }
        public override string ToString()
	    {
	      var baseSb = base.ToString();
		  var sb=new StringBuilder();
		  sb.Append($"交易类型:{交易类型}\n");
		  sb.Append($"医保交易流水号:{医保交易流水号}\n");
          return $"{baseSb}{sb.ToString()}";
        }

    }


}