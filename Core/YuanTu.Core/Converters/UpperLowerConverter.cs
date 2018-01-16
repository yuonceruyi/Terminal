using System;
using System.Globalization;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    public class UpperLowerConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            var s = values[0] as string;
            var toLower = (bool) values[1];
            return toLower ? s.ToLower() : s.ToUpper();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}