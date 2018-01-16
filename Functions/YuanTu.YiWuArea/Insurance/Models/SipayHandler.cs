
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.YiWuArea.Insurance.Models.Base;


namespace YuanTu.YiWuArea.Insurance.Models{

	public static partial class SipayHandler{
		public static Res签到 调用签到(Req签到 req){return Handler<Req签到, Res签到>(req,94);}
		public static Res获取参保人员信息 调用获取参保人员信息(Req获取参保人员信息 req){return Handler<Req获取参保人员信息, Res获取参保人员信息>(req,22);}
		public static Res门诊挂号预结算 调用门诊挂号预结算(Req门诊挂号预结算 req){return Handler<Req门诊挂号预结算, Res门诊挂号预结算>(req,27);}
		public static Res门诊挂号结算 调用门诊挂号结算(Req门诊挂号结算 req){return Handler<Req门诊挂号结算, Res门诊挂号结算>(req,28);}
		public static Res门诊预结算 调用门诊预结算(Req门诊预结算 req){return Handler<Req门诊预结算, Res门诊预结算>(req,29);}
		public static Res门诊结算 调用门诊结算(Req门诊结算 req){return Handler<Req门诊结算, Res门诊结算>(req,30);}
		public static Res住院预结算 调用住院预结算(Req住院预结算 req){return Handler<Req住院预结算, Res住院预结算>(req,34);}
		public static Res出院结算 调用出院结算(Req出院结算 req){return Handler<Req出院结算, Res出院结算>(req,36);}
		public static Res住院信息变动 调用住院信息变动(Req住院信息变动 req){return Handler<Req住院信息变动, Res住院信息变动>(req,38);}
		public static Res交易确认 调用交易确认(Req交易确认 req){return Handler<Req交易确认, Res交易确认>(req,49);}
		public static Res单价限额自负比列获取 调用单价限额自负比列获取(Req单价限额自负比列获取 req){return Handler<Req单价限额自负比列获取, Res单价限额自负比列获取>(req,58);}
		public static Res批量获取自负比例 调用批量获取自负比例(Req批量获取自负比例 req){return Handler<Req批量获取自负比例, Res批量获取自负比例>(req,71);}
		public static Res住院分娩结算 调用住院分娩结算(Req住院分娩结算 req){return Handler<Req住院分娩结算, Res住院分娩结算>(req,73);}
		public static Res病种登记信息下载 调用病种登记信息下载(Req病种登记信息下载 req){return Handler<Req病种登记信息下载, Res病种登记信息下载>(req,74);}
		public static Res无卡退费 调用无卡退费(Req无卡退费 req){return Handler<Req无卡退费, Res无卡退费>(req,82);}
	}

	public class Req签到:InsuranceRequestBase
	{
		public override string TradeName=>"签到";
		public override string Ic信息 { get;set;}
		public string 操作员账号{get;set;}


		public override string DataFormat(){
			return $"{操作员账号}";
		}
	}
	public class Req获取参保人员信息:InsuranceRequestBase
	{
		public override string TradeName=>"获取参保人员信息";
		public override string Ic信息 { get;set;}
		public string 读卡方式{get;set;}


