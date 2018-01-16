using System.Windows;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.Tablet.Component.Cashier.Views
{
    /// <summary>
    /// CardView.xaml 的交互逻辑
    /// </summary>
    public partial class ScanView : ViewsBase
    {
        public ScanView()
        {
            InitializeComponent();
            
        }

        private void ScanView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextBoxInput.Focus();
        }
    }
}
