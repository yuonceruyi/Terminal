namespace YuanTu.ChongQingArea.SiHandler
{
    #region 02_就诊登记

    /// <summary>
    ///     功能描述: 实现门诊、住院的就诊登记业务，同时，完成对就诊患者身份的验证、就诊资格审查以及待遇享受等情况的设定。
    /// </summary>
    public class Req就诊登记 : Req
    {
        public override string 交易类别代码 => "02";
        public override string 交易类别 => "就诊登记";

        public override string ToQuery()
        {
            return
                $"{交易类别代码}|{住院号_门诊号}|{医疗类别}|{社会保障识别号}|{入院科室编码}|{入院医师编码}|{入院日期}|{入院诊断}|{操作员}|{并发症信息}|{急诊转住院发生时间}|{病案号}|{生育证号码}|{新生儿出生日期}|{居民特殊就诊标记}|{险种类别}|{工伤个人编号}|{工伤单位编号}";
        }

        #region Properties

        /// <summary>
        ///     住院(门诊)号
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 住院号_门诊号 { get; set; }

        /// <summary>
        ///     医疗类别
        ///     Varchar2(3)
        ///     10	药店购药 11	普通门诊 13	特病门诊 14	急诊 15	超声乳化白内障摘除 16	生育门诊 17	工伤门诊 18	工伤康复门诊 19	工伤辅助器具 21	住院 22	转入住院 23	出院家庭病床 24	高龄家庭病床 25
        ///     急诊转住院 26	工伤康复住院 若是居民参保患者时，此字段可以为空
        /// </summary>
        public string 医疗类别 { get; set; }

        /// <summary>
        ///     社会保障识别号
        ///     Varchar2(20)
        ///     非空
        ///     社保卡卡号、老医保卡卡号二选一，其中： 社保卡卡号 9 位； 老医保卡卡号 8 位; 注：城居参保人未拿到卡之前，用身份证号 或者金保号输入
        /// </summary>
        public string 社会保障识别号 { get; set; }

        /// <summary>
        ///     医院科室自编码 
        ///     Varchar2(100)
        ///     非空
        /// </summary>
        public string 入院科室编码 { get; set; }

        /// <summary>
        ///     医师身份证号或证件号
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 入院医师编码 { get; set; }

        /// <summary>
        ///     入院日期
        ///     Varchar2(10)
        ///     非空
        ///     格式：YYYY-MM-DD,入院日期必须小于或 者等于当前时间。
        /// </summary>
        public string 入院日期 { get; set; }

        /// <summary>
        ///     入院诊断
        ///     Varchar2(20)
        ///     对于普通门诊，入院诊断可以为空； 对于特殊门诊和急诊，入院诊断必须为审批 过的特殊病编码或急诊病编码范围内的编 码； 对于住院，入院诊断可以直接填写患者所患 的病种名称或一段描述性语言，不要填写病 种的编码
        ///     对于生育门诊，入院诊断可以为空
        /// </summary>
        public string 入院诊断 { get; set; }

        /// <summary>
        ///     操作员
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 操作员 { get; set; }

        /// <summary>
        ///     并发症信息
        ///     Varchar2(200)
        ///     为空表示无并发症信息
        /// </summary>
        public string 并发症信息 { get; set; }

        /// <summary>
        ///     急诊转住院发生时间
        ///     Varchar2(10)
        ///     格式：YYYY-MM-DD 当医疗类别为急诊转住院时，此值非空
        /// </summary>
        public string 急诊转住院发生时间 { get; set; }

        /// <summary>
        ///     病案号
        ///     Varchar2(20)
        ///     为空表示无并案号
        /// </summary>
        public string 病案号 { get; set; }

        /// <summary>
        ///     生育证号码
        ///     Varchar2(20)
        ///     当居保新生儿随母住院或唐氏筛查产前检 查时，填入此号码。其他情况不用填写。
        /// </summary>
        public string 生育证号码 { get; set; }

        /// <summary>
        ///     新生儿出生日期
        ///     Varchar2(10)
        ///     格式：YYYY-MM-DD 当居保新生儿随母住院时，填入此号码。其 他情况不用填写
        /// </summary>
        public string 新生儿出生日期 { get; set; }

        /// <summary>
        ///     居民特殊就诊标记
        ///     Varchar2(3)
        ///     10	药店购药 12	普通门诊 13	慢性病门诊 15	重大疾病门诊 16	意外伤害门诊 17	耐多药结核门诊 18	儿童两病门诊 19	产前检查 21	普通住院 22	转入住院 23	耐多药结核住院 24	儿童两病住院 25
        ///     重大疾病住院 26	住院分娩 27	急诊转住院 31	康复项目 此字段即：居民医疗类别 若是职工参保患者时，此字段可以为空
        /// </summary>
        public string 居民特殊就诊标记 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险；
        /// </summary>
        public string 险种类别 { get; set; }

        /// <summary>
        ///     工伤个人编号
        ///     Varchar2(10)
        ///     若是工伤参保患者时，此字段不可以为空
        /// </summary>
        public string 工伤个人编号 { get; set; }

        /// <summary>
        ///     工伤单位编号
        ///     Varchar2(10)
        ///     若是工伤参保患者时，此字段不可以为空
        /// </summary>
        public string 工伤单位编号 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 实现门诊、住院的就诊登记业务，同时，完成对就诊患者身份的验证、就诊资格审查以及待遇享受等情况的设定。
    /// </summary>
    public class Res就诊登记 : Res
    {
        //public string ToQuery()
        //{
        //    return $"{执行代码}|{交易流水号}|{帐户余额}|{原转出医院名称}|{参保类别}";
        //}
        public static Res就诊登记 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res就诊登记();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            res.交易流水号 = list[1];
            res.帐户余额 = list[2];
            res.原转出医院名称 = list[3];
            res.参保类别 = list[4];
            // 执行代码|交易流水号|帐户余额|原转出医院名称|参保类别
            return res;
        }

        #region Properties

        /// <summary>
        ///     交易流水号
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 交易流水号 { get; set; }

        /// <summary>
        ///     帐户余额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 帐户余额 { get; set; }

        /// <summary>
        ///     原转出医院名称
        ///     Varchar2(50)
        ///     本次办理的不是转入入院，则该字段为空
        /// </summary>
        public string 原转出医院名称 { get; set; }

        /// <summary>
        ///     参保类别
        ///     Varchar2(3)
        ///     非空
        ///     1、职工医保；2、居民医保；3、离休干部
        /// </summary>
        public string 参保类别 { get; set; }

        #endregion Properties
    }

    #endregion 02_就诊登记
}