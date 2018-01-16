using System.Collections.Generic;

namespace YuanTu.NingXiaHospital.HisService.Response
{
    public class ResQueryPrescription : Response
    {
        public ReDataQueryPrescription redata { get; set; }
    }

    public class ReDataQueryPrescription
    {
        public ResHeadQueryPrescription head { get; set; }
        public List<DetailQueryPrescription> detail { get; set; }
    }


    public class DetailQueryPrescription : IDetail
    {
        public string yylsh { get; set; }

        public string fyje { get; set; }

        public string grxjzfje { get; set; }
    }

    public class ResHeadQueryPrescription : Head
    {
        public string jsid { get; set; }

        public string klx { get; set; }

        public string kh { get; set; }

        public string fyje { get; set; }

        public string ybzfje { get; set; }

        public string grzhzfje { get; set; }

        public string grxjzfje { get; set; }
    }
}