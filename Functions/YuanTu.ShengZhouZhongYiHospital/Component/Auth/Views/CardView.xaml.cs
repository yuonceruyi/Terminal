using System.Windows;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Auth.Views
{
    /// <summary>
    /// CardView.xaml 的交互逻辑
    /// </summary>
    public partial class CardView : ViewsBase
    {
        public CardView()
        {
            InitializeComponent();
            
        }

        private void CardView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtCardNo.Focus();
        }
    }
}
