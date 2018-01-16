using System.Linq.Expressions;
using System.Windows;
using YuanTu.Core.MultipScreen;

namespace YuanTu.Default
{
    /// <summary>
    /// AdWindow.xaml 的交互逻辑
    /// </summary>
    [MultipScreen(Index = 1, FullScreen = true)]
    public partial class AdWindow : Window
    {
        public AdWindow()
        {
            InitializeComponent();
        }
    }
}