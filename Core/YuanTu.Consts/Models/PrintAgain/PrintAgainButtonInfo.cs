using System.Windows.Media;
using YuanTu.Consts.Enums;

namespace YuanTu.Consts.Models.PrintAgain
{
    public class PrintAgainButtonInfo
    {
        public string Name { get; set; }
        public PrintAgainTypeEnum PrintAgainType { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}