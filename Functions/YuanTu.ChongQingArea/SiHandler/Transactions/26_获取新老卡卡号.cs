namespace YuanTu.ChongQingArea.SiHandler
{
    #region 26_获取新老卡卡号

    /// <summary>
    ///     功能描述: 通过新卡或者老卡卡号，获取新老卡号。
    /// </summary>
    public class Req获取新老卡卡号 : Req
    {
        public override string 交易类别代码 => "26";
        public override string 交易类别 => "获取新老卡卡号";

        public override string ToQuery()
        {
            return $"{交易类别代码}|{(string.IsNullOrEmpty(医保卡号) ? 社保卡号 : 医保卡号)}";
        }

        #region Properties

        /// <summary>
        ///     医保卡卡号
        ///     Varchar2(8)
        ///     非空
        /// </summary>
        public string 医保卡号 { get; set; }

        /// <summary>
        ///     社保卡卡号
        ///     Varchar2(9)
        ///     非空
        /// </summary>
        public string 社保卡号 { get; set; }

        #endregion Properties
    }

    /// <summary>
    ///     功能描述: 通过新卡或者老卡卡号，获取新老卡号。
    /// </summary>
    public class Res获取新老卡卡号 : Res
    {
        //public string ToQuery()
        //{
        //    return $"{执行代码}|{医保卡号}|{社保卡号}";
        //}
        public static Res获取新老卡卡号 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res获取新老卡卡号();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            res.医保卡号 = list[1];
            res.社保卡号 = list[2];
            // 执行代码|医保卡号(8 位)|社保卡号(9 位)
            return res;
        }

        #region Properties

        /// <summary>
        ///     医保卡卡号
        ///     Varchar2(8)
        ///     非空
        /// </summary>
        public string 医保卡号 { get; set; }

        /// <summary>
        ///     社保卡卡号
        ///     Varchar2(9)
        ///     非空
        /// </summary>
        public string 社保卡号 { get; set; }

        #endregion Properties
    }

    #endregion 26_获取新老卡卡号
}