		public override string DataFormat(){
			return $"{读卡方式}";
		}
	}
	public class Req门诊挂号预结算:InsuranceRequestBase
	{
		public override string TradeName=>"门诊挂号预结算";
		public override string Ic信息 { get;set;}
		public string 收费类型{get;set;}
		public string 门诊号{get;set;}
		public string 疾病编号{get;set;}
		public string 病种审批号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 本次结算单据张数{get;set;}
		public ItemList<预算单据列表> 单据列表{get;set;}
		public ItemList<预算收费项目列表> 收费项目列表{get;set;}
		public string 是否需要个帐支付{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{收费类型}~{门诊号}~{疾病编号}~{病种审批号}~{疾病名称}~{疾病描述}~{本次结算单据张数}~{单据列表}~{收费项目列表}~{是否需要个帐支付}~{对账流水号}";
		}
	}
	public class Req门诊挂号结算:InsuranceRequestBase
	{
		public override string TradeName=>"门诊挂号结算";
		public override string Ic信息 { get;set;}
		public string 收费类型{get;set;}
		public string 门诊号{get;set;}
		public string 疾病编号{get;set;}
		public string 病种审批号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 本次结算单据张数{get;set;}
		public ItemList<结算单据列表> 单据列表{get;set;}
		public ItemList<结算收费项目列表> 收费项目列表{get;set;}
		public string 经办人{get;set;}
		public string 是否需要个帐支付{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{收费类型}~{门诊号}~{疾病编号}~{病种审批号}~{疾病名称}~{疾病描述}~{本次结算单据张数}~{单据列表}~{收费项目列表}~{经办人}~{是否需要个帐支付}~{对账流水号}";
		}
	}
	public class Req门诊预结算:InsuranceRequestBase
	{
		public override string TradeName=>"门诊预结算";
		public override string Ic信息 { get;set;}
		public string 收费类型{get;set;}
		public string 门诊号{get;set;}
		public string 疾病编号{get;set;}
		public string 病种审批号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 本次结算单据张数{get;set;}
		public ItemList<预算单据列表> 单据列表{get;set;}
		public ItemList<预算收费项目列表> 收费项目列表{get;set;}
		public string 是否需要个帐支付{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{收费类型}~{门诊号}~{疾病编号}~{病种审批号}~{疾病名称}~{疾病描述}~{本次结算单据张数}~{单据列表}~{收费项目列表}~{是否需要个帐支付}~{对账流水号}";
		}
	}
	public class Req门诊结算:InsuranceRequestBase
	{
		public override string TradeName=>"门诊结算";
		public override string Ic信息 { get;set;}
		public string 收费类型{get;set;}
		public string 门诊号{get;set;}
		public string 疾病编号{get;set;}
		public string 病种审批号{get;set;}
		public string 疾病名称{get;set;}
		public string 疾病描述{get;set;}
		public string 本次结算单据张数{get;set;}
		public ItemList<结算单据列表> 单据列表{get;set;}
		public ItemList<结算收费项目列表> 收费项目列表{get;set;}
		public string 经办人{get;set;}
		public string 是否需要个帐支付{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{收费类型}~{门诊号}~{疾病编号}~{病种审批号}~{疾病名称}~{疾病描述}~{本次结算单据张数}~{单据列表}~{收费项目列表}~{经办人}~{是否需要个帐支付}~{对账流水号}";
		}
	}
	public class Req住院预结算:InsuranceRequestBase
	{
		public override string TradeName=>"住院预结算";
		public override string Ic信息 { get;set;}
		public string 结算类型{get;set;}
		public string 转诊转院申请单编号{get;set;}
		public string 住院号{get;set;}
		public string 住院登记交易号{get;set;}
		public string 本次结算明细条数{get;set;}
		public string 是否需要个帐支付{get;set;}


		public override string DataFormat(){
			return $"{结算类型}~{转诊转院申请单编号}~{住院号}~{住院登记交易号}~{本次结算明细条数}~{是否需要个帐支付}";
		}
	}
	public class Req出院结算:InsuranceRequestBase
	{
		public override string TradeName=>"出院结算";
		public override string Ic信息 { get;set;}
		public string 结算类型{get;set;}
		public string 转诊转院申请单编号{get;set;}
		public string 住院号{get;set;}
		public string 住院登记交易号{get;set;}
		public string 本次结算明细条数{get;set;}
		public string 是否需要个帐支付{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{结算类型}~{转诊转院申请单编号}~{住院号}~{住院登记交易号}~{本次结算明细条数}~{是否需要个帐支付}~{对账流水号}";
		}
	}
	public class Req住院信息变动:InsuranceRequestBase
	{
		public override string TradeName=>"住院信息变动";
		public override string Ic信息 { get;set;}
		public string 住院号{get;set;}
		public string 住院登记交易号{get;set;}
		public string 出院日期{get;set;}
		public string 病人床号{get;set;}
		public string 诊断医生姓名{get;set;}
		public string 诊断描述{get;set;}
		public string 疾病编号{get;set;}
		public string 疾病名称{get;set;}
		public string 科室名称{get;set;}
		public string 科室编号{get;set;}
		public string 出院原因{get;set;}
		public string 变动类型{get;set;}


