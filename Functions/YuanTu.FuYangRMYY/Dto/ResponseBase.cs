namespace YuanTu.FuYangRMYY
{
    public abstract class ResponseBase
    {

        /// <remarks/>
        public int ResultCode { get; set; }
        public string ResultContent { get; set; }
        /// <remarks/>
        public string ErrMsg { get; set; }
    }
}
