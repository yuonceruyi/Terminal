using System.Windows.Controls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.Default.House.Component.HealthDetection.Views
{
    /// <summary>
    /// ReportView.xaml 的交互逻辑
    /// </summary>
    public partial class ReportView : ViewsBase
    {
        public ReportView()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid)sender).UnselectAll();
        }
    }
}
