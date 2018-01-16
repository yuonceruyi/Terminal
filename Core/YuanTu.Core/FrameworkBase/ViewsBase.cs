using System;
using System.ComponentModel;
using System.Windows.Controls;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Core.FrameworkBase
{
    [ToolboxItem(false)]
    public abstract class ViewsBase : UserControl, IDependency
    {
        public ViewsBase()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Resources.Source = new Uri("pack://application:,,,/YuanTu.Default.Theme;component/default.xaml");
            }
        }
    }
}