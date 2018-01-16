using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace YuanTu.Core.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringFormatConverter : IValueConverter
    {
        static char[] seperators = new[] { ' ', ',' };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (string.IsNullOrEmpty(s))
                return s;
            var lengthsString = parameter as string;
            if (lengthsString == null)
                return value;
            var lengths = lengthsString.Split(seperators, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray();
            int p = 0;
            var sb = new StringBuilder();
            var valLength = s.Length;
            foreach (int length in lengths)
            {
                if(valLength >= p + length)
                {
                    sb.Append(s.Substring(p, length));
                }
                else
                {
                    sb.Append(s.Substring(p));
                    break;
                }
                p += length;
                if (valLength > p)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (string) value;
            return val;
        }
    }
}