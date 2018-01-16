using System.Collections.Generic;
namespace YuanTu.PanYu.House.PanYuGateway
{
    public partial class 病人信息
    {
		public string patientId { get; set; }
		public string platformId { get; set; }
		public string name { get; set; }
		public string sex { get; set; }
		public string birthday { get; set; }
		public string idNo { get; set; }
		public string cardNo { get; set; }
		public string guardianNo { get; set; }
		public string address { get; set; }
		public string phone { get; set; }
		public string patientType { get; set; }
		public string accountNo { get; set; }
		public string accBalance { get; set; }
    }

    public partial class 建档信息
    {
		public string cardNo { get; set; }
		public string patientId { get; set; }
    }

    public partial class 就诊情况记录
    {
		public string tradeTime { get; set; }
		public string deptName { get; set; }
		public string doctName { get; set; }
		public string info { get; set; }
		public string billNo { get; set; }
    }

    public partial class 病人基本信息修改结果
    {
    }

    public partial class 缴费概要信息
    {
		public string billNo { get; set; }
		public string billDate { get; set; }
		public string billType { get; set; }
		public string billFee { get; set; }
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string regId { get; set; }
		public string appoNo { get; set; }
		public string regNo { get; set; }
		public string billGroupNo { get; set; }
		public string extendBalanceInfo { get; set; }
		public string diseaseCode { get; set; }
		public string diseaseName { get; set; }
		public string diseaseType { get; set; }
		public string receiptType { get; set; }
		public string receiptTypeName { get; set; }
		public List<缴费明细信息> billItem { get; set; }
    }

    public partial class 缴费明细信息
    {
		public string billNo { get; set; }
		public string billDate { get; set; }
		public string billType { get; set; }
		public string billFee { get; set; }
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string itemNo { get; set; }
		public string productCode { get; set; }
		public string itemName { get; set; }
		public string itemSpecs { get; set; }
		public string itemLiquid { get; set; }
		public string itemUnits { get; set; }
		public string itemQty { get; set; }
		public string itemPrice { get; set; }
		public string hosFeeNo { get; set; }
		public string diseaseCode { get; set; }
		public string stapleFlag { get; set; }
		public string matchType { get; set; }
    }

    public partial class 预结算结果
    {
		public string selfFee { get; set; }
		public string insurFee { get; set; }
		public string insurFeeInfo { get; set; }
		public string payAccount { get; set; }
		public string billFee { get; set; }
		public string otherFee { get; set; }
		public string extend { get; set; }
    }

    public partial class 结算结果
    {
		public string selfFee { get; set; }
		public string insurFeeInfo { get; set; }
		public string insurFee { get; set; }
		public string payAccount { get; set; }
		public string takeMedWin { get; set; }
		public string hasMoreFee { get; set; }
		public string transNo { get; set; }
		public string receiptNo { get; set; }
		public string oppatNo { get; set; }
		public List<结算明细信息> itemDetail { get; set; }
    }

    public partial class 结算明细信息
    {
		public string billDate { get; set; }
		public string itemNo { get; set; }
		public string productCode { get; set; }
		public string itemName { get; set; }
		public string itemSpecs { get; set; }
		public string itemLiquid { get; set; }
		public string itemUnits { get; set; }
		public string itemQty { get; set; }
		public string itemPrice { get; set; }
		public string cost { get; set; }
    }

    public partial class 结算记录
    {
		public string tradeTime { get; set; }
		public string receiptNo { get; set; }
		public string itemName { get; set; }
		public string itemSpecs { get; set; }
		public string itemLiquid { get; set; }
		public string itemUnits { get; set; }
		public string itemQty { get; set; }
		public string itemPrice { get; set; }
		public string cost { get; set; }
    }

    public partial class 充值结果
    {
		public string receiptNo { get; set; }
		public string cash { get; set; }
    }

    public partial class 充值记录
    {
		public string tradeMode { get; set; }
		public string accountNo { get; set; }
		public string cash { get; set; }
		public string receiptNo { get; set; }
		public string tradeTime { get; set; }
		public string operId { get; set; }
    }

