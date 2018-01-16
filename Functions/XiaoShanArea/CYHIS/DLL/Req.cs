using System.Text;

namespace YuanTu.YuHangArea.CYHIS.DLL
{
	public class Req查询建档:IReqBase
	{
		private string myName =  "查询建档";
		public string 卡号;
		public string 结算类型;
		public string 调用接口ID;
		public string 调用类型;
		public string 病人类别;
		public string 结算方式;
		public string 应付金额;
		public string 就诊序号;
		public string 操作工号;
		public string 系统序号;
		public string 收费类别;
		public string 科室代码;
		public string 医生代码;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 挂号类别;
		public string 排班类别;
		public string 取号密码;
		public string 挂号日期;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+ myName + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
			sb.Append("结算类型:" + 结算类型 + "\n");
			sb.Append("调用接口ID:" + 调用接口ID + "\n");
			sb.Append("调用类型:" + 调用类型 + "\n");
			sb.Append("病人类别:" + 病人类别 + "\n");
			sb.Append("结算方式:" + 结算方式 + "\n");
			sb.Append("应付金额:" + 应付金额 + "\n");
			sb.Append("就诊序号:" + 就诊序号 + "\n");
			sb.Append("操作工号:" + 操作工号 + "\n");
			sb.Append("系统序号:" + 系统序号 + "\n");
			sb.Append("收费类别:" + 收费类别 + "\n");
			sb.Append("科室代码:" + 科室代码 + "\n");
			sb.Append("医生代码:" + 医生代码 + "\n");
			sb.Append("诊疗费加收:" + 诊疗费加收 + "\n");
			sb.Append("诊疗费:" + 诊疗费 + "\n");
			sb.Append("挂号类别:" + 挂号类别 + "\n");
			sb.Append("排班类别:" + 排班类别 + "\n");
			sb.Append("取号密码:" + 取号密码 + "\n");
			sb.Append("挂号日期:" + 挂号日期 + "\n");
            return sb.ToString();
		}

		public string GetMyName()
		{
			return myName;
		}

