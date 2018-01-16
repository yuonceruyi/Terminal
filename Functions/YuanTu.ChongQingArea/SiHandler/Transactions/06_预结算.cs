namespace YuanTu.ChongQingArea.SiHandler
{
    #region 06_预结算

    /// <summary>
    ///     功能描述: 将指定住院(或门诊)号对应的费用进行结算，但不产生结算记录。
    /// </summary>
    public class Req预结算 : Req
    {
        public override string 交易类别代码 => "06";
        public override string 交易类别 => "预结算";

        public override string ToQuery()
        {
            return
                $"{交易类别代码}|{住院号_门诊号}|{截止日期}|{住院床日}|{本次结算总金额}|{账户余额支付标志}|{本次结算明细总条数}|{险种类别}|{工伤认定编号}|{工伤认定疾病编码}|{尘肺结算类型}|{出院科室编码}|{出院医师编码}";
        }

        #region Properties

        /// <summary>
        ///     住院(门诊)号
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 住院号_门诊号 { get; set; }

        /// <summary>
        ///     截止日期
        ///     DATE
        ///     格式：YYYY-MM-DD 为空时表示结算当前时间前所有费用
        /// </summary>
        public string 截止日期 { get; set; }

        /// <summary>
        ///     住院床日
        ///     Number(8,2)
        ///     门诊类型的结算可以为空
        /// </summary>
        public string 住院床日 { get; set; }

        /// <summary>
        ///     本次结算总金额
        ///     Number(10,2)
        ///     非空
        /// </summary>
        public string 本次结算总金额 { get; set; }

        /// <summary>
        ///     账户余额支付标志
        ///     Varchar2(3)
        ///     0、表示用账户余额支付 1、表示可以用现金支付 为空时，默认为用账户余额支付
        /// </summary>
        public string 账户余额支付标志 { get; set; }

        /// <summary>
        ///     本次结算明细总条数
        ///     Number(8)
        ///     非空
        ///     参与此次结算的明细总条数。除开已冲销 的。
        /// </summary>
        public string 本次结算明细总条数 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险；
        /// </summary>
        public string 险种类别 { get; set; }

        /// <summary>
        ///     工伤认定编号
        ///     Varchar2(10)
        /// </summary>
        public string 工伤认定编号 { get; set; }

        /// <summary>
        ///     工伤认定疾病编码
        ///     Varchar2(200)
        ///     工伤认定病种编码 1#工伤认定病种编码 2#工伤认定病种编码 3#…
        /// </summary>
        public string 工伤认定疾病编码 { get; set; }

        /// <summary>
        ///     尘肺结算类型
        ///     Varchar2(3)
        ///     0、尘肺单病种结算 1、尘肺项目结算
        /// </summary>
        public string 尘肺结算类型 { get; set; }

        /// <summary>
        /// 医院科室自编码
        /// Varchar2(100) 
        /// </summary>
        public string 出院科室编码  { get; set; }
        /// <summary>
        /// 医师身份证号或证件号
        /// Varchar2(20) 
        /// </summary>
        public string 出院医师编码 { get; set; }
        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 将指定住院(或门诊)号对应的费用进行结算，但不产生结算记录。
    /// </summary>
    public class Res预结算 : Res
    {
        public string ToQuery()
        {
            return
                $"{执行代码}|{统筹支付}|{帐户支付}|{公务员补助}|{现金支付}|{大额理赔金额}|{历史起付线公务员返还}|{帐户余额}|{单病种定点医疗机构垫支}|{民政救助金额}|{民政救助门诊余额}|{耐多药项目支付金额}|{一般诊疗支付数}|{神华救助基金支付数}|{本年统筹支付累计}|{本年大额支付累计}|{特病起付线支付累计}|{耐多药项目累计}|{本年民政救助住院支付累计}|{中心结算时间}|{本次起付线支付金额}|{本次进入医保范围费用}|{药事服务支付数}|{医院超标扣款金额}|{生育基金支付}|{生育现金支付}|{工伤基金支付}|{工伤现金支付}|{工伤单病种机构垫支}|{工伤全自费原因}";
        }

        public static Res预结算 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res预结算();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            res.统筹支付 = list[1];
            res.帐户支付 = list[2];
            res.公务员补助 = list[3];
            res.现金支付 = list[4];
            res.大额理赔金额 = list[5];
            res.历史起付线公务员返还 = list[6];
            res.帐户余额 = list[7];
            res.单病种定点医疗机构垫支 = list[8];
            res.民政救助金额 = list[9];
            res.民政救助门诊余额 = list[10];
            res.耐多药项目支付金额 = list[11];
            res.一般诊疗支付数 = list[12];
            res.神华救助基金支付数 = list[13];
            res.本年统筹支付累计 = list[14];
            res.本年大额支付累计 = list[15];
            res.特病起付线支付累计 = list[16];
            res.耐多药项目累计 = list[17];
            res.本年民政救助住院支付累计 = list[18];
            res.中心结算时间 = list[19];
            res.本次起付线支付金额 = list[20];
            res.本次进入医保范围费用 = list[21];
            res.药事服务支付数 = list[22];
            res.医院超标扣款金额 = list[23];
            res.生育基金支付 = list[24];
            res.生育现金支付 = list[25];
            res.工伤基金支付 = list[26];
            res.工伤现金支付 = list[27];
            res.工伤单病种机构垫支 = list[28];
            res.工伤全自费原因 = list[29];
            // 执行代码|统筹支付|帐户支付|公务员补助|现金支付|大额理赔金额|历史起付线公务员返还|帐户余额|单病种定点医疗机构垫支|民政救助金额|民政救助门诊余额|耐多药项目支付金额|一般诊疗支付数|神华救助基金支付数|本年统筹支付累计|本年大额支付累计|特病起付线支付累计|耐多药项目累计|本年民政救助住院支付累计|中心结算时间|本次起付线支付金额|本次进入医保范围费用|药事服务支付数|医院超标扣款金额|生育基金支付|生育现金支付|工伤基金支付|工伤现金支付|工伤单病种机构垫支|工伤全自费原因
            return res;
        }

        #region Properties

        /// <summary>
        ///     统筹支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 统筹支付 { get; set; }

        /// <summary>
        ///     帐户支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 帐户支付 { get; set; }

        /// <summary>
        ///     公务员补助
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 公务员补助 { get; set; }

        /// <summary>
        ///     现金支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 现金支付 { get; set; }

        /// <summary>
        ///     大额理赔金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 大额理赔金额 { get; set; }

        /// <summary>
        ///     历史起付线公务员返还
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 历史起付线公务员返还 { get; set; }

        /// <summary>
        ///     帐户余额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 帐户余额 { get; set; }

        /// <summary>
        ///     单病种定点医疗机构垫支
        ///     Number(8,2)
        ///     非空
        ///     包括医院超标扣款金额
        /// </summary>
        public string 单病种定点医疗机构垫支 { get; set; }

        /// <summary>
        ///     民政救助金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 民政救助金额 { get; set; }

        /// <summary>
        ///     民政救助门诊余额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 民政救助门诊余额 { get; set; }

        /// <summary>
        ///     耐多药项目支付金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 耐多药项目支付金额 { get; set; }

        /// <summary>
        ///     一般诊疗支付数
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 一般诊疗支付数 { get; set; }

        /// <summary>
        ///     神华救助基金支付数
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 神华救助基金支付数 { get; set; }

        /// <summary>
        ///     本年统筹支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年统筹支付累计 { get; set; }

        /// <summary>
        ///     本年大额支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年大额支付累计 { get; set; }

        /// <summary>
        ///     特病起付线支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 特病起付线支付累计 { get; set; }

        /// <summary>
        ///     耐多药项目累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 耐多药项目累计 { get; set; }

        /// <summary>
        ///     本年民政救助住院支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年民政救助住院支付累计 { get; set; }

        /// <summary>
        ///     中心结算时间
        ///     DATE
        ///     非空
        ///     格式：YYYY-MM-DD HH24:MI:SS 返回中心该记录的结算时间，该时间也即该 结算记录对应的费款所属期
        /// </summary>
        public string 中心结算时间 { get; set; }

        /// <summary>
        ///     本次起付线支付金额
        ///     Number(8,2)
        /// </summary>
        public string 本次起付线支付金额 { get; set; }

        /// <summary>
        ///     本次进入医保范围费用
        ///     Number(8,2)
        /// </summary>
        public string 本次进入医保范围费用 { get; set; }

        /// <summary>
        ///     药事服务支付数
        ///     Number(8,2)
        /// </summary>
        public string 药事服务支付数 { get; set; }

        /// <summary>
        ///     医院超标扣款金额
        ///     Number(8,2)
        /// </summary>
        public string 医院超标扣款金额 { get; set; }

        /// <summary>
        ///     生育基金支付
        ///     Number(8,2)
        /// </summary>
        public string 生育基金支付 { get; set; }

        /// <summary>
        ///     生育现金支付
        ///     Number(8,2)
        /// </summary>
        public string 生育现金支付 { get; set; }

        /// <summary>
        ///     工伤基金支付
        ///     Number(8,2)
        /// </summary>
        public string 工伤基金支付 { get; set; }

        /// <summary>
        ///     工伤现金支付
        ///     Number(8,2)
        /// </summary>
        public string 工伤现金支付 { get; set; }

        /// <summary>
        ///     工伤单病种机构垫支
        ///     Number(8,2)
        /// </summary>
        public string 工伤单病种机构垫支 { get; set; }

        /// <summary>
        ///     工伤全自费原因
        ///     Varchar2(100)
        /// </summary>
        public string 工伤全自费原因 { get; set; }

        #endregion Properties
    }

    #endregion 06_预结算
}