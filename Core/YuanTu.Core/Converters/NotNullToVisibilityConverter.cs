using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var stringValue = value as string;
                if (stringValue != null)
                {
                    if (!string.IsNullOrWhiteSpace(stringValue))
                        return Visibility.Visible;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            return parameter == null ? Visibility.Collapsed : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}