    public partial class 账户余额
    {
		public string receiptNo { get; set; }
		public string cash { get; set; }
    }

    public partial class 排班科室信息
    {
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string parentDeptCode { get; set; }
		public string parentDeptName { get; set; }
		public string simplePy { get; set; }
		public string fullPy { get; set; }
    }

    public partial class 排班医生信息
    {
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string parentDeptCode { get; set; }
		public string parentDeptName { get; set; }
		public string medDate { get; set; }
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string doctTech { get; set; }
		public string medAmPm { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
		public string scheduleId { get; set; }
		public string restNum { get; set; }
		public string hosRegType { get; set; }
    }

    public partial class 排班信息
    {
		public string medDate { get; set; }
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string parentDeptCode { get; set; }
		public string parentDeptName { get; set; }
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string doctTech { get; set; }
		public string medAmPm { get; set; }
		public string regfee { get; set; }
		public string treatfee { get; set; }
		public string regAmount { get; set; }
		public string scheduleId { get; set; }
		public string restNum { get; set; }
		public string hosRegType { get; set; }
    }

    public partial class 号源明细
    {
		public string appoNo { get; set; }
		public string restNum { get; set; }
		public string medBegTime { get; set; }
		public string medEndTime { get; set; }
    }

    public partial class 锁号结果
    {
		public string seqno { get; set; }
		public string lockid { get; set; }
    }

    public partial class 当天挂号结果
    {
		public string orderNo { get; set; }
		public string patientId { get; set; }
		public string deptName { get; set; }
		public string parentDeptName { get; set; }
		public string doctName { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
		public string medDate { get; set; }
		public string address { get; set; }
		public string appoNo { get; set; }
		public string selfFee { get; set; }
		public string insurFee { get; set; }
		public string insurFeeInfo { get; set; }
		public string visitNo { get; set; }
		public string transNo { get; set; }
		public string receiptNo { get; set; }
		public string oppatNo { get; set; }
    }

    public partial class 预约挂号结果
    {
		public string orderNo { get; set; }
		public string patientId { get; set; }
		public string deptName { get; set; }
		public string parentDeptName { get; set; }
		public string doctName { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
		public string medDate { get; set; }
		public string address { get; set; }
		public string appoNo { get; set; }
		public string selfFee { get; set; }
		public string insurFee { get; set; }
		public string insurFeeInfo { get; set; }
    }

    public partial class 取号结果
    {
		public string orderNo { get; set; }
		public string patientId { get; set; }
		public string deptName { get; set; }
		public string parentDeptName { get; set; }
		public string doctName { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
		public string medDate { get; set; }
		public string address { get; set; }
		public string appoNo { get; set; }
		public string selfFee { get; set; }
		public string insurFee { get; set; }
		public string insurFeeInfo { get; set; }
    }

    public partial class 挂号预约记录
    {
		public string regNo { get; set; }
		public string tradeTime { get; set; }
		public string medDate { get; set; }
		public string medTime { get; set; }
		public string hospCode { get; set; }
		public string hospName { get; set; }
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string appoNo { get; set; }
		public string scheduleId { get; set; }
		public string medAmPm { get; set; }
		public string address { get; set; }
		public string appoFrom { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
		public string flowId { get; set; }
    }

    public partial class 取号预结算结果
    {
		public string orderNo { get; set; }
		public string patientId { get; set; }
		public string deptName { get; set; }
		public string doctName { get; set; }
		public string balanceNo { get; set; }
		public string hospName { get; set; }
		public string billNo { get; set; }
		public string queueNo { get; set; }
		public string regDate { get; set; }
		public string address { get; set; }
		public string regNo { get; set; }
		public string typeName { get; set; }
		public string amPm { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string regAmount { get; set; }
    }

    public partial class 取号结算结果
    {
		public string orderNo { get; set; }
		public string address { get; set; }
		public string medAmPm { get; set; }
		public string regNo { get; set; }
		public string medDate { get; set; }
		public string queueNo { get; set; }
		public string billNo { get; set; }
		public string balanceNo { get; set; }
		public string doctName { get; set; }
		public string deptName { get; set; }
		public string regId { get; set; }
		public string regFee { get; set; }
		public string treatFee { get; set; }
		public string visitNo { get; set; }
		public string regAmount { get; set; }
		public string transNo { get; set; }
		public string receiptNo { get; set; }
		public string oppatNo { get; set; }
    }

