using System.Windows.Media;

namespace YuanTu.Default.Tablet.Component.Cashier.Models
{
    public class CashierButtonInfo
    {
        public string Name { get; set; }
        public CashierTypeEnum Type { get; set; }
        public int Order { get; set; }
        public bool Visabled { get; set; }
        public bool IsEnabled { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}