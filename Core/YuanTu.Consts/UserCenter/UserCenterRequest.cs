using System.Collections.Generic;

namespace YuanTu.Consts.UserCenter
{
    public class UserCenterRequest
    {
        public UserCenterRequest()
        {
            token= FrameworkConst.Token;
        }
        public virtual string UrlPath { get; }
        public virtual string ServiceName { get; }
        public virtual string token { get; set; }
        public virtual Dictionary<string, string> GetParams()
        {
            return new Dictionary<string, string>
            {
                [nameof(token)] = token
            };
        }
    }
}
