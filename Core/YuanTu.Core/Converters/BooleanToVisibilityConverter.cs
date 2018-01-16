using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return value is bool && (bool) value ? Visibility.Visible : Visibility.Collapsed;
            }
            return value is bool && (bool) value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility) value == Visibility.Visible;
        }
    }
}