		public override string DataFormat(){
			return $"{住院号}~{住院登记交易号}~{出院日期}~{病人床号}~{诊断医生姓名}~{诊断描述}~{疾病编号}~{疾病名称}~{科室名称}~{科室编号}~{出院原因}~{变动类型}";
		}
	}
	public class Req交易确认:InsuranceRequestBase
	{
		public override string TradeName=>"交易确认";
		public override string Ic信息 { get;set;}
		public string 交易类型{get;set;}
		public string 医保交易流水号{get;set;}
		public string HIS事务结果{get;set;}
		public string 附加信息{get;set;}
		public string 是否需要诊间结算{get;set;}


		public override string DataFormat(){
			return $"{交易类型}~{医保交易流水号}~{HIS事务结果}~{附加信息}~{是否需要诊间结算}";
		}
	}
	public class Req单价限额自负比列获取:InsuranceRequestBase
	{
		public override string TradeName=>"单价限额自负比列获取";
		public override string Ic信息 { get;set;}
		public string 类别标志{get;set;}
		public string 项目编号{get;set;}
		public string 医疗人员类别{get;set;}
		public string 特殊病标志{get;set;}
		public string 医院等级{get;set;}
		public string 医疗类别{get;set;}
		public string 单复方标志{get;set;}
		public string 限制类标志{get;set;}
		public string 疾病编码{get;set;}
		public string 交易时间{get;set;}


		public override string DataFormat(){
			return $"{类别标志}~{项目编号}~{医疗人员类别}~{特殊病标志}~{医院等级}~{医疗类别}~{单复方标志}~{限制类标志}~{疾病编码}~{交易时间}";
		}
	}
	public class Req批量获取自负比例:InsuranceRequestBase
	{
		public override string TradeName=>"批量获取自负比例";
		public override string Ic信息 { get;set;}
		public string 医保待遇{get;set;}
		public ItemList<自负比例项目列表> 项目列表{get;set;}


		public override string DataFormat(){
			return $"{医保待遇}~{项目列表}";
		}
	}
	public class Req住院分娩结算:InsuranceRequestBase
	{
		public override string TradeName=>"住院分娩结算";
		public override string Ic信息 { get;set;}
		public string 准生证号码{get;set;}
		public string 出生证号码{get;set;}
		public string 婴儿出生日期{get;set;}
		public string 分娩种类{get;set;}
		public string 住院总费用{get;set;}


		public override string DataFormat(){
			return $"{准生证号码}~{出生证号码}~{婴儿出生日期}~{分娩种类}~{住院总费用}";
		}
	}
	public class Req病种登记信息下载:InsuranceRequestBase
	{
		public override string TradeName=>"病种登记信息下载";
		public override string Ic信息 { get;set;}
		public string 病种类别{get;set;}
		public string 病种证书号{get;set;}


		public override string DataFormat(){
			return $"{病种类别}~{病种证书号}";
		}
	}
	public class Req无卡退费:InsuranceRequestBase
	{
		public override string TradeName=>"无卡退费";
		public override string Ic信息 { get;set;}
		public string 要作废的结算交易号{get;set;}
		public string 经办人{get;set;}
		public string 对账流水号{get;set;}


		public override string DataFormat(){
			对账流水号=对账流水号??SipayHandler.SiToken;
			return $"{要作废的结算交易号}~{经办人}~{对账流水号}";
		}
	}

	public class Res签到:InsuranceResponseBase
	{
		
		public string 操作员账号{get;set;}
		public string 日对账流水号{get;set;}
		public string 签到时间{get;set;}


		public override void DataFormat(string[] arr){
			操作员账号=InternalTools.GetValueBack(操作员账号,arr[5]);
			日对账流水号=InternalTools.GetValueBack(日对账流水号,arr[6]);
			签到时间=InternalTools.GetValueBack(签到时间,arr[7]);
		}
	}
	public class Res获取参保人员信息:InsuranceResponseBase
	{
		
		public string 身份验证结果{get;set;}
		public string 封锁原因{get;set;}
		public string 开户银行{get;set;}


