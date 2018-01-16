namespace YuanTu.ChongQingArea.SiHandler
{
    #region 03_更新就诊信息

    /// <summary>
    ///     功能描述: 实现对就诊登记信息的修改，主要包括住院类别、确诊疾病、出院日期、并发症等。
    /// </summary>
    public class Req更新就诊信息 : Req
    {
        public override string 交易类别代码 => "03";
        public override string 交易类别 => "更新就诊信息";

        public override string ToQuery()
        {
            return
                $"{交易类别代码}|{住院号_门诊号}|{更新标志}|{医疗类别}|{科室编码}|{医生编码}|{入院日期}|{入院诊断}|{出院日期}|{确诊疾病编码}|{出院原因}|{经办人}|{并发症}|{病案号}|{生育证号码}|{新生儿出生日期}|{居民特殊就诊标记}|{险种类别}|{转入医院编码}";
        }

        #region Properties

        /// <summary>
        ///     住院号(门诊号)
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 住院号_门诊号 { get; set; }

        /// <summary>
        ///     更新标志
        ///     Varchar2(20)
        ///     非空
        ///     更新标志由 16 个‘0’和‘1’组成，分别 代表后面的 16 个字段， 0	表示不需要更新； 1	表示需要更新； 更新字段的顺序为：医疗类别，科室编码， 医生编码，入院日期，入院诊断，出院日期，
        ///     确诊疾病编码，出院原因，经办人，并发症， 并案号，生育证号码，新生儿出生日期，居 民特殊就诊标记，险种类别，转入医院编码； 例如：更新标志置为 ‘1000000000000000’，表示只更新医疗 类别字段值，而置为
        ///     ‘1111111111111111’，则表示要更新所 有字段。
        /// </summary>
        public string 更新标志 { get; set; }

        /// <summary>
        ///     医疗类别
        ///     Varchar2(3)
        ///     10	药店购药 11	普通门诊 13	特病门诊 14	急诊 15	超声乳化白内障摘除 16	生育门诊 17	工伤门诊 18	工伤康复门诊 19	工伤辅助器具 21	住院 22	转入住院 23	出院家庭病床 24	高龄家庭病床 25
        ///     急诊转住院 26	工伤康复住院 当本次更新不涉及此字段时，可以为空。 下同。
        /// </summary>
        public string 医疗类别 { get; set; }

        /// <summary>
        ///     科室
        ///     Varchar2(20)
        /// </summary>
        public string 科室编码 { get; set; }

        /// <summary>
        ///     医生
        ///     Varchar2(20)
        /// </summary>
        public string 医生编码 { get; set; }

        /// <summary>
        ///     入院日期
        ///     Varchar2(10)
        ///     格式：YYYY-MM-DD
        /// </summary>
        public string 入院日期 { get; set; }

        /// <summary>
        ///     入院诊断
        ///     Varchar2(20)
        /// </summary>
        public string 入院诊断 { get; set; }

        /// <summary>
        ///     出院日期
        ///     Varchar2(10)
        ///     格式：YYYY-MM-DD
        /// </summary>
        public string 出院日期 { get; set; }

        /// <summary>
        ///     确诊疾病编码
        ///     Varchar2(20)
        ///     确诊疾病编码必须是医保规定的病种目录 内的一个病种编码，对于患有多个病种的患 者，以描述性的文字在并发症中加以描述。
        /// </summary>
        public string 确诊疾病编码 { get; set; }

        /// <summary>
        ///     出院原因
        ///     Varchar2(3)
        ///     1	康复 2	转院 3	死亡 4	好转 9	其它
        /// </summary>
        public string 出院原因 { get; set; }

        /// <summary>
        ///     经办人
        ///     Varchar2(20)
        /// </summary>
        public string 经办人 { get; set; }

        /// <summary>
        ///     并发症
        ///     Varchar2(200)
        /// </summary>
        public string 并发症 { get; set; }

        /// <summary>
        ///     病案号
        ///     Varchar2(20)
        ///     全国统一编号
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
        ///     非空
        ///     10	药店购药 12	普通门诊 13	慢性病门诊 15	重大疾病门诊 16	意外伤害门诊 17	耐多药结核门诊 18	儿童两病门诊 19	产前检查 21	普通住院 22	转入住院 23	耐多药结核住院 24	儿童两病住院 25
        ///     重大疾病住院 26	住院分娩 27	急诊转住院 31	康复项目 此字段即：居民医疗类别
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
        ///     转入医院编码
        ///     Varchar2(14)
        ///     当耐药结核住院需从定点医院转院到非定 点医院时，定点医院需填写转入医院编码。 其他情况不用填写
        /// </summary>
        public string 转入医院编码 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 实现对就诊登记信息的修改，主要包括住院类别、确诊疾病、出院日期、并发症等。
    /// </summary>
    public class Res更新就诊信息 : Res
    {
        //public string ToQuery()
        //{
        //    return $"{执行代码}";
        //}
        public static Res更新就诊信息 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res更新就诊信息();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            // 执行代码
            return res;
        }
    }

    #endregion 03_更新就诊信息
}