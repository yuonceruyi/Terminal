using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using YuanTu.Core.Extension;
namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class UnitFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            val = val?.SafeToSplit('/')[0];
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string)value;
            return val;
        }
    }
}
