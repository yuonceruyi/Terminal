using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.LocalResourceEngine;

namespace YuanTu.Default.House.Converters
{
   
        [ValueConversion(typeof(string), typeof(Uri))]
        internal class BoolToColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var v= (bool) value;
                return v ? GetColorBrush("#1b9ef5"):GetColorBrush("#e9e9e9");
              
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        public virtual SolidColorBrush GetColorBrush(string text)
        {
            var brushConverter = new BrushConverter();
            return (SolidColorBrush)brushConverter.ConvertFromString(text);
        }
    }
    
}
