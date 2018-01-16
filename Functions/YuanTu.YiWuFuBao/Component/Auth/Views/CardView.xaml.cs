﻿using System;
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
using YuanTu.Core.FrameworkBase;

namespace YuanTu.YiWuFuBao.Component.Auth.Views
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

        private void CardView_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtCardNo.Focus();
        }
    }
}
