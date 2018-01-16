using System;
using System.Collections.Generic;
using YuanTu.Core.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    public static class CentralConfigurationExtensions
    {
        public static IConfigurationBuilder AddCentral(this IConfigurationBuilder builder, string ip, string mac, string url, Func<Dictionary<string, string>> getLocalConfigFunc = null)
        {
            return AddCentral(builder, new CentralConfigurationSource()
            {
                Ip = ip,
                Mac = mac,
                Url = url,
                GetLocalConfigFunc = getLocalConfigFunc
            });
        }

        public static IConfigurationBuilder AddCentral(this IConfigurationBuilder builder, CentralConfigurationSource configureSource)
            => builder.Add(configureSource);
    }
}
