namespace YuanTu.NingXiaHospital.HisService.Response
{
    public class ResPatientSignInfoQuery : Response
    {
        public ReDataPatientSignInfoQuery redata { get; set; }
    }

    public class ReDataPatientSignInfoQuery
    {
        public ResHeadPatientSignInfoQuery head { get; set; }
        public DetailPatientSignInfoQuery detail { get; set; }
    }


    public class DetailPatientSignInfoQuery : IDetail
    {
        public string yylsh { get; set; }

        public string fyje { get; set; }

        public string grxjzfje { get; set; }
    }

    public class ResHeadPatientSignInfoQuery : Head
    {
        public string klx { get; set; }

        public string kh { get; set; }

        public string sfzh { get; set; }

        public string xm { get; set; }

        public string xb { get; set; }

        public string sjhm { get; set; }

        public string jzkye { get; set; }

        public string patient_id { get; set; }

        public string fy { get; set; }
    }
}