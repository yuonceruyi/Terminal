using YuanTu.Core.Advertisement.Base;

namespace YuanTu.Core.Advertisement.Data
{
    public class GetAdvertisementReq : ReqBase
    {
        public override string UrlPath => "/adApi/open/findReceiptsAd.json";
        public override int adPositionId => 1;
    }

    public class GetAdvertisementRes : ResBase
    {
        public AdvertisementItem data { get; set; }
    }

    public class AdvertisementItem
    {
        public long adId { get; set; }
        public string picStr { get; set; }
        public int type { get; set; }
    }
}