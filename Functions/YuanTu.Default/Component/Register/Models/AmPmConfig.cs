using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;

namespace YuanTu.Default.Component.Register.Models
{
    public class AmPmConfig
    {
        public string Key { get; set; }
        public int Order { get; set; }
        public bool Enable { get; set; }
        public bool Visiable { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public Uri ImageUri { get; set; }
        public Color Color { get; set; }
        public string Remark { get; set; }

        public static List<InfoType> GetInfoTypes(
            IConfigurationManager config,
            IResourceEngine resource,
            string prefix,
            DelegateCommand<Info> command,
            Action<AmPmConfig> filter = null)
        {
            return config.GetValues("AmPmSession")
                .Select(s =>
                {
                    var p = Parse(config, resource, s.Path, s.Key);
                    filter?.Invoke(p);
                    return p;
                })
                .Where(p => p.Enable)
                .OrderBy(p => p.Order)
                .Select(p => new InfoType
                {
                    Title = p.Key,
                    ConfirmCommand = command,
                    IconUri = p.ImageUri,
                    Tag = p,
                    Color = p.Color,
                    Remark = p.Remark,
                })
                .ToList();
        }

        public static AmPmConfig Parse(IConfigurationManager config, IResourceEngine resource, string prefix, string key)
        {
            return new AmPmConfig
            {
                Key = key,
                Order = config.GetValueInt($"{prefix}:Order"),
                Enable = config.GetValueInt($"{prefix}:Enable") == 1,
                StartTime = config.GetValue($"{prefix}:StartTime"),
                EndTime = config.GetValue($"{prefix}:EndTime"),
                Visiable = config.GetValueInt($"{prefix}:Visiable") == 1,
                ImageUri = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName")),
                Color = config.GetValueColor($"{prefix}:Color"),
                Remark = config.GetValue($"{prefix}:Remark"),
            };
        }
    }
}