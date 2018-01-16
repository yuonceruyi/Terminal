using System.ComponentModel;
namespace YuanTu.Consts.Enums
{

    public enum Theme
    {

        [Description("pack://application:,,,/YuanTu.Default.Theme;component/YellowBlue.xaml")]
        YellowBlue,
        [Description("pack://application:,,,/YuanTu.Default.Theme;component/default.xaml")]
        Default,
        [Description("pack://application:,,,/YuanTu.Default.Clinic;component/Theme/default.xaml")]
        ClinicDefault,
        [Description("pack://application:,,,/YuanTu.Default.House;component/Theme/default.xaml")]
        HouseDefault

    }
}