		public string Serilize()
		{
			var list = new string[]
			{
				卡号,
				结算类型,
				调用接口ID,
				调用类型,
				病人类别,
				结算方式,
				应付金额,
				就诊序号,
				操作工号,
				系统序号,
				收费类别,
				科室代码,
				医生代码,
				诊疗费加收,
				诊疗费,
				挂号类别,
				排班类别,
				取号密码,
				挂号日期,
			};			
			return string.Join("#",list);
		}
	}

	public class Req挂号取号:IReqBase
	{
		private string myName =  "挂号取号";
		public string 卡号;
		public string 结算类型;
		public string 调用接口ID;
		public string 调用类型;
		public string 病人类别;
		public string 结算方式;
		public string 应付金额;
		public string 就诊序号;
		public string 操作工号;
		public string 系统序号;
		public string 收费类别;
		public string 科室代码;
		public string 医生代码;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 挂号类别;
		public string 排班类别;
		public string 取号密码;
		public string 挂号日期;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+ myName + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
			sb.Append("结算类型:" + 结算类型 + "\n");
			sb.Append("调用接口ID:" + 调用接口ID + "\n");
			sb.Append("调用类型:" + 调用类型 + "\n");
			sb.Append("病人类别:" + 病人类别 + "\n");
			sb.Append("结算方式:" + 结算方式 + "\n");
			sb.Append("应付金额:" + 应付金额 + "\n");
			sb.Append("就诊序号:" + 就诊序号 + "\n");
			sb.Append("操作工号:" + 操作工号 + "\n");
			sb.Append("系统序号:" + 系统序号 + "\n");
			sb.Append("收费类别:" + 收费类别 + "\n");
			sb.Append("科室代码:" + 科室代码 + "\n");
			sb.Append("医生代码:" + 医生代码 + "\n");
			sb.Append("诊疗费加收:" + 诊疗费加收 + "\n");
			sb.Append("诊疗费:" + 诊疗费 + "\n");
			sb.Append("挂号类别:" + 挂号类别 + "\n");
			sb.Append("排班类别:" + 排班类别 + "\n");
			sb.Append("取号密码:" + 取号密码 + "\n");
			sb.Append("挂号日期:" + 挂号日期 + "\n");
            return sb.ToString();
		}

		public string GetMyName()
		{
			return myName;
		}

		public string Serilize()
		{
			var list = new string[]
			{
				卡号,
				结算类型,
				调用接口ID,
				调用类型,
				病人类别,
				结算方式,
				应付金额,
				就诊序号,
				操作工号,
				系统序号,
				收费类别,
				科室代码,
				医生代码,
				诊疗费加收,
				诊疗费,
				挂号类别,
				排班类别,
				取号密码,
				挂号日期,
			};			
			return string.Join("#",list);
		}
	}

	public class Req缴费预结算:IReqBase
	{
		private string myName =  "缴费预结算";
		public string 卡号;
		public string 结算类型;
		public string 调用接口ID;
		public string 调用类型;
		public string 病人类别;
		public string 结算方式;
		public string 应付金额;
		public string 就诊序号;
		public string 操作工号;
		public string 系统序号;
		public string 收费类别;
		public string 科室代码;
		public string 医生代码;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 挂号类别;
		public string 排班类别;
		public string 取号密码;
		public string 挂号日期;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+ myName + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
			sb.Append("结算类型:" + 结算类型 + "\n");
			sb.Append("调用接口ID:" + 调用接口ID + "\n");
			sb.Append("调用类型:" + 调用类型 + "\n");
			sb.Append("病人类别:" + 病人类别 + "\n");
			sb.Append("结算方式:" + 结算方式 + "\n");
			sb.Append("应付金额:" + 应付金额 + "\n");
			sb.Append("就诊序号:" + 就诊序号 + "\n");
			sb.Append("操作工号:" + 操作工号 + "\n");
			sb.Append("系统序号:" + 系统序号 + "\n");
			sb.Append("收费类别:" + 收费类别 + "\n");
			sb.Append("科室代码:" + 科室代码 + "\n");
			sb.Append("医生代码:" + 医生代码 + "\n");
			sb.Append("诊疗费加收:" + 诊疗费加收 + "\n");
			sb.Append("诊疗费:" + 诊疗费 + "\n");
			sb.Append("挂号类别:" + 挂号类别 + "\n");
			sb.Append("排班类别:" + 排班类别 + "\n");
			sb.Append("取号密码:" + 取号密码 + "\n");
			sb.Append("挂号日期:" + 挂号日期 + "\n");
            return sb.ToString();
		}

		public string GetMyName()
		{
			return myName;
		}

		public string Serilize()
		{
			var list = new string[]
			{
				卡号,
				结算类型,
				调用接口ID,
				调用类型,
				病人类别,
				结算方式,
				应付金额,
				就诊序号,
				操作工号,
				系统序号,
				收费类别,
				科室代码,
				医生代码,
				诊疗费加收,
				诊疗费,
				挂号类别,
				排班类别,
				取号密码,
				挂号日期,
			};			
			return string.Join("#",list);
		}
	}

	public class Req缴费结算:IReqBase
	{
		private string myName =  "缴费结算";
		public string 卡号;
		public string 结算类型;
		public string 调用接口ID;
		public string 调用类型;
		public string 病人类别;
		public string 结算方式;
		public string 应付金额;
		public string 就诊序号;
		public string 操作工号;
		public string 系统序号;
		public string 收费类别;
		public string 科室代码;
		public string 医生代码;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 挂号类别;
		public string 排班类别;
		public string 取号密码;
		public string 挂号日期;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+ myName + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
			sb.Append("结算类型:" + 结算类型 + "\n");
			sb.Append("调用接口ID:" + 调用接口ID + "\n");
			sb.Append("调用类型:" + 调用类型 + "\n");
			sb.Append("病人类别:" + 病人类别 + "\n");
			sb.Append("结算方式:" + 结算方式 + "\n");
			sb.Append("应付金额:" + 应付金额 + "\n");
			sb.Append("就诊序号:" + 就诊序号 + "\n");
			sb.Append("操作工号:" + 操作工号 + "\n");
			sb.Append("系统序号:" + 系统序号 + "\n");
			sb.Append("收费类别:" + 收费类别 + "\n");
			sb.Append("科室代码:" + 科室代码 + "\n");
			sb.Append("医生代码:" + 医生代码 + "\n");
			sb.Append("诊疗费加收:" + 诊疗费加收 + "\n");
			sb.Append("诊疗费:" + 诊疗费 + "\n");
			sb.Append("挂号类别:" + 挂号类别 + "\n");
			sb.Append("排班类别:" + 排班类别 + "\n");
			sb.Append("取号密码:" + 取号密码 + "\n");
			sb.Append("挂号日期:" + 挂号日期 + "\n");
            return sb.ToString();
		}

		public string GetMyName()
		{
			return myName;
		}

		public string Serilize()
		{
			var list = new string[]
			{
				卡号,
				结算类型,
				调用接口ID,
				调用类型,
				病人类别,
				结算方式,
				应付金额,
				就诊序号,
				操作工号,
				系统序号,
				收费类别,
				科室代码,
				医生代码,
				诊疗费加收,
				诊疗费,
				挂号类别,
				排班类别,
				取号密码,
				挂号日期,
			};			
			return string.Join("#",list);
		}
	}

	public class Req签退:IReqBase
	{
		private string myName =  "签退";
		public string 卡号;
		public string 结算类型;
		public string 调用接口ID;
		public string 调用类型;
		public string 病人类别;
		public string 结算方式;
		public string 应付金额;
		public string 就诊序号;
		public string 操作工号;
		public string 系统序号;
		public string 收费类别;
		public string 科室代码;
		public string 医生代码;
		public string 诊疗费加收;
		public string 诊疗费;
		public string 挂号类别;
		public string 排班类别;
		public string 取号密码;
		public string 挂号日期;

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("NAME:"+ myName + "\n");
			sb.Append("卡号:" + 卡号 + "\n");
			sb.Append("结算类型:" + 结算类型 + "\n");
			sb.Append("调用接口ID:" + 调用接口ID + "\n");
			sb.Append("调用类型:" + 调用类型 + "\n");
			sb.Append("病人类别:" + 病人类别 + "\n");
			sb.Append("结算方式:" + 结算方式 + "\n");
			sb.Append("应付金额:" + 应付金额 + "\n");
			sb.Append("就诊序号:" + 就诊序号 + "\n");
			sb.Append("操作工号:" + 操作工号 + "\n");
			sb.Append("系统序号:" + 系统序号 + "\n");
			sb.Append("收费类别:" + 收费类别 + "\n");
			sb.Append("科室代码:" + 科室代码 + "\n");
			sb.Append("医生代码:" + 医生代码 + "\n");
			sb.Append("诊疗费加收:" + 诊疗费加收 + "\n");
			sb.Append("诊疗费:" + 诊疗费 + "\n");
			sb.Append("挂号类别:" + 挂号类别 + "\n");
			sb.Append("排班类别:" + 排班类别 + "\n");
			sb.Append("取号密码:" + 取号密码 + "\n");
			sb.Append("挂号日期:" + 挂号日期 + "\n");
            return sb.ToString();
		}

		public string GetMyName()
		{
			return myName;
		}

		public string Serilize()
		{
			var list = new string[]
			{
				卡号,
				结算类型,
				调用接口ID,
				调用类型,
				病人类别,
				结算方式,
				应付金额,
				就诊序号,
				操作工号,
				系统序号,
				收费类别,
				科室代码,
				医生代码,
				诊疗费加收,
				诊疗费,
				挂号类别,
				排班类别,
				取号密码,
				挂号日期,
			};			
			return string.Join("#",list);
		}
	}

}
