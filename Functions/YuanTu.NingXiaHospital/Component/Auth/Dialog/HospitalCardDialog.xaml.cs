using System.Windows.Controls;
using System.Windows.Media;

namespace YuanTu.NingXiaHospital.Component.Auth.Dialog
{
    /// <summary>
    ///     HospitalCardDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HospitalCardDialog : UserControl
    {
        public HospitalCardDialog()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //txtCardNo.Focus();
        }
    }
}