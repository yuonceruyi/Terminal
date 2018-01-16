using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(GridLength[]), typeof(string))]
    public class GridLengthConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            if (string.IsNullOrWhiteSpace(val))
            {
                return null;
            }
            var arr = val.Split(' ');
            GridLength[] grid = arr.Select(n => new GridLength(double.Parse(n),GridUnitType.Star)).ToArray();
            return grid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
