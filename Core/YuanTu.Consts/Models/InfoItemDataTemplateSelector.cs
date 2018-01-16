using System.Windows;
using System.Windows.Controls;

namespace YuanTu.Consts.Models
{
    public class InfoItemDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element != null)
            {
                var key = (item as Info)?.TemplateKey;
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return element.FindResource(key) as DataTemplate;
                }
                if (item is InfoType)
                {
                    return element.FindResource("InfoItemType") as DataTemplate;
                }
                if (item is InfoCard)
                {
                    return element.FindResource("InfoItemCard") as DataTemplate;
                }
                if (item is InfoMore)
                {
                    return element.FindResource("InfoItemMore") as DataTemplate;
                }
                if (item is InfoIcon)
                {
                    return element.FindResource("InfoItemIcon") as DataTemplate;
                }
                if (item is InfoAppt)
                {
                    return element.FindResource("InfoItemAppt") as DataTemplate;
                }
                if (item is InfoDoc)
                {
                    return element.FindResource("InfoItemDoc") as DataTemplate;
                }
                if (item is InfoTime)
                {
                    return element.FindResource("InfoItemTime") as DataTemplate;
                }
                if (item is InfoHouseReport)
                {
                    return element.FindResource("InfoItemHouseReport") as DataTemplate;
                }
                if (item is InfoHospital)
                {
                    return element.FindResource("InfoItemHospital") as DataTemplate;
                }
                if (item is Info)
                {
                    return element.FindResource("InfoItemNone") as DataTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}