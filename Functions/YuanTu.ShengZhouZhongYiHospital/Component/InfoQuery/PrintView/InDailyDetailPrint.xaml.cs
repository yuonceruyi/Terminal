using System.Windows;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.ShengZhouZhongYiHospital.Component.InfoQuery.PrintView
{
    /// <summary>
    /// BillDetailPrint.xaml 的交互逻辑
    /// </summary>
    public partial class InDailyDetailPrint : Window
    {
        public InDailyDetailPrint(string printerName)
        {
            InitializeComponent();
            this._printerName = printerName;
        }

        private readonly string _printerName;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("发到打印机?", "", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
                new PrintHelperEx(_printerName).Print(doc, "检验报告");
            else
                new PrintHelperEx("XPS").Print(doc, "检验报告");
        }
    }
}
