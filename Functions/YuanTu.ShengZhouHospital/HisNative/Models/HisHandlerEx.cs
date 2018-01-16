

namespace YuanTu.ShengZhouHospital.HisNative.Models
{
	public static partial class HisHandleEx{
		
		public static Res建档 执行建档(Req建档 req){return Handler<Req建档,Res建档>(req,"建档",180);}
		
		public static Res门诊读卡 执行门诊读卡(Req门诊读卡 req){return Handler<Req门诊读卡,Res门诊读卡>(req,"门诊读卡",180);}
		
		public static Res挂号取号预结算 执行挂号取号预结算(Req挂号取号预结算 req){return Handler<Req挂号取号预结算,Res挂号取号预结算>(req,"挂号取号预结算",180);}
		
		public static Res挂号取号结算 执行挂号取号结算(Req挂号取号结算 req){return Handler<Req挂号取号结算,Res挂号取号结算>(req,"挂号取号结算",180);}
		
		public static Res挂号取号回滚 执行挂号取号回滚(Req挂号取号回滚 req){return Handler<Req挂号取号回滚,Res挂号取号回滚>(req,"挂号取号回滚",180);}
		
		public static ResHIS缴费预结算 执行HIS缴费预结算(ReqHIS缴费预结算 req){return Handler<ReqHIS缴费预结算,ResHIS缴费预结算>(req,"HIS缴费预结算",180);}
		
		public static ResHIS缴费结算 执行HIS缴费结算(ReqHIS缴费结算 req){return Handler<ReqHIS缴费结算,ResHIS缴费结算>(req,"HIS缴费结算",180);}
		
		public static Res导引单 执行导引单(Req导引单 req){return Handler<Req导引单,Res导引单>(req,"导引单",180);}
		
		public static Res收费加锁解锁 执行收费加锁解锁(Req收费加锁解锁 req){return Handler<Req收费加锁解锁,Res收费加锁解锁>(req,"收费加锁解锁",180);}
	
	}

	
    #region 建档[GC0202]
	public class Req建档:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0202";
		
