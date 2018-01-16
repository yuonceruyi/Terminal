using System;
using System.Globalization;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class IDNoFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string) value;
            if (val?.Length > 14)
                val = $"{val.Substring(0, 6)} {val.Substring(6, 4)} {val.Substring(10, 4)} {val.Substring(14, val.Length - 14)}";
            else if (val?.Length > 10)
                val = $"{val.Substring(0, 6)} {val.Substring(6, 4)} {val.Substring(10, val.Length - 10)}";
            else if (val?.Length > 6)
                val = $"{val.Substring(0, 6)} {val.Substring(6, val.Length - 6)}";
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string) value;
            return val;
        }
    }
}