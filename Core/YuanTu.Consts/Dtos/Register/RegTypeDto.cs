using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;

namespace YuanTu.Consts.Dtos.Register
{
    public class RegTypeDto
    {
        public RegType RegType { get; set; }
        public string Name { get; set; }

        public Uri ImageSource { get; set; }
        public Color Color { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public string Remark { get; set; }
        public bool SearchDoctor { get; set; }

        public static List<InfoType> GetInfoTypes(
            IConfigurationManager config,
            IResourceEngine resource,
            string prefix,
            DelegateCommand<Info> command,
            Action<RegTypeDto> filter = null)
        {
            return Enum.GetNames(typeof(RegType))
                .Select(m =>
                {
                    var p = Parse(config, resource, $"{prefix}:{m}", m);
                    filter?.Invoke(p);
                    return p;
                })
                .Where(p => p.Visabled)
                .OrderBy(p => p.Order)
                .Select(p => new InfoType
                {
                    Title = p.Name,
                    ConfirmCommand = command,
                    IconUri = p.ImageSource,
                    Tag = p,
                    Color = p.Color,
                    Remark = p.Remark,
                })
                .ToList();
        }

        public static RegTypeDto Parse(IConfigurationManager config, IResourceEngine resource, string prefix, string regType)
        {
            RegType b;
            Enum.TryParse(regType, out b);
            var model = new RegTypeDto
            {
                RegType = b,
                Name = config.GetValue($"{prefix}:Name") ?? "未定义",
                Order = config.GetValueInt($"{prefix}:Order"),
                ImageSource = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName")),
                Color = config.GetValueColor($"{prefix}:Color"),
                Visabled = config.GetValue($"{prefix}:Visabled") == "1",
                Remark = config.GetValue($"{prefix}:Remark"),
                SearchDoctor = config.GetValueInt($"{prefix}:SearchDoctor") == 1,
            };
            return model;
        }
    }
}
