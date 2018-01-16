using System.Linq;
using YuanTu.Consts.Models.Configs;
using YuanTu.Consts.Services;
using YuanTu.Core.Infrastructure;

namespace YuanTu.Core.Services.ConfigService
{
    public sealed class ConfigurationManager : IConfigurationManager
    {
        public string ServiceName => "获取配置信息";
        public string GetValue(string key)
        {
            return SystemStartup.Configuration.GetSection(key).Value;
            //return SystemStartup.Configuration[key];//效果一样
        }

        public Section[] GetValues(string key)
        {
            return SystemStartup.Configuration.GetSection(key)
                .GetChildren()
                .Select(p => new Section(p.Key, p.Path, p.Value))
                .ToArray();
        }

    }
}