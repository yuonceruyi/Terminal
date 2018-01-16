using System;

namespace YuanTu.ChongQingArea.SiHandler
{

    #region 13_获取人员账户基础信息

    /// <summary>
    ///     功能描述: 获取患者的基本账户信息。
    /// </summary>
    public class Req获取人员账户基础信息 : Req
    {
        public override string 交易类别代码 => "13";
        public override string 交易类别 => "获取人员账户基础信息";

        public override string ToQuery()
        {
            return $"{交易类别代码}|{(string.IsNullOrEmpty(社保卡卡号) ? 老医保卡卡号 : 社保卡卡号)}|{险种类别}";
        }

        #region Properties

        /// <summary>
        ///     社保卡卡号
        ///     Varchar2(20)
        ///     非空
        ///     社保卡卡号、老医保卡卡号二选一，其中： 社保卡卡号 9 位； 老医保卡卡号 8 位; 注：城居参保人未拿到卡之前，用合作医疗 号代替金保卡卡号输入
        /// </summary>
        public string 社保卡卡号 { get; set; }

        /// <summary>
        ///     老医保卡卡号
        ///     Varchar2(8)
        ///     非空
        /// </summary>
        public string 老医保卡卡号 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险
        /// </summary>
        public string 险种类别 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 获取患者的基本账户信息。
    /// </summary>
    public class Res获取人员账户基础信息 : Res
    {
        //public string ToQuery()
        //{
        //    //医疗保险返回参数【包括职工医保、居民医保、离休干部医保】:
        //    var s1 = $"{执行代码}|{帐户余额}|{本年统筹支付累积}|{本年特殊门诊起付标准支付累计}|{本年特殊门诊医保费累计}|{本年恶性肿瘤住院起付标准支付累计}|{本年符合公务员范围门诊费用累计}|{本年住院次数}|{住院状态}|{本年特病门诊还需补助的自付金额}|{本年住院还需补助的自付金额}|{本年发生过恶性肿瘤标志}|{本年大病支付累计}|{居保重大疾病发生标志}|{本年意外伤害支付累计}|{本年耐多药结核支付累计}|{本年儿童两病支付累计}|{本年康复项目支付累计}|{年度民政住院支付累计}|{年度民政门诊支付累计}|{降消补助累计}|{年度普通门诊统筹累计}|{异地登记标识}|{账户信息预留1}|{账户信息预留2}";
        //    //工伤保险返回参数:
        //    var s2 = $"{执行代码}|{尘肺项目天数累计}|{住院状态}";
        //    //生育保险返回参数:
        //    return $"{执行代码}|{本孕期产前检查支付累计}|{本孕期遗传病基因检查支付累计}|{本孕期计划生育手术支付累计}|{本孕期分娩或终止妊娠医疗费支付累计}|{本孕期并发症支付累计}|{住院状态}";
        //}
        public static Res获取人员账户基础信息 Parse(string s, string 险种类别 = "1")
        {
            var list = s.Split('|');
            var res = new Res获取人员账户基础信息();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            switch (险种类别)
            {
                case "1":
                    res.帐户余额 = list[1];
                    res.本年统筹支付累积 = list[2];
                    res.本年特殊门诊起付标准支付累计 = list[3];
                    res.本年特殊门诊医保费累计 = list[4];
                    res.本年恶性肿瘤住院起付标准支付累计 = list[5];
                    res.本年符合公务员范围门诊费用累计 = list[6];
                    res.本年住院次数 = list[7];
                    res.住院状态 = list[8];
                    res.本年特病门诊还需补助的自付金额 = list[9];
                    res.本年住院还需补助的自付金额 = list[10];
                    res.本年发生过恶性肿瘤标志 = list[11];
                    res.本年大病支付累计 = list[12];
                    res.居保重大疾病发生标志 = list[13];
                    res.本年意外伤害支付累计 = list[14];
                    res.本年耐多药结核支付累计 = list[15];
                    res.本年儿童两病支付累计 = list[16];
                    res.本年康复项目支付累计 = list[17];
                    res.年度民政住院支付累计 = list[18];
                    res.年度民政门诊支付累计 = list[19];
                    res.降消补助累计 = list[20];
                    res.年度普通门诊统筹累计 = list[21];
                    res.异地登记标识 = list[22];
                    res.账户信息预留1 = list[23];
                    res.账户信息预留2 = list[24];
                    break;

                case "2":
                    res.尘肺项目天数累计 = list[1];
                    res.住院状态 = list[2];
                    break;

                case "3":
                    res.本孕期产前检查支付累计 = list[1];
                    res.本孕期遗传病基因检查支付累计 = list[2];
                    res.本孕期计划生育手术支付累计 = list[3];
                    res.本孕期分娩或终止妊娠医疗费支付累计 = list[4];
                    res.本孕期并发症支付累计 = list[5];
                    res.住院状态 = list[6];
                    break;

                default:
                    throw new NotSupportedException(nameof(险种类别));
            }
            // 医疗保险返回参数【包括职工医保、居民医保、离休干部医保】:执行代码|帐户余额|本年统筹支付累积|本年特殊门诊起付标准支付累计|本年特殊门诊医保费累计|本年恶性肿瘤住院起付标准支付累计|本年符合公务员范围门诊费用累计|本年住院次数|住院状态|本年特病门诊还需补助的自付金额|本年住院还需补助的自付金额|本年发生过恶性肿瘤标志|本年大病支付累计|居保重大疾病发生标志|本年意外伤害支付累计|本年耐多药结核支付累计|本年儿童两病支付累计|本年康复项目支付累计|年度民政住院支付累计|年度民政门诊支付累计|降消补助累计|年度普通门诊统筹累计|异地登记标识|账户信息预留1|账户信息预留2工伤保险返回参数:执行代码|尘肺项目天数累计|住院状态生育保险返回参数:执行代码|本孕期产前检查支付累计|本孕期遗传病基因检查支付累计|本孕期计划生育手术支付累计|本孕期分娩或终止妊娠医疗费支付累计|本孕期并发症支付累计|住院状态
            return res;
        }

