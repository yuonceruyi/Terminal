using System.Windows;
using System.Windows.Controls;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;

namespace YuanTu.Default.House
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IShell
    {
        public MainWindow()
        {
            InitializeComponent();
            _helper = new Default.MainWindowHelper(this);
#if !DEBUG
            this.WindowState = WindowState.Maximized;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 0;
            this.Left = 0;
#else
            this.ToBestScreen();
#endif
           
        }

        private Default.MainWindowHelper _helper;

        #region Implementation of IShell

        public Grid Mask => MaskContent;
        public bool IsTransitioning => TransitionPresenter.IsTransitioning;

        #endregion Implementation of IShell
    }
}