    public partial class 住院患者信息
    {
		public string status { get; set; }
		public string createDate { get; set; }
		public string outDate { get; set; }
		public string hosNo { get; set; }
		public string patientHosNo { get; set; }
		public string patientHosId { get; set; }
		public string deptName { get; set; }
		public string name { get; set; }
		public string sex { get; set; }
		public string nation { get; set; }
		public string birthday { get; set; }
		public string idNo { get; set; }
		public string cardNo { get; set; }
		public string guardianNo { get; set; }
		public string address { get; set; }
		public string phone { get; set; }
		public string patientType { get; set; }
		public string accountNo { get; set; }
		public string area { get; set; }
		public string bedNo { get; set; }
		public string visitId { get; set; }
		public string patientAccBalance { get; set; }
		public string accBalance { get; set; }
		public string balance { get; set; }
		public string patientId { get; set; }
		public string cost { get; set; }
		public string securityBalance { get; set; }
    }

    public partial class 住院患者费用明细
    {
		public string tradeTime { get; set; }
		public string itemName { get; set; }
		public string itemSpecs { get; set; }
		public string itemLiquid { get; set; }
		public string itemUnits { get; set; }
		public string itemQty { get; set; }
		public string itemPrice { get; set; }
		public string cost { get; set; }
		public string visited { get; set; }
    }

    public partial class 住院充值结果
    {
		public string receiptNo { get; set; }
		public string cash { get; set; }
		public string visitId { get; set; }
    }

    public partial class 住院充值记录
    {
		public string receiptNo { get; set; }
		public string tradeMode { get; set; }
		public string tradeTime { get; set; }
		public string cash { get; set; }
		public string visitId { get; set; }
		public string operId { get; set; }
    }

    public partial class 虚拟账户开通结果
    {
		public string accountNo { get; set; }
		public string accountId { get; set; }
    }

    public partial class 检验基本信息
    {
		public string reportId { get; set; }
		public string patientId { get; set; }
		public string cardNo { get; set; }
		public string inhospId { get; set; }
		public string examType { get; set; }
		public string checkPart { get; set; }
		public string checkDoc { get; set; }
		public string sendTime { get; set; }
		public string resultTime { get; set; }
		public string patientName { get; set; }
		public string age { get; set; }
		public string sex { get; set; }
		public string inspectDoctor { get; set; }
		public string bedNo { get; set; }
		public string remark { get; set; }
		public string type { get; set; }
		public string auditDoc { get; set; }
		public string auditTime { get; set; }
		public string printTimes { get; set; }
		public string sendDoct { get; set; }
		public string receiveTime { get; set; }
		public string receiveDoct { get; set; }
		public string examResult { get; set; }
		public string sampleType { get; set; }
		public string checkCode { get; set; }
		public List<检验项目> examItem { get; set; }
    }

    public partial class 检验项目
    {
		public string itemName { get; set; }
		public string itemRefRange { get; set; }
		public string itemRealValue { get; set; }
		public string itemUnits { get; set; }
		public string itemAbbr { get; set; }
		public string itemMark { get; set; }
		public string quaResult { get; set; }
		public string reportId { get; set; }
		public string showIndex { get; set; }
		public string extend { get; set; }
		public string checkPart { get; set; }
		public string checkDoc { get; set; }
		public string checkTime { get; set; }
		public string auditDoc { get; set; }
		public string auditTime { get; set; }
		public string checkMethod { get; set; }
    }

    public partial class 检验结果明细结果
    {
		public string totalCount { get; set; }
		public List<检验结果明细> examDetail { get; set; }
    }

    public partial class 检验结果明细
    {
		public string hospCode { get; set; }
		public string examId { get; set; }
		public string testNo { get; set; }
		public string testName { get; set; }
		public string testResult { get; set; }
		public string unit { get; set; }
		public string referRanges { get; set; }
		public string testValue { get; set; }
		public string showIndex { get; set; }
		public string testMethod { get; set; }
		public string sampleType { get; set; }
		public string sampleName { get; set; }
		public string remark { get; set; }
    }

