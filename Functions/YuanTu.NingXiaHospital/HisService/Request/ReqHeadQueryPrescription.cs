namespace YuanTu.NingXiaHospital.HisService.Request
{
    ///处方提取 M1
    public class ReqHeadQueryPrescription : Head
    {
        public string klx { get; set; }

        public string kh { get; set; }

        public string czy { get; set; }
    }
}