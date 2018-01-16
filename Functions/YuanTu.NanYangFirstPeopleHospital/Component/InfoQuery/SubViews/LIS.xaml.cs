using System.Windows;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.NanYangFirstPeopleHospital.Component.InfoQuery.SubViews
{
    /// <summary>
    ///     Report.xaml 的交互逻辑
    /// </summary>
    public partial class LIS : Window
    {
        public LIS(IConfigurationManager configurationManager)
        {
            InitializeComponent();
            this._configurationManager = configurationManager;
        }

        private readonly IConfigurationManager _configurationManager;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var printerName = _configurationManager.GetValue("Printer:Laser");
            var r = MessageBox.Show("发到打印机?", "", MessageBoxButton.YesNo);
            if (r == MessageBoxResult.Yes)
                new PrintHelperEx(printerName).Print(doc, "检验报告");
            else
                new PrintHelperEx("XPS").Print(doc, "检验报告");
        }

        
    }
}