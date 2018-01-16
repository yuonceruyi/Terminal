using System;
using System.Collections.Generic;
using YuanTu.Consts;

namespace YuanTu.Default.House.HealthManager.Base
{
    public class RequestBase
    {
        public string serviceName { get; set; }

        public string service { get; set; }

        protected string noncestr => new Guid().ToString();

        protected string timestamp
        {
            get
            {
                var start = new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local);
                return Convert.ToInt64((DateTimeCore.Now - start).TotalMilliseconds).ToString();
            }
        }

        public virtual Dictionary<string, string> GetParams()
        {
            return new Dictionary<string, string>
            {
                {nameof(noncestr), noncestr},
                {nameof(timestamp), timestamp}
            };
        }
    }
}