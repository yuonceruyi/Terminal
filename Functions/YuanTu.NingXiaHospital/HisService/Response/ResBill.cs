using System.Collections.Generic;

namespace YuanTu.NingXiaHospital.HisService.Response
{
    public class ResBill : Response
    {
        public ReDataBill redata { get; set; }
    }

    public class ReDataBill
    {
        public ResHeadBill head { get; set; }
        public List<DetailBill> detail { get; set; }
    }


    public class DetailBill : IDetail
    {
        public string yblsh { get; set; }

        public string yylsh { get; set; }

        public string grxjzfje { get; set; }

        public string fphm { get; set; }
    }

    public class ResHeadBill : Head
    {
        public string jsid { get; set; }

        public string fyje { get; set; }

        public string ybzfje { get; set; }

        public string grzhzfje { get; set; }

        public string grxjzfje { get; set; }
    }
}