    public partial class 检查结果
    {
		public string cardNo { get; set; }
		public string inspecDoctCode { get; set; }
		public string inspecDoctName { get; set; }
		public string inspecTime { get; set; }
		public string checkDate { get; set; }
		public string auditDoctCode { get; set; }
		public string auditDoctName { get; set; }
		public string checkStatus { get; set; }
		public string checkDesc { get; set; }
		public string checkResult { get; set; }
		public string auditDate { get; set; }
		public string checkNo { get; set; }
		public string patientNo { get; set; }
		public string wardName { get; set; }
		public string wardBed { get; set; }
		public string sex { get; set; }
		public string age { get; set; }
		public string inspecDeptName { get; set; }
    }

    public partial class 医生信息
    {
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string doctProfe { get; set; }
		public string doctIntro { get; set; }
		public string doctSpec { get; set; }
    }

    public partial class 科室信息
    {
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string deptIntro { get; set; }
    }

    public partial class 民生卡开卡结果
    {
		public string CertType { get; set; }
		public string CertNum { get; set; }
		public string CustName { get; set; }
		public string Sex { get; set; }
		public string DocNum { get; set; }
		public string M1key { get; set; }
		public string FormalCard { get; set; }
    }

    public partial class 民生卡终端签到结果
    {
		public string RspCode { get; set; }
		public string RspMsg { get; set; }
		public string MacKey { get; set; }
		public string PinKey { get; set; }
		public string MacChk { get; set; }
		public string PinChk { get; set; }
    }

    public partial class 民生卡余额查询结果
    {
		public string AccountBal { get; set; }
    }

    public partial class 民生卡交易明细查询结果
    {
		public string tradeMode { get; set; }
		public string accountNo { get; set; }
		public string cash { get; set; }
		public string receiptNo { get; set; }
		public string tradeTime { get; set; }
		public string operId { get; set; }
    }

    public partial class 民生卡充值结果
    {
		public string AccountBal { get; set; }
    }

    public partial class 民生卡充值冲正结果
    {
    }

    public partial class 民生卡消费结果
    {
    }

    public partial class 民生卡消费冲正结果
    {
    }

    public partial class 银联卡消费登记结果
    {
    }

    public partial class 民生卡退费结果
    {
    }

    public partial class 民生卡工本费
    {
		public string Amt { get; set; }
		public string RspMsg { get; set; }
    }

    public partial class 民生卡客户信息更新
    {
		public string CertType { get; set; }
		public string CertNum { get; set; }
		public string CustName { get; set; }
		public string Sex { get; set; }
		public string DocNum { get; set; }
		public string M1key { get; set; }
    }

    public partial class 民生卡重置密码
    {
    }

    public partial class 民生卡卡片信息查询
    {
		public string CardNo { get; set; }
		public string CardStat { get; set; }
		public string OpenDate { get; set; }
		public string CertType { get; set; }
		public string CertNum { get; set; }
		public string CustName { get; set; }
		public string ParentName { get; set; }
		public string Sex { get; set; }
		public string Nation { get; set; }
		public string PhoneNum { get; set; }
		public string Adrr { get; set; }
		public string BirthDay { get; set; }
    }

    public partial class 挂号退号结果
    {
    }

    public partial class 医保信息查询
    {
		public string stapleFlag { get; set; }
		public string matchType { get; set; }
    }

    public partial class 取消预约或挂号结果
    {
		public string success { get; set; }
    }

    public partial class 民生卡密码修改结果
    {
    }

    public partial class 民生卡CPU卡密码设置结果
    {
    }

    public partial class 医生信息快速查询
    {
		public string doctCode { get; set; }
		public string doctName { get; set; }
		public string deptCode { get; set; }
		public string deptName { get; set; }
		public string doctTech { get; set; }
		public string doctSpec { get; set; }
		public string doctIntro { get; set; }
    }

