using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace YuanTu.ShangHaiZSYY
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IShell
    {
        private MainWindowHelper _helper;

        public MainWindow()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
                Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/YuanTu.Default.Theme;component/default.xaml")
                });
            _helper = new MainWindowHelper(this);
#if !DEBUG
            this.WindowState = WindowState.Maximized;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Top = 0;
            this.Left = 0;
#else
            this.ToBestScreen();
            //RowDefinition.Height = new GridLength(0);
#endif

        }

        public Grid Mask => MaskContent;
        public bool IsTransitioning => false;
    }
}
