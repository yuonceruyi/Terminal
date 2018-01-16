using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;
using YuanTu.Consts.Enums;

namespace YuanTu.Consts.Models
{
    public class ChoiceButtonInfo
    {
        public string Name { get; set; }
        public Business ButtonBusiness { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public ImageSource ImageSource { get; set; }
        public string DisableText { get; set; } = "即将上线";
        public List<ChoiceButtonInfo> SubModules=new List<ChoiceButtonInfo>();
        public string BusinessTest { get; set; }
        // public string EnumKey { get; set; }
    }
}