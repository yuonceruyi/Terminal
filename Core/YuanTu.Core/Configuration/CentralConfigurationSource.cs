using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace YuanTu.Core.Configuration
{
    public class CentralConfigurationSource : IConfigurationSource
    {
        public string Ip { get; set; }
        public string Mac { get; set; }

        public string Url { get; set; }
        public Func<Dictionary<string, string>> GetLocalConfigFunc { get; set; }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new CentralConfigurationProvider(Ip, Mac, Url, GetLocalConfigFunc);
        }
    }
}
