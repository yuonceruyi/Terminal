namespace YuanTu.NingXiaHospital.HisService.Request
{
    //病人信息签约查询 M5
    public class ReqHeadPatientSignInfoQuery : Head
    {
        public string klx { get; set; }

        public string kh { get; set; }
    }
}