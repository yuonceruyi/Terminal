using System.Collections.Generic;
using System.Xml.Serialization;

namespace YuanTu.XiaoShanZYY.HIS
{
    [XmlType("YIYUANPBXX_IN")]
    public partial class Req医院排班信息 : Req
    {
        [XmlIgnore]
        public override string Service => "YIYUANPBXX";
        ///<summary>
        /// 挂号方式
        /// 1.挂号、2.预约
        ///</summary>
        public string GUAHAOFS { get; set; }
        ///<summary>
        /// 排班类型
        /// 1.一周、2.当天
        ///</summary>
        public string PAIBANLX { get; set; }
        ///<summary>
        /// 排班日期
        /// YYYY-MM-DD
        ///</summary>
        public string PAIBANRQ { get; set; }
        ///<summary>
        /// 挂号班次
        /// 0全部 1上午 2下午
        ///</summary>
        public string GUAHAOBC { get; set; }
        ///<summary>
        /// 挂号类别
        /// 0全部 1普通 2急诊  3专科 >3相关专家类别
        ///</summary>
        public string GUAHAOLB { get; set; }
        ///<summary>
        /// 科室代码
        /// 空，返回全部
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 医生代码
        /// *返回全部
        ///</summary>
        public string YISHENGDM { get; set; }
        ///<summary>
        /// 获取条数
        /// 空或0无效
        ///</summary>
        public string HUOQUROW { get; set; }
    }

    [XmlType("YIYUANPBXX_OUT")]
    public partial class Res医院排班信息 : Res
    {
        [XmlIgnore]
        public override string Service => "YIYUANPBXX";
        ///<summary>
        /// 排班列表
        /// 
        ///</summary>
        public List<PAIBANMX> PAIBANLB { get; set; }
    }

    public partial class PAIBANMX
    {
        ///<summary>
        /// 科室代码
        /// 
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 科室名称
        /// 
        ///</summary>
        public string KESHIMC { get; set; }
        ///<summary>
        /// 就诊地点
        /// 
        ///</summary>
        public string JIUZHENDD { get; set; }
        ///<summary>
        /// 科室介绍
        /// 
        ///</summary>
        public string KESHIJS { get; set; }
        ///<summary>
        /// 医生代码
        /// 普通号用*号代替
        ///</summary>
        public string YISHENGDM { get; set; }
        ///<summary>
        /// 医生姓名
        /// 普通号显示普通
        ///</summary>
        public string YISHENGXM { get; set; }
        ///<summary>
        /// 医生职称
        /// 
        ///</summary>
        public string YISHENGZC { get; set; }
        ///<summary>
        /// 医生特长
        /// 
        ///</summary>
        public string YISHENGTC { get; set; }
        ///<summary>
        /// 医生介绍
        /// 
        ///</summary>
        public string YISHENGJS { get; set; }
        ///<summary>
        /// 上午号源总数
        /// 已挂人数
        ///</summary>
        public int SHANGWUHYZS { get; set; }
        ///<summary>
        /// 上午号源剩余
        /// 剩余可挂人数
        ///</summary>
        public int SHANGWUHYSYS { get; set; }
        ///<summary>
        /// 下午号源总数
        /// 已挂人数
        ///</summary>
        public int XIAWUHYZS { get; set; }
        ///<summary>
        /// 下午号源剩余
        /// 剩余可挂人数
        ///</summary>
        public int XIAWUHYSYS { get; set; }
        ///<summary>
        /// 诊疗加收费
        /// 
        ///</summary>
        public string ZHENLIAOJSF { get; set; }
        ///<summary>
        /// 诊疗费
        /// 
        ///</summary>
        public string ZHENLIAOF { get; set; }
        ///<summary>
        /// 排班日期
        /// YYYY-MM-DD
        ///</summary>
        public string PAIBANRQ { get; set; }
        ///<summary>
        /// 挂号班次
        /// 0全部 1上午 2 下午
        ///</summary>
        public string GUAHAOBC { get; set; }
        ///<summary>
        /// 挂号类别
        /// 0全部 1普通 2急诊  3专科 >3相关专家类别
        ///</summary>
        public string GUAHAOLB { get; set; }
    }

    [XmlType("GUAHAOYSXX_IN")]
    public partial class Req挂号医生信息 : Req
    {
        [XmlIgnore]
        public override string Service => "GUAHAOYSXX";
        ///<summary>
        /// 挂号方式
        /// 1. 挂号、2. 预约
        ///</summary>
        public string GUAHAOFS { get; set; }
        ///<summary>
        /// 日期
        /// YYYY-MM-DD
        ///</summary>
        public string RIQI { get; set; }
        ///<summary>
        /// 挂号班次
        /// 0全部 1上午 2 下午
        ///</summary>
        public string GUAHAOBC { get; set; }
        ///<summary>
        /// 科室代码
        /// 空，返回全部
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 挂号类别
        /// 0全部 1普通 2急诊  3专科 >3相关专家类别
        ///</summary>
        public string GUAHAOLB { get; set; }
    }

