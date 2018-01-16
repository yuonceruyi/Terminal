using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;

namespace YuanTu.Consts.Dtos
{
    public class PayMethodDto
    {
        public PayMethod PayMethod { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Uri ImageSource { get; set; }

        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public string DisableText { get; set; }
        public Color Color { get; set; }

        public string Hint { get; set; }
        public string Template { get; set; }

        public static List<InfoPay> GetInfoPays(
            IConfigurationManager config, 
            IResourceEngine resource,
            string prefix, 
            DelegateCommand<Info> command, 
            Action<PayMethodDto> filter = null)
        {
            return Enum.GetValues(typeof(PayMethod))
                .Cast<PayMethod>()
                .Select(m =>
                {
                    var p = Parse(config, resource, $"{prefix}:{m}", m);
                    filter?.Invoke(p);
                    return p;
                })
                .Where(p => p.Visabled)
                .OrderBy(p => p.Order)
                .Select(p =>
                {
                    var infoPay = new InfoPay
                    {
                        Title = p.Name,
                        ConfirmCommand = command,
                        IconUri = p.ImageSource,
                        Tag = p.PayMethod,
                        Color = p.Color,
                        IsEnabled = p.IsEnabled,
                        ShowHint = !string.IsNullOrEmpty(p.Hint),
                        HintText = p.Hint,
                        TemplateKey = p.Template,
                    };
                    if (!string.IsNullOrEmpty(p.DisableText))
                        infoPay.DisableText = p.DisableText;
                    return infoPay;
                })
                .ToList();
        }

        public static PayMethodDto Parse(IConfigurationManager config, IResourceEngine resource, string prefix, PayMethod payMethod)
        {
            Color color;
            var value = config.GetValue($"{prefix}:Color");
            if (string.IsNullOrEmpty(value))
            {
                color = new Color();
            }
            else
            {
                var rgb = value.Split(',');
                color = Color.FromRgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            }
            var model = new PayMethodDto
            {
                PayMethod = payMethod,
                Name = config.GetValue($"{prefix}:Name") ?? "未定义",
                Order = config.GetValueInt($"{prefix}:Order"),
                IsEnabled = config.GetValue($"{prefix}:IsEnabled") == "1",
                ImageSource = resource.GetImageResourceUri(config.GetValue($"{prefix}:ImageName")),
                Color = color,
                Visabled = config.GetValue($"{prefix}:Visabled") == "1",
                Hint = config.GetValue($"{prefix}:Hint"),
                DisableText = config.GetValue($"{prefix}:DisableText"),
                Template = config.GetValue($"{prefix}:Template"),
            };
            return model;
        }
    }
}