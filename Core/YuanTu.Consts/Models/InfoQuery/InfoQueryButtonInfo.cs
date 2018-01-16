using System.Windows.Media;
using YuanTu.Consts.Enums;

namespace YuanTu.Consts.Models.InfoQuery
{
    public class InfoQueryButtonInfo
    {
        public string Name { get; set; }
        public InfoQueryTypeEnum InfoQueryType { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public ImageSource ImageSource { get; set; }

        public string DisableText { get; set; } = "即将上线";
    }
}