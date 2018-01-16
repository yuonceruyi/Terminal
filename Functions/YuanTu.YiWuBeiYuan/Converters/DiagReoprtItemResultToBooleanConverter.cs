using System;
using System.Globalization;
using System.Windows.Data;

namespace YuanTu.YiWuBeiYuan.Converters
{
    class DiagReoprtItemResultToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == "正常";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
