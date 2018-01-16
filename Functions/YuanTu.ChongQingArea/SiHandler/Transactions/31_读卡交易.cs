namespace YuanTu.ChongQingArea.SiHandler
{
    #region 31_读卡交易

    /// <summary>
    ///     功能描述: 通过读卡返回社保卡卡号，无需输入密码
    /// </summary>
    public class Req读卡交易 : Req
    {
        public override string 交易类别代码 => "31";
        public override string 交易类别 => "读卡交易";

        public override string ToQuery()
        {
            return $"{交易类别代码}|";
        }
    }

    /// <summary>
    ///     功能描述: 通过读卡返回社保卡卡号，无需输入密码
    /// </summary>
    public class Res读卡交易 : Res
    {
        #region Properties

        /// <summary>
        ///     社保卡卡号
        ///     Varchar2(9)
        ///     非空
        ///     目前社保卡卡号为 9 位。 需将东软接口动态库 SiInterface.dll 升级 到 1.0.3.2(或以上)版本。
        /// </summary>
        public string 社保卡卡号 { get; set; }

        #endregion Properties

        //public string ToQuery()
        //{
        //    return $"{执行代码}|{社保卡卡号}";
        //}
        public static Res读卡交易 Parse(string s)
        {
            var list = s.Split('|');
            var res = new Res读卡交易();
            res.执行代码 = list[0];
            if (res.执行代码 != "1")
            {
                res.错误信息 = list[1];
                return res;
            }
            res.社保卡卡号 = list[1];
            // 执行代码|社保卡卡号
            return res;
        }
    }

    #endregion 31_读卡交易
}