    [XmlType("GUAHAOYSXX_OUT")]
    public partial class Res挂号医生信息 : Res
    {
        [XmlIgnore]
        public override string Service => "GUAHAOYSXX";
        ///<summary>
        /// 医生明细
        /// 
        ///</summary>
        public List<YISHENGXX> YISHENGMX { get; set; }
    }

    public partial class YISHENGXX
    {
        ///<summary>
        /// 医生代码
        /// 普通号用*号代替
        ///</summary>
        public string YISHENGDM { get; set; }
        ///<summary>
        /// 医生姓名
        /// 普通号显示普通
        ///</summary>
        public string YISHENGXM { get; set; }
        ///<summary>
        /// 科室代码
        /// 
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 科室名称
        /// 
        ///</summary>
        public string KESHIMC { get; set; }
        ///<summary>
        /// 医生职称
        /// 
        ///</summary>
        public string YISHENGZC { get; set; }
        ///<summary>
        /// 医生特长
        /// 调整为已挂人数
        ///</summary>
        public string YISHENGTC { get; set; }
        ///<summary>
        /// 医生介绍
        /// 调整为剩余可挂人数
        ///</summary>
        public string YISHENGJS { get; set; }
    }

    [XmlType("GUAHAOHYXX_IN")]
    public partial class Req挂号号源信息 : Req
    {
        [XmlIgnore]
        public override string Service => "GUAHAOHYXX";
        ///<summary>
        /// 挂号方式
        /// 1. 挂号、2. 预约
        ///</summary>
        public string GUAHAOFS { get; set; }
        ///<summary>
        /// 日期
        /// YYYY-MM-DD
        ///</summary>
        public string RIQI { get; set; }
        ///<summary>
        /// 挂号班次
        /// 0全部 1上午 2 下午
        ///</summary>
        public string GUAHAOBC { get; set; }
        ///<summary>
        /// 科室代码
        /// 
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 医生代码
        /// 
        ///</summary>
        public string YISHENGDM { get; set; }
    }

    [XmlType("GUAHAOHYXX_OUT")]
    public partial class Res挂号号源信息 : Res
    {
        [XmlIgnore]
        public override string Service => "GUAHAOHYXX";
        ///<summary>
        /// 号源明细
        /// 
        ///</summary>
        public List<HAOYUANXX> HAOYUANMX { get; set; }
    }

    public partial class HAOYUANXX
    {
        ///<summary>
        /// 日期
        /// 
        ///</summary>
        public string RIQI { get; set; }
        ///<summary>
        /// 挂号班次
        /// 0全部 1上午 2 下午
        ///</summary>
        public string GUAHAOBC { get; set; }
        ///<summary>
        /// 挂号类别
        /// 0全部 1普通 2急诊  3专科 >3相关专家类别
        ///</summary>
        public string GUAHAOLB { get; set; }
        ///<summary>
        /// 科室代码
        /// 
        ///</summary>
        public string KESHIDM { get; set; }
        ///<summary>
        /// 医生代码
        /// 
        ///</summary>
        public string YISHENGDM { get; set; }
        ///<summary>
        /// 挂号序号
        /// 
        ///</summary>
        public string GUAHAOXH { get; set; }
        ///<summary>
        /// 就诊时间
        /// 
        ///</summary>
        public string JIUZHENSJ { get; set; }
        ///<summary>
        /// 一周排班ID
        /// 
        ///</summary>
        public string YIZHOUPBID { get; set; }
        ///<summary>
        /// 当天排班ID
        /// 
        ///</summary>
        public string DANGTIANPBID { get; set; }
    }

    [XmlType("MENZHENFYMX_IN")]
    public partial class Req门诊费用明细 : Req
    {
        [XmlIgnore]
        public override string Service => "MENZHENFYMX";
        ///<summary>
        /// 就诊卡类型
        /// 可空
        ///</summary>
        public string JIUZHENKLX { get; set; }
        ///<summary>
        /// 就诊卡号
        /// 非空
        ///</summary>
        public string JIUZHENKH { get; set; }
        ///<summary>
        /// 病人类别
        /// 可空
        ///</summary>
        public string BINGRENLB { get; set; }
        ///<summary>
        /// 病人性质
        /// 可空
        ///</summary>
        public string BINGRENXZ { get; set; }
        ///<summary>
        /// 医保卡类型
        /// 可空
        ///</summary>
        public string YIBAOKLX { get; set; }
        ///<summary>
        /// 医保卡号
        /// 可空
        ///</summary>
        public string YIBAOKH { get; set; }
        ///<summary>
        /// 医保卡密码
        /// 可空
        ///</summary>
        public string YIBAOKMM { get; set; }
        ///<summary>
        /// 医保卡信息
        /// 可空
        ///</summary>
        public string YIBAOKXX { get; set; }
        ///<summary>
        /// 医保病人信息
        /// 可空
        ///</summary>
        public string YIBAOBRXX { get; set; }
        ///<summary>
        /// 医疗类别
        /// 可空
        ///</summary>
        public string YILIAOLB { get; set; }
        ///<summary>
        /// 结算类别
        /// 可空
        ///</summary>
        public string JIESUANLB { get; set; }
        ///<summary>
        /// HIS 病人信息
        /// 可空
        ///</summary>
        public string HISBRXX { get; set; }
    }

