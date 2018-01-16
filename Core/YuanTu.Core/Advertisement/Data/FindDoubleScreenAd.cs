using System.Collections.Generic;
using YuanTu.Core.Advertisement.Base;

namespace YuanTu.Core.Advertisement.Data
{
    public class FindDoubleScreenAdReq : ReqBase
    {
        public override string UrlPath => "/adApi/open/findDoubleScreenAd.json";
        public override int adPositionId => 27;
    }

    public class FindDoubleScreenAdRes : ResBase
    {
        public DoubleScreenAdData data { get; set; }
    }

    public class DoubleScreenAdData
    {
        public long lastUpdateTime { get; set; }

        public List<DoubleScreenAdItem> adList { get; set; }
    }

    public class DoubleScreenAdItem
    {
        public long adId { get; set; }
        public int type { get; set; }

        public List<DoubleScreenAdContent> contentList { get; set; }
    }

    public class DoubleScreenAdContent
    {
        public long id { get; set; }
        public long adId { get; set; }
        public int type { get; set; }
        public string url { get; set; }
        public int showTime { get; set; }
    }
}