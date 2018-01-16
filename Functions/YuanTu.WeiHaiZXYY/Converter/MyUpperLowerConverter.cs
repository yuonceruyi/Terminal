using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace YuanTu.WeiHaiZXYY.Converter
{
    public class MyUpperLowerConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            var s = values[0] as string;

            if (IsNum(s))
            {
                return s;
            }
            return s.ToUpper();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private bool IsNum(string no)
        {
            var resource = new [] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
            return resource.Contains(no);
        }
    }
}
