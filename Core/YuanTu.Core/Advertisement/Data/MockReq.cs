using System.Collections.Generic;
using YuanTu.Core.Advertisement.Base;

namespace YuanTu.Core.Advertisement.Data
{
    public class MockReq : ReqBase
    {
        public MockReq(string urlPath, Dictionary<string, string> dic)
        {
            UrlPath = urlPath;
            Params = dic;
        }

        public override string UrlPath { get; }
        public override int adPositionId { get; }

        public Dictionary<string, string> Params { get; set; }

        public override Dictionary<string, string> BuldDict()
        {
            return Params;
        }
    }
}