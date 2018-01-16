using System;
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
using YuanTu.Consts.Services;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.HuNanHangTianHospital.Component.InfoQuery.PrintView
{
    /// <summary>
    /// BillDetailPrint.xaml 的交互逻辑
    /// </summary>
    public partial class BillDetailPrint : Window
    {
        public BillDetailPrint(string printerName)
        {
            InitializeComponent();
            this._printerName = printerName;
        }

        private string _printerName;

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