    [XmlType("MENZHENFYMX_OUT")]
    public partial class Res门诊费用明细 : Res
    {
        [XmlIgnore]
        public override string Service => "MENZHENFYMX";
        ///<summary>
        /// 医疗类别
        /// 
        ///</summary>
        public string YILIAOLB { get; set; }
        ///<summary>
        /// 疾病明细
        /// 
        ///</summary>
        public List<JIBINGXX> JIBINGMX { get; set; }
        ///<summary>
        /// 费用明细条数
        /// 
        ///</summary>
        public string FEIYONGMXTS { get; set; }
        ///<summary>
        /// 费用明细
        /// 
        ///</summary>
        public List<MENZHENFYXX> FEIYONGMX { get; set; }
    }

    public partial class JIBINGXX
    {
        ///<summary>
        /// 疾病代码
        /// 
        ///</summary>
        public string JIBINGDM { get; set; }
        ///<summary>
        /// 疾病ICD
        /// 
        ///</summary>
        public string JIBINGICD { get; set; }
        ///<summary>
        /// 疾病名称
        /// 
        ///</summary>
        public string JIBINGMC { get; set; }
        ///<summary>
        /// 疾病描述
        /// 
        ///</summary>
        public string JIBINGMS { get; set; }
    }

    public partial class MENZHENFYXX
    {
        ///<summary>
        /// 处方类型
        /// 0.医技 1.处方
        ///</summary>
        public string CHUFANGLX { get; set; }
        ///<summary>
        /// 处方序号
        /// 
        ///</summary>
        public string CHUFANGXH { get; set; }
        ///<summary>
        /// 明细序号
        /// 
        ///</summary>
        public string MINGXIXH { get; set; }
        ///<summary>
        /// 费用类型
        /// 
        ///</summary>
        public string FEIYONGLX { get; set; }
        ///<summary>
        /// 项目序号
        /// 
        ///</summary>
        public string XIANGMUXH { get; set; }
        ///<summary>
        /// 项目产地代码
        /// 
        ///</summary>
        public string XIANGMUCDDM { get; set; }
        ///<summary>
        /// 项目名称
        /// 
        ///</summary>
        public string XIANGMUMC { get; set; }
        ///<summary>
        /// 项目归类
        /// 
        ///</summary>
        public string XIANGMUGL { get; set; }
        ///<summary>
        /// 项目归类名称
        /// 
        ///</summary>
        public string XIANGMUGLMC { get; set; }
        ///<summary>
        /// 项目规格
        /// 
        ///</summary>
        public string XIANGMUGG { get; set; }
        ///<summary>
        /// 项目剂型
        /// 
        ///</summary>
        public string XIANGMUJX { get; set; }
        ///<summary>
        /// 项目单位
        /// 
        ///</summary>
        public string XIANGMUDW { get; set; }
        ///<summary>
        /// 项目产地名称
        /// 
        ///</summary>
        public string XIANGMUCDMC { get; set; }
        ///<summary>
        /// 中草药贴数
        /// 
        ///</summary>
        public string ZHONGCAOYTS { get; set; }
        ///<summary>
        /// 单价
        /// 
        ///</summary>
        public string DANJIA { get; set; }
        ///<summary>
        /// 数量
        /// 
        ///</summary>
        public string SHULIANG { get; set; }
        ///<summary>
        /// 金额
        /// 
        ///</summary>
        public string JINE { get; set; }
        ///<summary>
        /// 开单科室代码
        /// 
        ///</summary>
        public string KAIDANKSDM { get; set; }
        ///<summary>
        /// 开单科室名称
        /// 
        ///</summary>
        public string KAIDANKSMC { get; set; }
        ///<summary>
        /// 开单医生代码
        /// 
        ///</summary>
        public string KAIDANYSDM { get; set; }
        ///<summary>
        /// 开单医生姓名
        /// 
        ///</summary>
        public string KAIDANYSXM { get; set; }
    }

