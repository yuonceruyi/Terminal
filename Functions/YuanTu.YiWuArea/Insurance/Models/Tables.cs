
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.YiWuArea.Insurance.Models.Base;


namespace YuanTu.YiWuArea.Insurance.Models{

	public class 预算单据列表:ItemBase{
		public override int ItemCount=>13;
		public string 单据号{get;set;}
		public string 门诊号{get;set;}
		public string 处方号码{get;set;}
		public string 就诊日期{get;set;}
		public string 收费类型{get;set;}
		public string 科室代码{get;set;}
		public string 科室名称{get;set;}
		public string 开方医师身份证号{get;set;}
		public string 疾病编号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 非医保项目总额{get;set;}
		public string 收费明细条数{get;set;}
		public override string FormatData(){
			return $"{单据号}%%{门诊号}%%{处方号码}%%{就诊日期}%%{收费类型}%%{科室代码}%%{科室名称}%%{开方医师身份证号}%%{疾病编号}%%{疾病名称}%%{疾病描述}%%{非医保项目总额}%%{收费明细条数}";
		}
		public override void Descirbe(string[] arr)
	    {
			单据号=arr[0];
			门诊号=arr[1];
			处方号码=arr[2];
			就诊日期=arr[3];
			收费类型=arr[4];
			科室代码=arr[5];
			科室名称=arr[6];
			开方医师身份证号=arr[7];
			疾病编号=arr[8];
			疾病名称=arr[9];
			疾病描述=arr[10];
			非医保项目总额=arr[11];
			收费明细条数=arr[12];
	    }
	}
	public class 预算收费项目列表:ItemBase{
		public override int ItemCount=>20;
		public string 单据号码{get;set;}
		public string 药品诊疗类型{get;set;}
		public string 项目医院编号{get;set;}
		public string 项目医院端名称{get;set;}
		public string 医院端规格{get;set;}
		public string 医院端剂型{get;set;}
		public string 单复方标志{get;set;}
		public string 单价{get;set;}
		public string 数量{get;set;}
		public string 单位{get;set;}
		public string 项目总金额{get;set;}
		public string 自付比例{get;set;}
		public string 进口类自负比例{get;set;}
		public string 项目包装数量{get;set;}
		public string 项目最小包装单位{get;set;}
		public string 每天次数{get;set;}
		public string 每次用量{get;set;}
		public string 用量天数{get;set;}
		public string 疾病编码{get;set;}
		public string 项目贴数{get;set;}
		public override string FormatData(){
			return $"{单据号码}%%{药品诊疗类型}%%{项目医院编号}%%{项目医院端名称}%%{医院端规格}%%{医院端剂型}%%{单复方标志}%%{单价}%%{数量}%%{单位}%%{项目总金额}%%{自付比例}%%{进口类自负比例}%%{项目包装数量}%%{项目最小包装单位}%%{每天次数}%%{每次用量}%%{用量天数}%%{疾病编码}%%{项目贴数}";
		}
		public override void Descirbe(string[] arr)
	    {
			单据号码=arr[0];
			药品诊疗类型=arr[1];
			项目医院编号=arr[2];
			项目医院端名称=arr[3];
			医院端规格=arr[4];
			医院端剂型=arr[5];
			单复方标志=arr[6];
			单价=arr[7];
			数量=arr[8];
			单位=arr[9];
			项目总金额=arr[10];
			自付比例=arr[11];
			进口类自负比例=arr[12];
			项目包装数量=arr[13];
			项目最小包装单位=arr[14];
			每天次数=arr[15];
			每次用量=arr[16];
			用量天数=arr[17];
			疾病编码=arr[18];
			项目贴数=arr[19];
	    }
	}
	public class 结算单据列表:ItemBase{
		public override int ItemCount=>13;
		public string 单据号{get;set;}
		public string 门诊号{get;set;}
		public string 处方号码{get;set;}
		public string 就诊日期{get;set;}
		public string 收费类型{get;set;}
		public string 科室代码{get;set;}
		public string 科室名称{get;set;}
		public string 医生姓名{get;set;}
		public string 疾病编号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 非医保项目总额{get;set;}
		public string 收费明细条数{get;set;}
		public override string FormatData(){
			return $"{单据号}%%{门诊号}%%{处方号码}%%{就诊日期}%%{收费类型}%%{科室代码}%%{科室名称}%%{医生姓名}%%{疾病编号}%%{疾病名称}%%{疾病描述}%%{非医保项目总额}%%{收费明细条数}";
		}
		public override void Descirbe(string[] arr)
	    {
			单据号=arr[0];
			门诊号=arr[1];
			处方号码=arr[2];
			就诊日期=arr[3];
			收费类型=arr[4];
			科室代码=arr[5];
			科室名称=arr[6];
			医生姓名=arr[7];
			疾病编号=arr[8];
			疾病名称=arr[9];
			疾病描述=arr[10];
			非医保项目总额=arr[11];
			收费明细条数=arr[12];
	    }
	}
	public class 结算收费项目列表:ItemBase{
		public override int ItemCount=>20;
		public string 单据号码{get;set;}
		public string 药品诊疗类型{get;set;}
		public string 项目医院编号{get;set;}
		public string 项目医院端名称{get;set;}
		public string 医院端规格{get;set;}
		public string 医院端剂型{get;set;}
		public string 单复方标志{get;set;}
		public string 单价{get;set;}
		public string 数量{get;set;}
		public string 单位{get;set;}
		public string 项目总金额{get;set;}
		public string 自付比例{get;set;}
		public string 进口类自负比例{get;set;}
		public string 项目包装数量{get;set;}
		public string 项目最小包装单位{get;set;}
		public string 每天次数{get;set;}
		public string 每次用量{get;set;}
		public string 用量天数{get;set;}
		public string 疾病编码{get;set;}
		public string 项目贴数{get;set;}
		public override string FormatData(){
			return $"{单据号码}%%{药品诊疗类型}%%{项目医院编号}%%{项目医院端名称}%%{医院端规格}%%{医院端剂型}%%{单复方标志}%%{单价}%%{数量}%%{单位}%%{项目总金额}%%{自付比例}%%{进口类自负比例}%%{项目包装数量}%%{项目最小包装单位}%%{每天次数}%%{每次用量}%%{用量天数}%%{疾病编码}%%{项目贴数}";
		}
		public override void Descirbe(string[] arr)
	    {
			单据号码=arr[0];
			药品诊疗类型=arr[1];
			项目医院编号=arr[2];
			项目医院端名称=arr[3];
			医院端规格=arr[4];
			医院端剂型=arr[5];
			单复方标志=arr[6];
			单价=arr[7];
			数量=arr[8];
			单位=arr[9];
			项目总金额=arr[10];
			自付比例=arr[11];
			进口类自负比例=arr[12];
			项目包装数量=arr[13];
			项目最小包装单位=arr[14];
			每天次数=arr[15];
			每次用量=arr[16];
			用量天数=arr[17];
			疾病编码=arr[18];
			项目贴数=arr[19];
	    }
	}
	public class 计算结果信息:ItemBase{
		public override int ItemCount=>30;
		public string 费用总额{get;set;}
		public string 自费总额_非医保{get;set;}
		public string 自理总额_目录内自负比例部分{get;set;}
		public string 统筹基金支付{get;set;}
		public string 往年帐户支付{get;set;}
		public string 当年帐户支付{get;set;}
		public string 大额救助支付{get;set;}
		public string 公务员城镇职工补助支付{get;set;}
		public string 专项基金支付{get;set;}
		public string 劳模基金支付{get;set;}
		public string 民政补助支付{get;set;}
		public string 个人现金支付{get;set;}
		public string 转院自负{get;set;}
		public string 起付线{get;set;}
		public string 合计报销金额{get;set;}
		public string 特殊治疗{get;set;}
		public string 特殊治疗自负{get;set;}
		public string 乙类药品{get;set;}
		public string 乙类药品自负{get;set;}
		public string 床位费{get;set;}
		public string 床位费自负{get;set;}
		public string 自费药品{get;set;}
		public string 其他自负{get;set;}
		public string 合计自负{get;set;}
		public string 报销基数{get;set;}
		public string 起伏标准比例{get;set;}
		public string 转院自负比例{get;set;}
		public string 医保年度{get;set;}
		public string 大病救助支付{get;set;}
		public string 家庭账户支付{get;set;}
		public override string FormatData(){
			return $"{费用总额}%%{自费总额_非医保}%%{自理总额_目录内自负比例部分}%%{统筹基金支付}%%{往年帐户支付}%%{当年帐户支付}%%{大额救助支付}%%{公务员城镇职工补助支付}%%{专项基金支付}%%{劳模基金支付}%%{民政补助支付}%%{个人现金支付}%%{转院自负}%%{起付线}%%{合计报销金额}%%{特殊治疗}%%{特殊治疗自负}%%{乙类药品}%%{乙类药品自负}%%{床位费}%%{床位费自负}%%{自费药品}%%{其他自负}%%{合计自负}%%{报销基数}%%{起伏标准比例}%%{转院自负比例}%%{医保年度}%%{大病救助支付}%%{家庭账户支付}";
		}
		public override void Descirbe(string[] arr)
	    {
			费用总额=arr[0];
			自费总额_非医保=arr[1];
			自理总额_目录内自负比例部分=arr[2];
			统筹基金支付=arr[3];
			往年帐户支付=arr[4];
			当年帐户支付=arr[5];
			大额救助支付=arr[6];
			公务员城镇职工补助支付=arr[7];
			专项基金支付=arr[8];
			劳模基金支付=arr[9];
			民政补助支付=arr[10];
			个人现金支付=arr[11];
			转院自负=arr[12];
			起付线=arr[13];
			合计报销金额=arr[14];
			特殊治疗=arr[15];
			特殊治疗自负=arr[16];
			乙类药品=arr[17];
			乙类药品自负=arr[18];
			床位费=arr[19];
			床位费自负=arr[20];
			自费药品=arr[21];
			其他自负=arr[22];
			合计自负=arr[23];
			报销基数=arr[24];
			起伏标准比例=arr[25];
			转院自负比例=arr[26];
			医保年度=arr[27];
			大病救助支付=arr[28];
			家庭账户支付=arr[29];
	    }
	}
	public class 有误收费项目明细返回信息:ItemBase{
		public override int ItemCount=>7;
		public string 药品诊疗类型{get;set;}
		public string 医院项目编码{get;set;}
		public string 正确的自负比例{get;set;}
		public string 正确的进口类自负比例{get;set;}
		public string 正确的自负金额{get;set;}
		public string 不可报原因代码{get;set;}
		public string 不可报原因说明{get;set;}
		public override string FormatData(){
			return $"{药品诊疗类型}%%{医院项目编码}%%{正确的自负比例}%%{正确的进口类自负比例}%%{正确的自负金额}%%{不可报原因代码}%%{不可报原因说明}";
		}
		public override void Descirbe(string[] arr)
	    {
			药品诊疗类型=arr[0];
			医院项目编码=arr[1];
			正确的自负比例=arr[2];
			正确的进口类自负比例=arr[3];
			正确的自负金额=arr[4];
			不可报原因代码=arr[5];
			不可报原因说明=arr[6];
	    }
	}
	public class 超限明细列表:ItemBase{
		public override int ItemCount=>7;
		public string 药品诊疗类型{get;set;}
		public string 医院项目编码{get;set;}
		public string 超限数量{get;set;}
		public string 超限自负金额{get;set;}
		public string 合计自负金额{get;set;}
		public string 超限原因代码{get;set;}
		public string 超限原因说明{get;set;}
		public override string FormatData(){
			return $"{药品诊疗类型}%%{医院项目编码}%%{超限数量}%%{超限自负金额}%%{合计自负金额}%%{超限原因代码}%%{超限原因说明}";
		}
		public override void Descirbe(string[] arr)
	    {
			药品诊疗类型=arr[0];
			医院项目编码=arr[1];
			超限数量=arr[2];
			超限自负金额=arr[3];
			合计自负金额=arr[4];
			超限原因代码=arr[5];
			超限原因说明=arr[6];
	    }
	}
	public class 超限列表结构体:ItemBase{
		public override int ItemCount=>6;
		public string 明细序号{get;set;}
		public string 超限数量{get;set;}
		public string 超限自负金额{get;set;}
		public string 合计自负金额 {get;set;}
		public string 超限原因代码{get;set;}
		public string 超限原因说明{get;set;}
		public override string FormatData(){
			return $"{明细序号}%%{超限数量}%%{超限自负金额}%%{合计自负金额 }%%{超限原因代码}%%{超限原因说明}";
		}
		public override void Descirbe(string[] arr)
	    {
			明细序号=arr[0];
			超限数量=arr[1];
			超限自负金额=arr[2];
			合计自负金额 =arr[3];
			超限原因代码=arr[4];
			超限原因说明=arr[5];
	    }
	}
	public class 自负比例项目列表:ItemBase{
		public override int ItemCount=>5;
		public string 医嘱明细序号{get;set;}
		public string 药品诊疗类型{get;set;}
		public string 医保编码{get;set;}
		public string 医嘱时间{get;set;}
		public string 限制类标志{get;set;}
		public override string FormatData(){
			return $"{医嘱明细序号}%%{药品诊疗类型}%%{医保编码}%%{医嘱时间}%%{限制类标志}";
		}
		public override void Descirbe(string[] arr)
	    {
			医嘱明细序号=arr[0];
			药品诊疗类型=arr[1];
			医保编码=arr[2];
			医嘱时间=arr[3];
			限制类标志=arr[4];
	    }
	}
	public class 自负比例列表:ItemBase{
		public override int ItemCount=>3;
		public string 医嘱明细序号{get;set;}
		public string 自负比列{get;set;}
		public string 进口类自负比例{get;set;}
		public override string FormatData(){
			return $"{医嘱明细序号}%%{自负比列}%%{进口类自负比例}";
		}
		public override void Descirbe(string[] arr)
	    {
			医嘱明细序号=arr[0];
			自负比列=arr[1];
			进口类自负比例=arr[2];
	    }
	}
	public class 登记信息列表:ItemBase{
		public override int ItemCount=>9;
		public string 病种类别{get;set;}
		public string 病种证书号{get;set;}
		public string 特殊病代码{get;set;}
		public string 特殊病名称{get;set;}
		public string 并发症代码{get;set;}
		public string 并发症名称{get;set;}
		public string 有效开始日期{get;set;}
		public string 有效结束日期{get;set;}
		public string 备注{get;set;}
		public override string FormatData(){
			return $"{病种类别}%%{病种证书号}%%{特殊病代码}%%{特殊病名称}%%{并发症代码}%%{并发症名称}%%{有效开始日期}%%{有效结束日期}%%{备注}";
		}
		public override void Descirbe(string[] arr)
	    {
			病种类别=arr[0];
			病种证书号=arr[1];
			特殊病代码=arr[2];
			特殊病名称=arr[3];
			并发症代码=arr[4];
			并发症名称=arr[5];
			有效开始日期=arr[6];
			有效结束日期=arr[7];
			备注=arr[8];
	    }
	}
}