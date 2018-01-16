using YuanTu.Core.FrameworkBase;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Auth.Views
{
    /// <summary>
    /// QrCodeView.xaml 的交互逻辑
    /// </summary>
    public partial class QrCodeView : ViewsBase
    {
        public QrCodeView()
        {
            InitializeComponent();
        }

        private void ViewsBase_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.txtPatientId.Focus();
        }
    }
}