    [XmlType("CLINICORDERD_IN")]
    public partial class Req预约挂号处理 : Req
    {
        [XmlIgnore]
        public override string Service => "CLINICORDERD";
        ///<summary>
        /// 病人ID
        /// 
        ///</summary>
        public string PATIENTID { get; set; }
        ///<summary>
        /// 操作工号
        /// 
        ///</summary>
        public string OPERATOR { get; set; }
        ///<summary>
        /// 科室代码
        /// 
        ///</summary>
        public string REGDEPTID { get; set; }
        ///<summary>
        /// 医生代码
        /// 
        ///</summary>
        public string DOCTORID { get; set; }
        ///<summary>
        /// 预约日期
        /// 
        ///</summary>
        public string ORDERDATE { get; set; }
        ///<summary>
        /// 挂号类别
        /// 0全部 1普通 2急诊  3专科 >3相关专家类别
        ///</summary>
        public string CLINICTTYPE { get; set; }
        ///<summary>
        /// 挂号班次
        /// 1上午 2 下午
        ///</summary>
        public string DUTYTYPE { get; set; }
        ///<summary>
        /// 挂号序号
        /// 传0 则HIS 分配
        ///</summary>
        public string SEQUENCENUM { get; set; }
        ///<summary>
        /// 手机号
        /// 
        ///</summary>
        public string TELNO { get; set; }
    }

    [XmlType("CLINICORDERD_OUT")]
    public partial class Res预约挂号处理 : Res
    {
        [XmlIgnore]
        public override string Service => "CLINICORDERD";
        ///<summary>
        /// 取号密码
        /// 
        ///</summary>
        public string ORDERNUM { get; set; }
        ///<summary>
        /// 就诊时间
        /// 
        ///</summary>
        public string CLINICTTIME { get; set; }
        ///<summary>
        /// 挂号序号
        /// 
        ///</summary>
        public string SEQUENCENUM { get; set; }
    }

    [XmlType("GUAHAOYYTH_IN")]
    public partial class Req预约退号处理 : Req
    {
        [XmlIgnore]
        public override string Service => "GUAHAOYYTH";
        ///<summary>
        /// 就诊卡类型
        /// 可空
        ///</summary>
        public string JIUZHENKLX { get; set; }
        ///<summary>
        /// 就诊卡号
        /// 可空
        ///</summary>
        public string JIUZHENKH { get; set; }
        ///<summary>
        /// 证件类型
        /// 可空
        ///</summary>
        public string ZHENGJIANLX { get; set; }
        ///<summary>
        /// 证件号码
        /// 可空
        ///</summary>
        public string ZHENGJIANHM { get; set; }
        ///<summary>
        /// 姓名
        /// 可空
        ///</summary>
        public string XINGMING { get; set; }
        ///<summary>
        /// 预约来源
        /// 可空
        ///</summary>
        public string YUYUELY { get; set; }
        ///<summary>
        /// 取号密码
        /// 非空
        ///</summary>
        public string QUHAOMM { get; set; }
    }

    [XmlType("GUAHAOYYTH_OUT")]
    public partial class Res预约退号处理 : Res
    {
        [XmlIgnore]
        public override string Service => "GUAHAOYYTH";
    }

    [XmlType("DANGANXXCL_IN")]
    public partial class Req档案信息处理 : Req
    {
        [XmlIgnore]
        public override string Service => "DANGANXXCL";
        ///<summary>
        /// 就诊卡号
        /// 病人唯一标识，非空
        ///</summary>
        public string JIUZHENKH { get; set; }
        ///<summary>
        /// 姓名
        /// 非空
        ///</summary>
        public string XINGMING { get; set; }
        ///<summary>
        /// 联系电话
        /// 非空
        ///</summary>
        public string LIANXIDIANHUA { get; set; }
    }

    [XmlType("DANGANXXCL_OUT")]
    public partial class Res档案信息处理 : Res
    {
        [XmlIgnore]
        public override string Service => "DANGANXXCL";
    }

    [XmlType("QueryFeeRecord_IN")]
    public partial class Req缴费结算查询 : Req
    {
        [XmlIgnore]
        public override string Service => "QueryFeeRecord";
        ///<summary>
        /// 就诊卡号
        /// 非空
        ///</summary>
        public string cardNo { get; set; }
        ///<summary>
        /// 就诊卡类型
        /// 可空
        ///</summary>
        public string cardType { get; set; }
        ///<summary>
        /// 开始时间 默认取最近一周
        /// 可空
        ///</summary>
        public string startDate { get; set; }
        ///<summary>
        /// 结束时间 默认取最近一周
        /// 可空
        ///</summary>
        public string endDate { get; set; }
        ///<summary>
        /// 扩展字段
        /// 可空
        ///</summary>
        public string extend { get; set; }
    }

