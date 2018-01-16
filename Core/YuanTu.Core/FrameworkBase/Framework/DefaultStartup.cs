using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;

namespace YuanTu.Core.FrameworkBase
{
    public abstract class DefaultStartup : IStartup
    {
        public virtual int Order => int.MaxValue / 2;

        public virtual string[] UseConfigPath()
        {
            return null;
        }

        public virtual bool RegisterTypes(ViewCollection collection)
        {
            return false;
        }

        public virtual void InitConfig(IConfiguration root)
        {
        }

        public virtual void AfterStartup()
        {
        }

        public virtual Dictionary<string, string[]> GetStrategy()
        {
            return DeviceType.FallBackToDefaultStrategy;
        }

        public virtual List<Uri> GetResourceDictionaryUris()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var themeUri = config.GetValue("ThemeUri");
            
            if (string.IsNullOrWhiteSpace(themeUri))
                return new List<Uri>();

            var themeUriLower = themeUri.ToLower();
            var themes = Enum.GetValues(typeof(Theme));
            foreach (Theme theme in themes)
                if (themeUriLower.Contains(theme.ToString().ToLower()))
                {
                    FrameworkConst.CurrentTheme = theme;
                    return new List<Uri> { new Uri(theme.GetEnumDescription())};
                }

            return new List<Uri> {new Uri(Theme.Default.GetEnumDescription())};
        }

        public virtual string CurrentStrategyType()
        {
            var stg = GetStrategy();
            if (stg.ContainsKey(FrameworkConst.DeviceType))
                return stg[FrameworkConst.DeviceType].First();
            return "";
        }
    }
}