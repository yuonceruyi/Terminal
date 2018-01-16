using System;
using System.Globalization;
using System.Windows.Data;
using YuanTu.Core.Extension;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class AmountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return value.ToString().In元();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}