    [XmlType("QueryFeeRecord_OUT")]
    public partial class Res缴费结算查询 : Res
    {
        [XmlIgnore]
        public override string Service => "QueryFeeRecord";
        ///<summary>
        /// 单据明细条数
        /// 
        ///</summary>
        public string BILLMXTS { get; set; }
        ///<summary>
        /// 单据明细
        /// 
        ///</summary>
        [XmlArrayItem("billdetial")]
        public List<billdetail> BILLMX { get; set; }
    }

    public partial class billdetail
    {
        ///<summary>
        /// 单据流水
        /// 
        ///</summary>
        public string billNo { get; set; }
        ///<summary>
        /// 交易时间
        /// 
        ///</summary>
        public string tradeTime { get; set; }
        ///<summary>
        /// 收据(发票)号
        /// 
        ///</summary>
        public string receiptNo { get; set; }
        ///<summary>
        /// 单据总金额
        /// 
        ///</summary>
        public string billFee { get; set; }
        ///<summary>
        /// 费用类型
        /// 
        ///</summary>
        public string billType { get; set; }
        ///<summary>
        /// 自费金额
        /// 
        ///</summary>
        public string selfFee { get; set; }
        ///<summary>
        /// 医保金额
        /// 
        ///</summary>
        public string insurFee { get; set; }
        ///<summary>
        /// 医保信息
        /// 
        ///</summary>
        public string insurFeeInfo { get; set; }
        ///<summary>
        /// 优惠金额
        /// 
        ///</summary>
        public string discountFee { get; set; }
        ///<summary>
        /// 支付方式
        /// 
        ///</summary>
        public string tradeMode { get; set; }
        ///<summary>
        /// 操作员 
        /// 
        ///</summary>
        public string operId { get; set; }
    }

    [XmlType("QueryReprint_IN")]
    public partial class Req补打查询 : Req
    {
        [XmlIgnore]
        public override string Service => "QueryReprint";
        ///<summary>
        /// 就诊卡号
        /// 非空
        ///</summary>
        public string cardNo { get; set; }
        ///<summary>
        /// 就诊卡类型
        /// 可空
        ///</summary>
        public string cardType { get; set; }
        ///<summary>
        /// 开始时间 默认取最近一周
        /// 可空
        ///</summary>
        public string startDate { get; set; }
        ///<summary>
        /// 结束时间 默认取最近一周
        /// 可空
        ///</summary>
        public string endDate { get; set; }
        ///<summary>
        /// 扩展字段
        /// 可空
        ///</summary>
        public string extend { get; set; }
    }

    [XmlType("QueryReprint_OUT")]
    public partial class Res补打查询 : Res
    {
        [XmlIgnore]
        public override string Service => "QueryReprint";
        ///<summary>
        /// 单据明细条数
        /// 
        ///</summary>
        public string BILLMXTS { get; set; }
        ///<summary>
        /// 单据明细
        /// 
        ///</summary>
        [XmlArrayItem("billdetial")]
        public List<billdetail2> BILLMX { get; set; }
    }

    public partial class billdetail2
    {
        ///<summary>
        /// 提示信息
        /// 
        ///</summary>
        public string Tsxx { get; set; }
        ///<summary>
        /// 结算类型
        /// 
        ///</summary>
        public string Jslx { get; set; }
        ///<summary>
        /// 结算时间
        /// 
        ///</summary>
        public string Jsrq { get; set; }
        ///<summary>
        /// 发票号码
        /// 
        ///</summary>
        public string Fphm { get; set; }
        ///<summary>
        /// 病人姓名
        /// 
        ///</summary>
        public string Brxm { get; set; }
        ///<summary>
        /// 总计金额
        /// 
        ///</summary>
        public string Zjje { get; set; }
        ///<summary>
        /// 医保报销
        /// 
        ///</summary>
        public string Ybbx { get; set; }
        ///<summary>
        /// 市民卡支付
        /// 
        ///</summary>
        public string Smkje { get; set; }
        ///<summary>
        /// 本年余额
        /// 
        ///</summary>
        public string Bnzhye { get; set; }
        ///<summary>
        /// 历年账户余额
        /// 
        ///</summary>
        public string Lnzhye { get; set; }
        ///<summary>
        /// 移动支付
        /// 
        ///</summary>
        public string Thirdcash { get; set; }
        ///<summary>
        /// 备注信息
        /// 
        ///</summary>
        public string Bzxx { get; set; }
        ///<summary>
        /// 取药窗口
        /// 
        ///</summary>
        public string Qyck { get; set; }
        ///<summary>
        /// 挂号科室名称
        /// 
        ///</summary>
        public string Ghks { get; set; }
        ///<summary>
        /// 医生姓名
        /// 
        ///</summary>
        public string Ghys { get; set; }
        ///<summary>
        /// 就诊序号
        /// 
        ///</summary>
        public string Jzxh { get; set; }
        ///<summary>
        /// 科室位置
        /// 
        ///</summary>
        public string Kswz { get; set; }
    }