		public override void DataFormat(string[] arr){
			身份验证结果=InternalTools.GetValueBack(身份验证结果,arr[5]);
			封锁原因=InternalTools.GetValueBack(封锁原因,arr[6]);
			开户银行=InternalTools.GetValueBack(开户银行,arr[7]);
		}
	}
	public class Res门诊挂号预结算:InsuranceResponseBase
	{
		
		public string 超限提示标记{get;set;}
		public string 规定病种标志{get;set;}
		public string 结算时间{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<有误收费项目明细返回信息> 有误收费项目明细返回信息{get;set;}
		public ItemList<超限明细列表> 超限明细列表{get;set;}


		public override void DataFormat(string[] arr){
			超限提示标记=InternalTools.GetValueBack(超限提示标记,arr[5]);
			规定病种标志=InternalTools.GetValueBack(规定病种标志,arr[6]);
			结算时间=InternalTools.GetValueBack(结算时间,arr[7]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[8]);
			有误收费项目明细返回信息=InternalTools.GetValueBack(有误收费项目明细返回信息,arr[9]);
			超限明细列表=InternalTools.GetValueBack(超限明细列表,arr[10]);
		}
	}
	public class Res门诊挂号结算:InsuranceResponseBase
	{
		
		public string 超限提示标记{get;set;}
		public string 规定病种标志{get;set;}
		public string 结算时间{get;set;}
		public string 结算流水号{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<有误收费项目明细返回信息> 有误收费项目明细返回信息{get;set;}
		public ItemList<超限明细列表> 超限明细列表{get;set;}


		public override void DataFormat(string[] arr){
			超限提示标记=InternalTools.GetValueBack(超限提示标记,arr[5]);
			规定病种标志=InternalTools.GetValueBack(规定病种标志,arr[6]);
			结算时间=InternalTools.GetValueBack(结算时间,arr[7]);
			结算流水号=InternalTools.GetValueBack(结算流水号,arr[8]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[9]);
			有误收费项目明细返回信息=InternalTools.GetValueBack(有误收费项目明细返回信息,arr[10]);
			超限明细列表=InternalTools.GetValueBack(超限明细列表,arr[11]);
		}
	}
	public class Res门诊预结算:InsuranceResponseBase
	{
		
		public string 超限提示标记{get;set;}
		public string 规定病种标志{get;set;}
		public string 结算时间{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<有误收费项目明细返回信息> 有误收费项目明细返回信息{get;set;}
		public ItemList<超限明细列表> 超限明细列表{get;set;}


		public override void DataFormat(string[] arr){
			超限提示标记=InternalTools.GetValueBack(超限提示标记,arr[5]);
			规定病种标志=InternalTools.GetValueBack(规定病种标志,arr[6]);
			结算时间=InternalTools.GetValueBack(结算时间,arr[7]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[8]);
			有误收费项目明细返回信息=InternalTools.GetValueBack(有误收费项目明细返回信息,arr[9]);
			超限明细列表=InternalTools.GetValueBack(超限明细列表,arr[10]);
		}
	}
	public class Res门诊结算:InsuranceResponseBase
	{
		
		public string 超限提示标记{get;set;}
		public string 规定病种标志{get;set;}
		public string 结算时间{get;set;}
		public string 结算流水号{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<有误收费项目明细返回信息> 有误收费项目明细返回信息{get;set;}
		public ItemList<超限明细列表> 超限明细列表{get;set;}


		public override void DataFormat(string[] arr){
			超限提示标记=InternalTools.GetValueBack(超限提示标记,arr[5]);
			规定病种标志=InternalTools.GetValueBack(规定病种标志,arr[6]);
			结算时间=InternalTools.GetValueBack(结算时间,arr[7]);
			结算流水号=InternalTools.GetValueBack(结算流水号,arr[8]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[9]);
			有误收费项目明细返回信息=InternalTools.GetValueBack(有误收费项目明细返回信息,arr[10]);
			超限明细列表=InternalTools.GetValueBack(超限明细列表,arr[11]);
		}
	}
	public class Res住院预结算:InsuranceResponseBase
	{
		
