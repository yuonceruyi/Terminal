using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Default;

namespace YuanTu.YuHangZYY
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
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
