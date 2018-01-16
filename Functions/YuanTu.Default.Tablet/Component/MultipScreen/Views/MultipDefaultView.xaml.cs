using System.Windows;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Core.MultipScreen;
using YuanTu.Default.Tablet.Component.MultipScreen.Models;

namespace YuanTu.Default.Tablet.Component.MultipScreen.Views
{
    /// <summary>
    ///     MultipUserInfoView.xaml 的交互逻辑
    /// </summary>
    [MultipScreen(Index = 1, FullScreen = true)]
    public partial class MultipDefaultView : Window
    {
        public MultipDefaultView()
        {
            InitializeComponent();
            DataContext = ServiceLocator.Current.GetInstance<IMultipModel>();
        }
    }
}