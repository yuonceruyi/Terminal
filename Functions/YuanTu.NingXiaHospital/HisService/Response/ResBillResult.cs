namespace YuanTu.NingXiaHospital.HisService.Response
{
    public class ResBillResult : Response
    {
        public ReDataBillResult redata { get; set; }
    }

    public class ReDataBillResult
    {
        public ResHeadBillResult head { get; set; }
        public DetailBillResult detail { get; set; }
    }


    public class DetailBillResult : IDetail
    {
    }

    public class ResHeadBillResult : Head
    {
        public string jsid { get; set; }

        public string result { get; set; }
    }
}