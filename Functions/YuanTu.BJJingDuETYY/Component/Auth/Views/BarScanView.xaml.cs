using System.Windows;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.BJJingDuETYY.Component.Auth.Views
{
    /// <summary>
    ///     CardView.xaml 的交互逻辑
    /// </summary>
    public partial class BarScanView : ViewsBase
    {
        public BarScanView()
        {
            InitializeComponent();
        }
        private void CardView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtCardNo.Focus();
        }
    }
}