    public partial class ReqDll
    {
        ///<summary>
        /// 识别结算病人的唯一标识
        /// 
        ///</summary>
        public string 卡号 { get; set; }
        ///<summary>
        /// 00 病人信息查询建档 01 门诊挂号 02 门诊取号03 门诊收费 04 计算LIS试管费
        /// 
        ///</summary>
        public string 结算类型 { get; set; }
        ///<summary>
        /// 传入空字符串
        /// 
        ///</summary>
        public string 调用接口ID { get; set; }
        ///<summary>
        /// 传入空字符串
        /// 
        ///</summary>
        public string 调用类型 { get; set; }
        ///<summary>
        /// 15、省医保55、省异地56、市异地84、市医保88、萧山医保100、健康卡  
        /// 
        ///</summary>
        public string 病人类别 { get; set; }
        ///<summary>
        /// 1、预结算 2、结算
        /// 
        ///</summary>
        public string 结算方式 { get; set; }
        ///<summary>
        /// 预结算获取返回应付金额，结算时再次传入做校验，结算方式 = 2时必传，否则无效
        /// 
        ///</summary>
        public string 应付金额 { get; set; }
        ///<summary>
        /// 指定是否是结算本次就诊费用，不传则结算所有可结算费用
        /// 
        ///</summary>
        public string 就诊序号 { get; set; }
        ///<summary>
        /// 传入空字符串无效
        /// 
        ///</summary>
        public string 操作工号 { get; set; }
        ///<summary>
        /// 传入空字符串无效
        /// 
        ///</summary>
        public string 系统序号 { get; set; }
        ///<summary>
        /// 传入空字符串无效
        /// 
        ///</summary>
        public string 收费类别 { get; set; }
        ///<summary>
        /// 挂号交易时不能为空
        /// 
        ///</summary>
        public string 科室代码 { get; set; }
        ///<summary>
        /// 专家挂号交易时不能为空
        /// 
        ///</summary>
        public string 医生代码 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 诊疗费_加收 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 诊疗费 { get; set; }
        ///<summary>
        /// 挂号交易时不能为空 1、普通门诊 2、急诊3、专家门诊
        /// 
        ///</summary>
        public string 挂号类别 { get; set; }
        ///<summary>
        /// 挂号交易时不能为空 1、上午 2、下午
        /// 
        ///</summary>
        public string 排班类别 { get; set; }
        ///<summary>
        /// 取号交易时不能为空
        /// 
        ///</summary>
        public string 取号密码 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 挂号日期 { get; set; }
        ///<summary>
        /// 结算时间 yyyy.mm.dd hh:mm:ss
        /// 
        ///</summary>
        public string 支付时间 { get; set; }
        ///<summary>
        /// 25 微信 26 支付宝 27 银行卡
        /// 
        ///</summary>
        public string 支付方式 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 结算流水号 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 支付流水号 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 支付金额 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 设备编码 { get; set; }
        public override string ToString()
        {
            return string.Join("#", 卡号, 结算类型, 调用接口ID, 调用类型, 病人类别, 结算方式, 应付金额, 就诊序号, 操作工号, 系统序号, 收费类别, 科室代码, 医生代码, 诊疗费_加收, 诊疗费, 挂号类别, 排班类别, 取号密码, 挂号日期, 支付时间, 支付方式, 结算流水号, 支付流水号, 支付金额, 设备编码);
        }
    }

    public partial class Res病人信息查询建档 : DllRes
    {
        ///<summary>
        /// 00、成功01、取消结算-1、失败
        /// 
        ///</summary>
        public string 结算结果 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 病人姓名 { get; set; }
        ///<summary>
        /// 病人唯一标识
        /// 
        ///</summary>
        public string 就诊卡号 { get; set; }
        ///<summary>
        /// 1、男 2、女
        /// 
        ///</summary>
        public string 病人性别 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 身份证号 { get; set; }
        ///<summary>
        /// 格式（YYYY.MM.DD）
        /// 
        ///</summary>
        public string 出生日期 { get; set; }
        ///<summary>
        /// 15、省医保55、省异地56、市异地84、市医保88、萧山医保100、健康卡  
        /// 
        ///</summary>
        public string 病人类别 { get; set; }
        ///<summary>
        /// 医保当年账户余额，格式（0.00）
        /// 
        ///</summary>
        public string 当年账户余额 { get; set; }
        ///<summary>
        /// 医保历年账户余额，格式（0.00）
        /// 
        ///</summary>
        public string 历年账户余额 { get; set; }
        ///<summary>
        /// 市民卡账户余额，格式（0.00）
        /// 
        ///</summary>
        public string 市民卡余额 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 联系电话 { get; set; }

