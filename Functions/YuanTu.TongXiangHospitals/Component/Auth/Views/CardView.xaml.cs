﻿using System;
using System.Windows;
using System.Windows.Controls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.TongXiangHospitals.Component.Auth.Views
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

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = ((MediaElement)sender).Position.Add(TimeSpan.FromMilliseconds(1));
        }
    }
}
