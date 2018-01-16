using YuanTu.Core.Advertisement.Base;

namespace YuanTu.Core.Advertisement.Data
{
    public class UploadUsageInfoReq : ReqBase
    {
        public override string UrlPath => "adApi/open/addReceiptsAdResult.json";
        public override int adPositionId => 1;
    }
}
