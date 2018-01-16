using System;
using System.Windows;
using System.Windows.Controls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ChongQingArea.Component.Auth.Views
{
    /// <summary>
    ///     FingerPrintView.xaml 的交互逻辑
    /// </summary>
    public partial class FingerPrintValidationView : ViewsBase
    {
        public FingerPrintValidationView()
        {
            InitializeComponent();
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = ((MediaElement)sender).Position.Add(TimeSpan.FromMilliseconds(1));
        }
    }
}