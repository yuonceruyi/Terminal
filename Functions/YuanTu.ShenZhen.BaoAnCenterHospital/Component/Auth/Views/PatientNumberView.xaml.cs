using System;
using System.Windows.Threading;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.Views
{
    /// <summary>
    /// PatientNumberView.xaml 的交互逻辑
    /// </summary>
    public partial class PatientNumberView : ViewsBase
    {
        public PatientNumberView()
        {
            InitializeComponent();
        }

        private void ViewsBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //this.txtPatientId.Focus();
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => txtPatientId.Focus()));
        }

        
    }
}
