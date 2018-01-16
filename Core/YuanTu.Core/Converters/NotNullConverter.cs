using System;
using System.Globalization;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return (value as string).Length > 0;
            }
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}