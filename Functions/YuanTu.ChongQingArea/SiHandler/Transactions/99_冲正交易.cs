namespace YuanTu.ChongQingArea.SiHandler
{
    #region 99_冲正交易

    /// <summary>
    ///     功能描述: 对指定交易流水号的交易进行冲正处理，即对指定的操作进行取消操作
    /// </summary>
    public class Req冲正交易 : Req
    {
        public override string 交易类别代码 => "99";
        public override string 交易类别 => "冲正交易";

        public override string ToQuery()
        {
            return $"{交易类别代码}|{交易流水号}|{经办人}|{住院号_门诊号}|{冲账类型}|{险种类别}";
        }

        #region Properties

        /// <summary>
        ///     交易流水号
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 交易流水号 { get; set; }

        /// <summary>
        ///     经办人
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 经办人 { get; set; }

        /// <summary>
        ///     住院(门诊)号
        ///     Varchar2(18)
        ///     非空
        /// </summary>
        public string 住院号_门诊号 { get; set; }

        /// <summary>
        ///     冲账类型
        ///     Varchar2(3)
        ///     非空
        ///     0、 普通冲正 1、 改账冲正(需中心审批)
        /// </summary>
        public string 冲账类型 { get; set; }

        /// <summary>
        ///     险种类别
        ///     Varchar2(3)
        ///     非空
        ///     1、医疗保险；2、工伤保险；3、生育保险；
        /// </summary>
        public string 险种类别 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 对指定交易流水号的交易进行冲正处理，即对指定的操作进行取消操作
    /// </summary>
    public class Res冲正交易 : Res
    {
        //public string ToQuery()
        //{
        //    //[冲正就诊登记和处方明细时]
        //    var s1 = $"{执行代码}";
        //    //[冲正结算记录时]
        //    return
        //        $"{执行代码}|{负结算记录交易流水号}|{应退统筹金额}|{应退帐户金额}|{应退公务员补助}|{应退现金支付}|{应退大额理赔金额}|{应退历史起付线公务员返还}|{应退民政救助金额}|{应退中盖基金支付金额}|{应退降消项目金额}|{应退神华救助基金支付数}|{应退支付扩展预留2}|{中心结算时间}|{应退生育基金金额}|{应退生育现金支付}|{应退工伤基金金额}|{应退工伤现金支付}";
        //}
        public static Res冲正交易 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res冲正交易();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            if (list.Length > 2)
            {
                res.负结算记录交易流水号 = list[1];
                res.应退统筹金额 = list[2];
                res.应退帐户金额 = list[3];
                res.应退公务员补助 = list[4];
                res.应退现金支付 = list[5];
                res.应退大额理赔金额 = list[6];
                res.应退历史起付线公务员返还 = list[7];
                res.应退民政救助金额 = list[8];
                res.应退中盖基金支付金额 = list[9];
                res.应退降消项目金额 = list[10];
                res.应退神华救助基金支付数 = list[11];
                res.应退支付扩展预留2 = list[12];
                res.中心结算时间 = list[13];
            }
            if (list.Length > 14)
            {
                res.应退生育基金金额 = list[14];
                res.应退生育现金支付 = list[15];
                res.应退工伤基金金额 = list[16];
                res.应退工伤现金支付 = list[17];
            }
            // [冲正就诊登记和处方明细时]执行代码[冲正结算记录时]:执行代码|负结算记录交易流水号|应退统筹金额|应退帐户金额|应退公务员补助|应退现金支付|应退大额理赔金额|应退历史起付线公务员返还|应退民政救助金额|应退中盖基金支付金额|应退降消项目金额|应退神华救助基金支付数|应退支付扩展预留 2|中心结算时间|应退生育基金金额|应退生育现金支付|应退工伤基金金额|应退工伤现金支付
            return res;
        }

        #region Properties

        /// <summary>
        ///     负结算记录交易流水号
        ///     Varchar2(20)
        ///     非空
        /// </summary>
        public string 负结算记录交易流水号 { get; set; }

        /// <summary>
        ///     应退统筹金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退统筹金额 { get; set; }

        /// <summary>
        ///     应退帐户金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退帐户金额 { get; set; }

        /// <summary>
        ///     应退公务员补助
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退公务员补助 { get; set; }

        /// <summary>
        ///     应退现金支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退现金支付 { get; set; }

        /// <summary>
        ///     应退大额理赔金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退大额理赔金额 { get; set; }

        /// <summary>
        ///     应退历史起付线公务员返还
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退历史起付线公务员返还 { get; set; }

        /// <summary>
        ///     应退民政救助金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退民政救助金额 { get; set; }

        /// <summary>
        ///     应退中盖基金支付金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退中盖基金支付金额 { get; set; }

        /// <summary>
        ///     应退降消项目金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退降消项目金额 { get; set; }

        /// <summary>
        ///     应退神华救助基金支付数
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退神华救助基金支付数 { get; set; }

        /// <summary>
        ///     应退支付扩展预留2
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退支付扩展预留2 { get; set; }

        /// <summary>
        ///     中心结算时间
        ///     DATE
        ///     非空
        ///     格式：YYYY-MM-DD   HH24:MI:SS
        /// </summary>
        public string 中心结算时间 { get; set; }

        /// <summary>
        ///     应退生育基金金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退生育基金金额 { get; set; }

        /// <summary>
        ///     应退生育现金支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退生育现金支付 { get; set; }

        /// <summary>
        ///     应退工伤基金金额
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退工伤基金金额 { get; set; }

        /// <summary>
        ///     应退工伤现金支付
        ///     Number(8,2)
        ///     非空
        /// </summary>
        public string 应退工伤现金支付 { get; set; }

        #endregion Properties
    }

    #endregion 99_冲正交易
}