using System.Windows.Controls;
using System.Windows;
using YuanTu.Core.FrameworkBase;
using System;

namespace YuanTu.BJJingDuETYY.Component.Tools.Views
{
    /// <summary>
    /// SiPayView.xaml 的交互逻辑
    /// </summary>
    public partial class SiPayView : ViewsBase
    {
        public SiPayView()
        {
            InitializeComponent();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = ((MediaElement)sender).Position.Add(TimeSpan.FromMilliseconds(1));
        }
    }
}
