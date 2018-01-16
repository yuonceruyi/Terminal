namespace YuanTu.NingXiaHospital.HisService.Response
{
    public class ResPatientSign : Response
    {
        public ReDataPatientSign redata { get; set; }
    }

    public class ReDataPatientSign
    {
        public ResHeadPatientSign head { get; set; }
        public DetailPatientSign detail { get; set; }
    }


    public class DetailPatientSign : IDetail
    {
        public string yylsh { get; set; }

        public string fyje { get; set; }

        public string grxjzfje { get; set; }
    }

    public class ResHeadPatientSign : Head
    {
        public string result { get; set; }

        public string klx { get; set; }

        public string kh { get; set; }
    }
}