        public override bool Parse(string s)
        {
            var list = s.Split('|');
            结算结果 = list[0];
            病人姓名 = list[1];
            就诊卡号 = list[2];
            病人性别 = list[3];
            身份证号 = list[4];
            出生日期 = list[5];
            病人类别 = list[6];
            当年账户余额 = list[7];
            历年账户余额 = list[8];
            市民卡余额 = list[9];
            联系电话 = list[10];
            return true;
        }
    }

    public partial class Res挂号取号 : DllRes
    {
        ///<summary>
        /// 00、成功01、取消结算02、余额不足-1、失败
        /// 
        ///</summary>
        public string 结算结果 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 病人姓名 { get; set; }
        ///<summary>
        /// 病人唯一标识
        /// 
        ///</summary>
        public string 就诊卡号 { get; set; }
        ///<summary>
        /// 格式（YYYY.MM.DD）
        /// 
        ///</summary>
        public string 挂号日期 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 诊疗费_加收 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 诊疗费 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 医保支付 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 市民卡账户支付 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 惠民减免金额 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 记账金额 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 科室名称 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 科室位置 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 医生名称 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 挂号序号 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 就诊号码 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 候诊时间 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 移动支付 { get; set; }

        public override bool Parse(string s)
        {
            var list = s.Split('|');
            结算结果 = list[0];
            病人姓名 = list[1];
            就诊卡号 = list[2];
            挂号日期 = list[3];
            诊疗费_加收 = list[4];
            诊疗费 = list[5];
            医保支付 = list[6];
            市民卡账户支付 = list[7];
            惠民减免金额 = list[8];
            记账金额 = list[9];
            科室名称 = list[10];
            科室位置 = list[11];
            医生名称 = list[12];
            挂号序号 = list[13];
            就诊号码 = list[14];
            候诊时间 = list[15];
            移动支付 = list[16];
            return true;
        }
    }

    public partial class Res预结算 : DllRes
    {
        ///<summary>
        /// 00、成功01、取消结算02、余额不足-1、失败
        /// 
        ///</summary>
        public string 结算结果 { get; set; }
        ///<summary>
        /// 病人姓名：****
        /// 
        ///</summary>
        public string 病人姓名 { get; set; }
        ///<summary>
        /// 单据总金额：50
        /// 
        ///</summary>
        public string 单据总金额 { get; set; }
        ///<summary>
        /// 医保报销金额：8
        /// 
        ///</summary>
        public string 医保报销金额 { get; set; }
        ///<summary>
        /// 市民卡余额 16
        /// 
        ///</summary>
        public string 市民卡余额 { get; set; }
        ///<summary>
        /// 应付金额：42
        /// 
        ///</summary>
        public string 应付金额 { get; set; }

        public override bool Parse(string s)
        {
            var list = s.Split('|');
            结算结果 = list[0];
            病人姓名 = list[1];
            单据总金额 = list[2];
            医保报销金额 = list[3];
            市民卡余额 = list[4];
            应付金额 = list[5];
            return true;
        }
    }

    public partial class Res结算 : DllRes
    {
        ///<summary>
        /// 00、成功01、取消结算02、余额不足-1、失败
        /// 
        ///</summary>
        public string 结算结果 { get; set; }
        ///<summary>
        /// 病人姓名：****
        /// 
        ///</summary>
        public string 病人姓名 { get; set; }
        ///<summary>
        /// 杭州市-<市民卡医疗支付>结算成功！
        /// 
        ///</summary>
        public string 成功提示信息 { get; set; }
        ///<summary>
        /// 电脑号：33008813
        /// 
        ///</summary>
        public string 电脑号 { get; set; }
        ///<summary>
        /// 单据总金额：50
        /// 
        ///</summary>
        public string 单据总金额 { get; set; }
        ///<summary>
        /// 医保报销金额：8
        /// 
        ///</summary>
        public string 医保报销金额 { get; set; }
        ///<summary>
        /// 医保本年账户余额：0
        /// 
        ///</summary>
        public string 医保本年账户余额 { get; set; }
        ///<summary>
        /// 医保历年账户余额：0
        /// 
        ///</summary>
        public string 医保历年账户余额 { get; set; }
        ///<summary>
        /// 应付金额：42
        /// 
        ///</summary>
        public string 应付金额 { get; set; }
        ///<summary>
        /// 市民卡支付金额：0
        /// 
        ///</summary>
        public string 市民卡支付金额 { get; set; }
        ///<summary>
        /// 市民卡账户余额：0
        /// 
        ///</summary>
        public string 市民卡账户余额 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 移动支付 { get; set; }
        ///<summary>
        /// 结算日期：2014/07/22 14:07:50
        /// 
        ///</summary>
        public string 结算日期 { get; set; }
        ///<summary>
        /// 如需发票请到门诊楼一层11号自助机打印
        /// 
        ///</summary>
        public string 打印发票 { get; set; }
        ///<summary>
        /// 门诊二楼西药房8号窗口取药
        /// 
        ///</summary>
        public string 取药窗口 { get; set; }

