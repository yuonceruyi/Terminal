using System.Xml.Serialization;

namespace YuanTu.ZheJiangHospital.Component.Appoint
{
    public class Req
    {
        public string funcode { get; set; }

        public virtual string ServiceName => string.Empty;
    }

    [XmlRoot("data")]
    public class Res
    {
        public virtual string ServiceName => string.Empty;
        public int state { get; set; }

        public string result { get; set; }
    }

    #region 科室列表查询

    [XmlRoot("data")]
    public class Req科室列表查询 : Req
    {
        public Req科室列表查询()
        {
            funcode = "100200";
        }

        public override string ServiceName => "科室列表查询";
    }

    [XmlRoot("data")]
    public class Res科室列表查询 : Res
    {
        public override string ServiceName => "科室列表查询";

        [XmlArrayItem("item")]
        public 科室信息[] list { get; set; }
    }

    public class 科室信息
    {
        public string departID { get; set; }

        public string departName { get; set; }

        public string departRemark { get; set; }

        public string deptaddress { get; set; }

        public string stanid { get; set; }
    }

    #endregion 科室列表查询

    #region 科室排班查询

    [XmlRoot("data")]
    public class Req科室排班查询 : Req
    {
        public Req科室排班查询()
        {
            funcode = "100201";
        }

        public override string ServiceName => "科室排班查询";

        /// <summary>
        ///     科室编号
        /// </summary>
        public string deptid { get; set; }
    }

    [XmlRoot("data")]
    public class Res科室排班查询 : Res
    {
        public override string ServiceName => "科室排班查询";

        [XmlArrayItem("item")]
        public 排班信息[] list { get; set; }
    }

    public class 排班信息
    {
        public string schid { get; set; }

        public string orgid { get; set; }

        public string docid { get; set; }

        public string docname { get; set; }

        /// <summary>
        ///     1-男 2-女
        /// </summary>
        public string docsex { get; set; }

        public string title { get; set; }

        public string deptid { get; set; }

        public string deptname { get; set; }

        public string schdate { get; set; }

        /// <summary>
        ///     1-上午 2-下午
        /// </summary>
        public string ampm { get; set; }

        public string numcount { get; set; }

        public string numremain { get; set; }

        /// <summary>
        ///     1-普通 2-专家
        /// </summary>
        public string categor { get; set; }

        public string regfee { get; set; }

        public string fee { get; set; }

        public string schstate { get; set; }

        public string docphoto { get; set; }

        public string docdescription { get; set; }
    }

    #endregion 科室排班查询

    #region 排班号源查询

    [XmlRoot("data")]
    public class Req排班号源查询 : Req
    {
        public Req排班号源查询()
        {
            funcode = "100204";
        }

        public override string ServiceName => "排班号源查询";

        /// <summary>
        ///     排班编号
        /// </summary>
        public string schid { get; set; }
    }

    [XmlRoot("data")]
    public class Res排班号源查询 : Res
    {
        public override string ServiceName => "排班号源查询";

        [XmlArrayItem("item")]
        public 号源信息[] list { get; set; }
    }

    public class 号源信息
    {
        public string numid { get; set; }

        public string numno { get; set; }

        public string numdate { get; set; }

        public string numtime { get; set; }

        public string numendtime { get; set; }

        public string numstate { get; set; }

        public string ontime { get; set; }
    }

    #endregion 排班号源查询

    #region 预约挂号

    [XmlRoot("data")]
    public class Req预约挂号 : Req
    {
        public Req预约挂号()
        {
            funcode = "100208";
        }

        public override string ServiceName => "预约挂号";

        /// <summary>
        ///     号源编号
        /// </summary>
        public string numid { get; set; }

        /// <summary>
        ///     患者姓名
        /// </summary>
        public string patname { get; set; }

        /// <summary>
        ///     患者性别
        /// </summary>
        public string patsex { get; set; }

        /// <summary>
        ///     手机号码
        /// </summary>
        public string mobileno { get; set; }

        /// <summary>
        ///     证件类型
        /// </summary>
        public string idcardtype { get; set; }

        /// <summary>
        ///     身份证号
        /// </summary>
        public string idcard { get; set; }

        /// <summary>
        ///     取号密码
        /// </summary>
        public string pass { get; set; }

        /// <summary>
        ///     就诊序号
        /// </summary>
        public string no { get; set; }

        /// <summary>
        ///     服务商编号
        /// </summary>
        public string spid { get; set; }

        /// <summary>
        ///     服务商名称
        /// </summary>
        public string spname { get; set; }

        /// <summary>
        ///     用户IP地址，导医填工作人员姓名或编号
        /// </summary>
        public string oper { get; set; }

        /// <summary>
        ///     应用编号，由医院信息中心分配
        /// </summary>
        public string appID { get; set; }

        /// <summary>
        ///     时间戳，使用13位时间戳格式
        /// </summary>
        public string time { get; set; }

        /// <summary>
        ///     交易验证串，算法：md5(appID+appKey+time+funcode),appKey为应用密匙，协商约定，可随时申请更换
        /// </summary>
        public string captcha { get; set; }
    }

    [XmlRoot("data")]
    public class Res预约挂号 : Res
    {
        public override string ServiceName => "预约挂号";

        /// <summary>
        ///     预约号
        /// </summary>
        public string orderid { get; set; }
    }

    #endregion 预约挂号

    #region 取消预约

    [XmlRoot("data")]
    public class Req取消预约 : Req
    {
        public Req取消预约()
        {
            funcode = "100209";
        }

        public override string ServiceName => "取消预约";

        /// <summary>
        ///     订单号
        /// </summary>
        public string orderid { get; set; }

        /// <summary>
        ///     取号密码
        /// </summary>
        public string pass { get; set; }

        /// <summary>
        ///     应用编号，由医院信息中心分配
        /// </summary>
        public string appID { get; set; }

        /// <summary>
        ///     时间戳，使用13位时间戳格式
        /// </summary>
        public string time { get; set; }

        /// <summary>
        ///     交易验证串，算法：md5(appID+appKey+time+funcode),appKey为应用密匙，协商约定，可随时申请更换
        /// </summary>
        public string captcha { get; set; }
    }

    [XmlRoot("data")]
    public class Res取消预约 : Res
    {
        public override string ServiceName => "取消预约";
    }

    #endregion 取消预约

    #region 患者就诊信息

    [XmlRoot("data")]
    public class Req患者就诊信息 : Req
    {
        public Req患者就诊信息()
        {
            funcode = "200106";
        }

        public override string ServiceName => "患者就诊信息";

        /// <summary>
        ///     订单号
        /// </summary>
        public string orderid { get; set; }

        /// <summary>
        ///     取号密码
        /// </summary>
        public string pass { get; set; }

        /// <summary>
        ///     应用编号，由医院信息中心分配
        /// </summary>
        public string appID { get; set; }

        /// <summary>
        ///     时间戳，使用13位时间戳格式
        /// </summary>
        public string time { get; set; }

        /// <summary>
        ///     交易验证串，算法：md5(appID+appKey+time+funcode),appKey为应用密匙，协商约定，可随时申请更换
        /// </summary>
        public string captcha { get; set; }
    }

    [XmlRoot("data")]
    public class Res患者就诊信息 : Res
    {
        public override string ServiceName => "患者就诊信息";
    }

    #endregion 患者就诊信息
}