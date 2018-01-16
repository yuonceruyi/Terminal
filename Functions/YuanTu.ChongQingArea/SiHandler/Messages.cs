namespace YuanTu.ChongQingArea.SiHandler
{
    public class Req
    {
        public virtual string 交易类别代码 { get; }

        public virtual string 交易类别 { get; }

        public virtual string ToQuery()
        {
            return null;
        }
    }

    public class Res
    {
        public string 执行代码 { get; set; }
        public string 错误信息 { get; set; }
    }
}