		/// <summary>
		/// 1一卡通 2医保卡
		/// </summary>
		public string 卡类别{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 1男2女
		/// </summary>
		public string 性别{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 居住地址{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 出生日期{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 证件号码{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 手机号码{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 卡号{get;set;}
			
		protected override string Build(){
			return $"GC0202"+"|"+卡类别+"|"+姓名+"|"+性别+"|"+居住地址+"|"+出生日期+"|"+证件号码+"|"+手机号码+"|"+卡号;
		 }
	}
	public class Res建档:HisRes{

	public override int ArrLen =>0;
	
		public override void Build(string[] arrs){
			
		}

	}
	#endregion


    #region 门诊读卡[GC0101]
	public class Req门诊读卡:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0101";
		
		/// <summary>
		/// 1一卡通 2医保卡
		/// </summary>
		public string 卡类别{get;set;}
			
		protected override string Build(){
			return $"GC0101"+"|"+卡类别;
		 }
	}
	public class Res门诊读卡:HisRes{

	public override int ArrLen =>8;
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 1男2女
		/// </summary>
		public string 性别{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 证件类型{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 证件号码{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 手机号码{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 出生日期{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 居住地址{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 卡号{get;set;}
	
		public override void Build(string[] arrs){
			
				姓名=arrs[1];
			
				性别=arrs[2];
			
				证件类型=arrs[3];
			
				证件号码=arrs[4];
			
				手机号码=arrs[5];
			
				出生日期=arrs[6];
			
				居住地址=arrs[7];
			
				卡号=arrs[8];
			
		}

	}
	#endregion


    #region 挂号取号预结算[GC0401]
	public class Req挂号取号预结算:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0401";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 排班表主键{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 科室编号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医生工号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号类型{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号时间{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保类型{get;set;}
		
		/// <summary>
		/// 0：正常挂号1：预约取号
		/// </summary>
		public string 预约标记{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 预约记录主键{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 诊疗费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 工本费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 现金结算单流水号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 现金结算金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 程序名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 操作科室{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 终端编号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 值班类别{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 支付类别{get;set;}
			
		protected override string Build(){
			return $"GC0401"+"|"+患者唯一标识+"|"+姓名+"|"+排班表主键+"|"+科室编号+"|"+医生工号+"|"+挂号类型+"|"+挂号时间+"|"+医保类型+"|"+预约标记+"|"+预约记录主键+"|"+挂号费+"|"+诊疗费+"|"+工本费+"|"+就诊序号+"|"+挂号序号+"|"+现金结算单流水号+"|"+现金结算金额+"|"+总金额+"|"+程序名+"|"+操作科室+"|"+终端编号+"|"+值班类别+"|"+支付类别;
		 }
	}
	public class Res挂号取号预结算:HisRes{

	public override int ArrLen =>6;
		
		/// <summary>
		/// 
		/// </summary>
		public string 返回信息{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 个人现金支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 结算备注{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保类型{get;set;}
	
		public override void Build(string[] arrs){
			
				返回信息=arrs[1];
			
				总金额=arrs[2];
			
				医保支付金额=arrs[3];
			
				个人现金支付金额=arrs[4];
			
				结算备注=arrs[5];
			
				医保类型=arrs[6];
			
		}

	}
	#endregion


    #region 挂号取号结算[GC0402]
	public class Req挂号取号结算:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0402";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 排班表主键{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 科室编号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医生工号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号类型{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号时间{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保类型{get;set;}
		
		/// <summary>
		/// 0：正常挂号1：预约取号
		/// </summary>
		public string 预约标记{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 预约记录主键{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 诊疗费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 工本费{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 现金结算单流水号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 现金结算金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 程序名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 操作科室{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 终端编号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 值班类别{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 支付类别{get;set;}
			
		protected override string Build(){
			return $"GC0402"+"|"+患者唯一标识+"|"+姓名+"|"+排班表主键+"|"+科室编号+"|"+医生工号+"|"+挂号类型+"|"+挂号时间+"|"+医保类型+"|"+预约标记+"|"+预约记录主键+"|"+挂号费+"|"+诊疗费+"|"+工本费+"|"+就诊序号+"|"+挂号序号+"|"+现金结算单流水号+"|"+现金结算金额+"|"+总金额+"|"+程序名+"|"+操作科室+"|"+终端编号+"|"+值班类别+"|"+支付类别;
		 }
	}
	public class Res挂号取号结算:HisRes{

	public override int ArrLen =>9;
		
		/// <summary>
		/// 
		/// </summary>
		public string 返回信息{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 挂号发票号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 个人现金支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 结算备注{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊地点{get;set;}
	
		public override void Build(string[] arrs){
			
				返回信息=arrs[1];
			
				挂号序号=arrs[2];
			
				就诊序号=arrs[3];
			
				挂号发票号=arrs[4];
			
				总金额=arrs[5];
			
				医保支付金额=arrs[6];
			
				个人现金支付金额=arrs[7];
			
				结算备注=arrs[8];
			
				就诊地点=arrs[9];
			
		}

	}
	#endregion


    #region 挂号取号回滚[GC0501]
	public class Req挂号取号回滚:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0501";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 原挂号记录序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 预约标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 预约记录序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string SB联众开发{get;set;}
			
		protected override string Build(){
			return $"GC0501"+"|"+患者唯一标识+"|"+姓名+"|"+原挂号记录序号+"|"+预约标识+"|"+预约记录序号+"|"+SB联众开发;
		 }
	}
	public class Res挂号取号回滚:HisRes{

	public override int ArrLen =>0;
	
		public override void Build(string[] arrs){
			
		}

	}
	#endregion


    #region HIS缴费预结算[GC0604]
	public class ReqHIS缴费预结算:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0604";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保类型{get;set;}
		
		/// <summary>
		/// 类型/费用序号, 类型/费用序号，类型分为1：表示处方2：表示费用
		/// </summary>
		public string 处方单号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总费用{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 银行结算流水号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 银行支付费用{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 程序名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 操作科室{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 终端编号{get;set;}
		
		/// <summary>
		/// 1：银联2：支付宝3：微信)
		/// </summary>
		public string 支付类型{get;set;}
			
		protected override string Build(){
			return $"GC0604"+"|"+患者唯一标识+"|"+姓名+"|"+就诊序号+"|"+医保类型+"|"+处方单号+"|"+总费用+"|"+银行结算流水号+"|"+银行支付费用+"|"+程序名+"|"+操作科室+"|"+终端编号+"|"+支付类型;
		 }
	}
	public class ResHIS缴费预结算:HisRes{

	public override int ArrLen =>5;
		
		/// <summary>
		/// 
		/// </summary>
		public string 返回信息{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 个人现金支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 结算备注{get;set;}
	
		public override void Build(string[] arrs){
			
				返回信息=arrs[1];
			
				总金额=arrs[2];
			
				医保支付金额=arrs[3];
			
				个人现金支付金额=arrs[4];
			
				结算备注=arrs[5];
			
		}

	}
	#endregion


    #region HIS缴费结算[GC0605]
	public class ReqHIS缴费结算:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0605";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 就诊序号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保类型{get;set;}
		
		/// <summary>
		/// 类型/费用序号, 类型/费用序号，类型分为1：表示处方2：表示费用
		/// </summary>
		public string 处方单号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总费用{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 银行结算流水号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 银行支付费用{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 程序名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 操作科室{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 终端编号{get;set;}
		
		/// <summary>
		/// 1：银联2：支付宝3：微信)
		/// </summary>
		public string 支付类型{get;set;}
			
		protected override string Build(){
			return $"GC0605"+"|"+患者唯一标识+"|"+姓名+"|"+就诊序号+"|"+医保类型+"|"+处方单号+"|"+总费用+"|"+银行结算流水号+"|"+银行支付费用+"|"+程序名+"|"+操作科室+"|"+终端编号+"|"+支付类型;
		 }
	}
	public class ResHIS缴费结算:HisRes{

	public override int ArrLen =>9;
		
		/// <summary>
		/// 
		/// </summary>
		public string 返回信息{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 发票号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 取药地点{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 总金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 医保支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 个人现金支付金额{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 结算备注{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 电脑号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 导引单信息{get;set;}
	
		public override void Build(string[] arrs){
			
				返回信息=arrs[1];
			
				发票号=arrs[2];
			
				取药地点=arrs[3];
			
				总金额=arrs[4];
			
				医保支付金额=arrs[5];
			
				个人现金支付金额=arrs[6];
			
				结算备注=arrs[7];
			
				电脑号=arrs[8];
			
				导引单信息=arrs[9];
			
		}

	}
	#endregion


    #region 导引单[T002]
	public class Req导引单:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"T002";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 处方单号{get;set;}
			
		protected override string Build(){
			return $"T002"+"|"+患者唯一标识+"|"+姓名+"|"+处方单号;
		 }
	}
	public class Res导引单:HisRes{

	public override int ArrLen =>3;
		
		/// <summary>
		/// 
		/// </summary>
		public string 取药窗口{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 检验就诊处{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 检查就诊处{get;set;}
	
		public override void Build(string[] arrs){
			
				取药窗口=arrs[1];
			
				检验就诊处=arrs[2];
			
				检查就诊处=arrs[3];
			
		}

	}
	#endregion


    #region 收费加锁解锁[GC0701]
	public class Req收费加锁解锁:HisReq{
		/// <summary>
		/// 服务编号
		/// </summary>
		public override string 服务编号 =>"GC0701";
		
		/// <summary>
		/// 
		/// </summary>
		public string 患者唯一标识{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 姓名{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 处方单号{get;set;}
		
		/// <summary>
		/// 
		/// </summary>
		public string 加锁标志{get;set;}
			
		protected override string Build(){
			return $"GC0701"+"|"+患者唯一标识+"|"+姓名+"|"+处方单号+"|"+加锁标志;
		 }
	}
	public class Res收费加锁解锁:HisRes{

	public override int ArrLen =>1;
		
		/// <summary>
		/// 
		/// </summary>
		public string 返回信息{get;set;}
	
		public override void Build(string[] arrs){
			
				返回信息=arrs[1];
			
		}

	}
	#endregion

}