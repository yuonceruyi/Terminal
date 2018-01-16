using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.Default;

namespace YuanTu.ZheJiangZhongLiuHospital
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
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