        public override bool Parse(string s)
        {
            var list = s.Split('|');
            结算结果 = list[0];
            病人姓名 = list[1];
            成功提示信息 = list[2];
            电脑号 = list[3];
            单据总金额 = list[4];
            医保报销金额 = list[5];
            医保本年账户余额 = list[6];
            医保历年账户余额 = list[7];
            应付金额 = list[8];
            市民卡支付金额 = list[9];
            市民卡账户余额 = list[10];
            移动支付 = list[11];
            结算日期 = list[12];
            打印发票 = list[13];
            取药窗口 = list[14];
            return true;
        }
    }

    public partial class Res计算LIS试管费 : DllRes
    {
        ///<summary>
        /// 00、成功-1、失败
        /// 
        ///</summary>
        public string 结算结果 { get; set; }
        ///<summary>
        /// 
        /// 
        ///</summary>
        public string 病人姓名 { get; set; }
        ///<summary>
        /// 病人唯一标识
        /// 
        ///</summary>
        public string 就诊卡号 { get; set; }
        ///<summary>
        /// 0: 无LIS试管费>0 :有LIS试管费
        /// 
        ///</summary>
        public string 费用明细条数 { get; set; }
        ///<summary>
        /// 格式使用JSON
        /// 
        ///</summary>
        public string 费用明细 { get; set; }

        public override bool Parse(string s)
        {
            var list = s.Split('|');
            结算结果 = list[0];
            病人姓名 = list[1];
            就诊卡号 = list[2];
            费用明细条数 = list[3];
            费用明细 = list[4];
            return true;
        }
    }

    public partial class LIS试管费费用明细
    {
        ///<summary>
        /// 项目名称
        /// 
        ///</summary>
        public string XIANGMUMC { get; set; }
        ///<summary>
        /// 项目归类名称
        /// 
        ///</summary>
        public string XIANGMUGLMC { get; set; }
        ///<summary>
        /// 单价
        /// 
        ///</summary>
        public string DANJIA { get; set; }
        ///<summary>
        /// 数量
        /// 
        ///</summary>
        public string SHULIANG { get; set; }
        ///<summary>
        /// 金额
        /// 
        ///</summary>
        public string JINE { get; set; }
    }

}
// public static Result<Res医院排班信息> 医院排班信息(Req医院排班信息 req)
//{
//      return HISConnection.Handle<Res医院排班信息>(req);
//}
// public static Result<Res挂号医生信息> 挂号医生信息(Req挂号医生信息 req)
//{
//      return HISConnection.Handle<Res挂号医生信息>(req);
//}
// public static Result<Res挂号号源信息> 挂号号源信息(Req挂号号源信息 req)
//{
//      return HISConnection.Handle<Res挂号号源信息>(req);
//}
// public static Result<Res门诊费用明细> 门诊费用明细(Req门诊费用明细 req)
//{
//      return HISConnection.Handle<Res门诊费用明细>(req);
//}
// public static Result<Res预约挂号处理> 预约挂号处理(Req预约挂号处理 req)
//{
//      return HISConnection.Handle<Res预约挂号处理>(req);
//}
// public static Result<Res预约退号处理> 预约退号处理(Req预约退号处理 req)
//{
//      return HISConnection.Handle<Res预约退号处理>(req);
//}
// public static Result<Res档案信息处理> 档案信息处理(Req档案信息处理 req)
//{
//      return HISConnection.Handle<Res档案信息处理>(req);
//}
// public static Result<Res缴费结算查询> 缴费结算查询(Req缴费结算查询 req)
//{
//      return HISConnection.Handle<Res缴费结算查询>(req);
//}
// public static Result<Res补打查询> 补打查询(Req补打查询 req)
//{
//      return HISConnection.Handle<Res补打查询>(req);
//}

//public static Result<Res病人信息查询建档> 病人信息查询建档(ReqDll req)
//{
//    return RunExe<Res病人信息查询建档>(req, 1);
//}
//public static Result<Res挂号取号> 挂号取号(ReqDll req)
//{
//    return RunExe<Res挂号取号>(req, 2);
//}
//public static Result<Res预结算> 预结算(ReqDll req)
//{
//    return RunExe<Res预结算>(req, 3);
//}
//public static Result<Res结算> 结算(ReqDll req)
//{
//    return RunExe<Res结算>(req, 4);
//}
//public static Result<Res计算LIS试管费> 计算LIS试管费(ReqDll req)
//{
//    return RunExe<Res计算LIS试管费>(req, 5);
//}

