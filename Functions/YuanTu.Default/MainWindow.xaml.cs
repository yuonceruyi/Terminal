using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;

namespace YuanTu.Default
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IShell
    {
        public MainWindow()
        {
            InitializeComponent();
            _helper = new MainWindowHelper(this);

#if !DEBUG
            this.WindowState = WindowState.Maximized;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 0;
            this.Left = 0;
#else
            this.ToBestScreen();
#endif
        }

        private MainWindowHelper _helper;

        #region Implementation of IShell

        public Grid Mask => MaskContent;

        public bool IsTransitioning => TransitionPresenter.IsTransitioning;

        #endregion Implementation of IShell

        private void MainWindow_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = (DataContext as MainWindowViewModel);
            if (vm == null)
                return;
            var p = e.GetPosition(this);
            Canvas.SetLeft(Ellipse, (p.X - Ellipse.Width / 2) / vm.ScaleX);
            Canvas.SetTop(Ellipse, (p.Y - Ellipse.Height / 2) / vm.ScaleY);
            vm.Click = false;
            vm.Click = true;
        }
    }
}