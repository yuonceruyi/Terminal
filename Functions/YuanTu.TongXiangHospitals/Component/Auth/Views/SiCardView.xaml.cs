using System;
using System.Windows;
using System.Windows.Controls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.TongXiangHospitals.Component.Auth.Views
{
    /// <summary>
    /// CardView.xaml 的交互逻辑
    /// </summary>
    public partial class SiCardView : ViewsBase
    {
        public SiCardView()
        {
            InitializeComponent();
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((MediaElement)sender).Position = ((MediaElement)sender).Position.Add(TimeSpan.FromMilliseconds(1));
        }
        //private void wb_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    var dom = (mshtml.IHTMLDocument2)wb.Document;

        //    dom.body.style.overflow = "hidden";
        //    dom.body.style.border = "none";

        //    dom.body.style.padding = "0";
        //    dom.body.style.margin = "0";
        //    dom.body.setAttribute("scroll", "no");
        //    dom.body.setAttribute("frameborder", "no");

        //}

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {

            //var dom = (mshtml.IHTMLDocument2)wb.Document;

            //dom.body.style.overflow = "hidden";
            //dom.body.style.border = "none";

            //dom.body.style.padding = "0";
            //dom.body.style.margin = "0";
            //dom.body.setAttribute("scroll", "no");
            //dom.body.setAttribute("frameborder", "no");
            //wb.Refresh();
            //pb.ImageLocation = @"E:\浙江项目\Terminal\bin\Debug\Resource\190\Images\插社保卡.gif";
        }
    }
}