        #region Properties

        /// <summary>
        ///     帐户余额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 帐户余额 { get; set; }

        /// <summary>
        ///     统筹支付累积
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年统筹支付累积 { get; set; }

        /// <summary>
        ///     本年特殊门诊起付标准支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年特殊门诊起付标准支付累计 { get; set; }

        /// <summary>
        ///     特殊门诊医保费累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年特殊门诊医保费累计 { get; set; }

        /// <summary>
        ///     本年恶性肿瘤住院起付标准支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年恶性肿瘤住院起付标准支付累计 { get; set; }

        /// <summary>
        ///     本年符合公务员范围门诊费用累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年符合公务员范围门诊费用累计 { get; set; }

        /// <summary>
        ///     本年住院次数
        ///     Number(3)
        ///     非空
        /// </summary>
        public string 本年住院次数 { get; set; }

        /// <summary>
        ///     住院状态
        ///     Varchar2(3)
        ///     非空
        ///     0、未住院；1、在住院
        /// </summary>
        public string 住院状态 { get; set; }

        /// <summary>
        ///     本年大病支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年大病支付累计 { get; set; }

        /// <summary>
        ///     特病门诊还需补助的自付金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年特病门诊还需补助的自付金额 { get; set; }

        /// <summary>
        ///     住院还需补助的自付 金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年住院还需补助的自付金额 { get; set; }

        /// <summary>
        ///     本年发生过恶性肿瘤标志
        ///     Varchar2(3)
        ///     非空
        ///     0、未发生；1、已发生
        /// </summary>
        public string 本年发生过恶性肿瘤标志 { get; set; }

        /// <summary>
        ///     居保重大疾病发生标志
        ///     Varchar2(3)
        ///     非空
        ///     0、未发生；1、已发生
        /// </summary>
        public string 居保重大疾病发生标志 { get; set; }

        /// <summary>
        ///     本年意外伤害支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年意外伤害支付累计 { get; set; }

        /// <summary>
        ///     本年耐多药结核支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年耐多药结核支付累计 { get; set; }

        /// <summary>
        ///     本年儿童两病支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年儿童两病支付累计 { get; set; }

        /// <summary>
        ///     本年康复项目支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本年康复项目支付累计 { get; set; }

        /// <summary>
        ///     年度民政住院支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 年度民政住院支付累计 { get; set; }

        /// <summary>
        ///     年度民政门诊支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 年度民政门诊支付累计 { get; set; }

        /// <summary>
        ///     降消补助累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 降消补助累计 { get; set; }

        /// <summary>
        ///     年度普通门诊统筹累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 年度普通门诊统筹累计 { get; set; }

        /// <summary>
        ///     异地登记标识
        ///     Varchar2(3)
        ///     非空
        ///     0	未登记异地 1	区内二级以上医院就诊 2	长期驻外 3	区外急诊 4	区内转诊 5	区外转诊
        /// </summary>
        public string 异地登记标识 { get; set; }

        /// <summary>
        ///     账户信息预留1
        ///     Number(8,2)
        ///     备用字段
        /// </summary>
        public string 账户信息预留1 { get; set; }

        /// <summary>
        ///     账户信息预留2
        ///     Number(8,2)
        ///     备用字段
        /// </summary>
        public string 账户信息预留2 { get; set; }

        /// <summary>
        ///     尘肺项目天数累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 尘肺项目天数累计 { get; set; }

        /// <summary>
        ///     本孕期产前检查支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本孕期产前检查支付累计 { get; set; }

        /// <summary>
        ///     本孕期遗传病基因检查支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本孕期遗传病基因检查支付累计 { get; set; }

        /// <summary>
        ///     本孕期计划生育手术支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本孕期计划生育手术支付累计 { get; set; }

        /// <summary>
        ///     本孕期分娩或终止妊娠医疗费支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本孕期分娩或终止妊娠医疗费支付累计 { get; set; }

        /// <summary>
        ///     本孕期并发症支付累计
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 本孕期并发症支付累计 { get; set; }

        #endregion Properties
    }

    #endregion 13_获取人员账户基础信息
}