    public partial class 订单扫码
    {
        /// <summary>
        /// 用户平台支付流水
        /// </summary>
		public string outTradeNo { get; set; }
        /// <summary>
        /// 二维码串
        /// </summary>
		public string qrCode { get; set; }
    }

    public partial class 取消订单
    {
        /// <summary>
        /// 用户平台支付流水
        /// </summary>
		public string outTradeNo { get; set; }
        /// <summary>
        /// 用户平台退款单号
        /// </summary>
		public string outRefundNo { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
		public string fee { get; set; }
    }

    public partial class 订单状态
    {
        /// <summary>
        /// 用户平台支付流水
        /// </summary>
		public string outTradeNo { get; set; }
        /// <summary>
        /// 退款金额
        /// </summary>
		public string fee { get; set; }
        /// <summary>
        /// 101:处理中 200:支付成功 201:支付失败 400:失效订单 500:退款成功 501:退款失败
        /// </summary>
		public string status { get; set; }
		public string outPayNo { get; set; }
		public string outRefundNo { get; set; }
		public string paymentTime { get; set; }
		public string statusDes { get; set; }
    }

    public partial class 支付宝支付成功上报结果
    {
		public string result { get; set; }
    }

    public partial class 系统签到结果
    {
		public string currentTime { get; set; }
		public string currentDate { get; set; }
    }

    public partial class 信息上报结果
    {
		public string currentTime { get; set; }
		public string currentDate { get; set; }
    }

    public partial class 接种针次结果
    {
		public 接种人信息 childInfo { get; set; }
		public List<接种针次列表> scheduleList { get; set; }
    }

    public partial class 接种人信息
    {
		public string name { get; set; }
		public string birthday { get; set; }
		public string sex { get; set; }
		public string institutionName { get; set; }
    }

    public partial class 接种针次列表
    {
		public string vaccineName { get; set; }
		public string stepIndex { get; set; }
		public string estimateDate { get; set; }
    }

    public partial class 接种清单结果
    {
		public 接种人信息 childInfo { get; set; }
		public List<接种清单列表> scheduleList { get; set; }
    }

    public partial class 接种清单列表
    {
		public string vaccineName { get; set; }
		public string stepIndex { get; set; }
		public string estimateDate { get; set; }
		public string isInoc { get; set; }
    }

    public partial class 公费人员信息结果
    {
		public string gfCardNo { get; set; }
		public string doctorValid { get; set; }
		public string veinValid { get; set; }
    }

    public partial class 公费人员指静脉验证结果
    {
		public string flag { get; set; }
		public string sign { get; set; }
		public string description { get; set; }
    }

    public partial class 停车计费查询结果
    {
		public string cash { get; set; }
		public string totalFee { get; set; }
		public string allParkTime { get; set; }
		public string currFavMoney { get; set; }
		public string carNo { get; set; }
		public string entryTime { get; set; }
    }

    public partial class 停车订单生成结果
    {
		public string cash { get; set; }
		public string flowId { get; set; }
		public string transNo { get; set; }
		public string carNo { get; set; }
		public string entryTime { get; set; }
		public string totalFee { get; set; }
    }

    public partial class 停车订单支付结果
    {
		public string cash { get; set; }
		public string totalFee { get; set; }
		public string allParkTime { get; set; }
		public string currFavMoney { get; set; }
		public string carNo { get; set; }
		public string entryTime { get; set; }
    }

    public partial class 病人卡片信息
    {
		public string Rec { get; set; }
		public string CardNo { get; set; }
		public string CardProdId { get; set; }
		public string sBank { get; set; }
		public string CardStat { get; set; }
    }

    public partial class 病人绑卡结果
    {
		public string patientId { get; set; }
		public string cardNo { get; set; }
		public string patientType { get; set; }
		public string insurNo { get; set; }
		public string name { get; set; }
		public string sex { get; set; }
		public string birthday { get; set; }
		public string address { get; set; }
		public string phone { get; set; }
		public string mobile { get; set; }
		public string recordNo { get; set; }
		public string idType { get; set; }
		public string idNo { get; set; }
		public string gfCardNo { get; set; }
    }

}