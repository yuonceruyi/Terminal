namespace YuanTu.NingXiaHospital.HisService.Request
{
    //就诊缴费 M2
    public class ReqHeadBill : Head
    {
        public string fylx { get; set; }

        public string klx { get; set; }

        public string kh { get; set; }

        public string jsid { get; set; }

        public string hbid { get; set; }

        public string fyje { get; set; }

        public string czy { get; set; }

        public string zfmode { get; set; }
    }
}