		public string 住院结算交易交流水号{get;set;}
		public string 结算日期{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<超限列表结构体> 超限列表结构体{get;set;}


		public override void DataFormat(string[] arr){
			住院结算交易交流水号=InternalTools.GetValueBack(住院结算交易交流水号,arr[5]);
			结算日期=InternalTools.GetValueBack(结算日期,arr[6]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[7]);
			超限列表结构体=InternalTools.GetValueBack(超限列表结构体,arr[8]);
		}
	}
	public class Res出院结算:InsuranceResponseBase
	{
		
		public string 住院结算交易交流水号{get;set;}
		public string 结算日期{get;set;}
		public 计算结果信息 计算结果信息{get;set;}
		public ItemList<超限列表结构体> 超限列表结构体{get;set;}


		public override void DataFormat(string[] arr){
			住院结算交易交流水号=InternalTools.GetValueBack(住院结算交易交流水号,arr[5]);
			结算日期=InternalTools.GetValueBack(结算日期,arr[6]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[7]);
			超限列表结构体=InternalTools.GetValueBack(超限列表结构体,arr[8]);
		}
	}
	public class Res住院信息变动:InsuranceResponseBase
	{
		
		public string 交易状态{get;set;}
		public string 错误信息{get;set;}
		public string 写医保卡结果{get;set;}
		public string 扣银行卡结果{get;set;}
		public string 写卡后IC卡数据{get;set;}


		public override void DataFormat(string[] arr){
			交易状态=InternalTools.GetValueBack(交易状态,arr[5]);
			错误信息=InternalTools.GetValueBack(错误信息,arr[6]);
			写医保卡结果=InternalTools.GetValueBack(写医保卡结果,arr[7]);
			扣银行卡结果=InternalTools.GetValueBack(扣银行卡结果,arr[8]);
			写卡后IC卡数据=InternalTools.GetValueBack(写卡后IC卡数据,arr[9]);
		}
	}
	public class Res交易确认:InsuranceResponseBase
	{
		
		public string 交易流水号{get;set;}
		public string 诊间结算流水号{get;set;}


		public override void DataFormat(string[] arr){
			交易流水号=InternalTools.GetValueBack(交易流水号,arr[5]);
			诊间结算流水号=InternalTools.GetValueBack(诊间结算流水号,arr[6]);
		}
	}
	public class Res单价限额自负比列获取:InsuranceResponseBase
	{
		
		public string 自负比列{get;set;}
		public string 进口类自负比例{get;set;}


		public override void DataFormat(string[] arr){
			自负比列=InternalTools.GetValueBack(自负比列,arr[5]);
			进口类自负比例=InternalTools.GetValueBack(进口类自负比例,arr[6]);
		}
	}
	public class Res批量获取自负比例:InsuranceResponseBase
	{
		
		public ItemList<自负比例列表> 自负比例列表{get;set;}


		public override void DataFormat(string[] arr){
			自负比例列表=InternalTools.GetValueBack(自负比例列表,arr[5]);
		}
	}
	public class Res住院分娩结算:InsuranceResponseBase
	{
		
		public string 报销金额{get;set;}


		public override void DataFormat(string[] arr){
			报销金额=InternalTools.GetValueBack(报销金额,arr[5]);
		}
	}
	public class Res病种登记信息下载:InsuranceResponseBase
	{
		
		public ItemList<登记信息列表> 登记信息列表{get;set;}


		public override void DataFormat(string[] arr){
			登记信息列表=InternalTools.GetValueBack(登记信息列表,arr[5]);
		}
	}
	public class Res无卡退费:InsuranceResponseBase
	{
		
		public string 是否重复退费{get;set;}
		public string 退费交易流水号{get;set;}
		public string 退费结算日期{get;set;}
		public 计算结果信息 计算结果信息{get;set;}


		public override void DataFormat(string[] arr){
			是否重复退费=InternalTools.GetValueBack(是否重复退费,arr[5]);
			退费交易流水号=InternalTools.GetValueBack(退费交易流水号,arr[6]);
			退费结算日期=InternalTools.GetValueBack(退费结算日期,arr[7]);
			计算结果信息=InternalTools.GetValueBack(计算结果信息,arr[8]);
		}
	}


}