using System;
using System.Globalization;
using System.Windows.Data;

namespace YuanTu.FuYangRMYY.Converters
{
    class DiagReoprtItemMarkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string r = "";
            switch (value?.ToString())
            {
                case "1":r = "偏高";break;
                case "-1": r = "偏低"; break;
                case "0": r = ""; break;
                default:r = value?.ToString()+"fff";break;
            }
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
