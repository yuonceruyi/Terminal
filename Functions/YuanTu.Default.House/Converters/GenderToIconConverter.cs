using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.LocalResourceEngine;

namespace YuanTu.Default.House.Converters
{
    [ValueConversion(typeof(string), typeof(Uri))]
    internal class GenderToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;
            var s = value.ToString();
            if (s == "男")
                return ServiceLocator.Current.GetInstance<ResourceEngine>().GetImageResourceUri("头像男");
            if (s == "女")
                return ServiceLocator.Current.GetInstance<ResourceEngine>().GetImageResourceUri("头像女");
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}