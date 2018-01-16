using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.UserControls
{
    public class SimpleButton : Button
    {
        public static readonly DependencyProperty TagStringProperty = DependencyProperty.Register(
            nameof(TagString), typeof(string), typeof(SimpleButton), new PropertyMetadata(default(string)));

        public string TagString
        {
            get { return (string)GetValue(TagStringProperty); }
            set { SetValue(TagStringProperty, value); }
        }

        public static readonly DependencyProperty CanMultiClickProperty = DependencyProperty.Register(
            nameof(CanMultiClick), typeof(bool), typeof(SimpleButton), new PropertyMetadata(default(bool)));

        public bool CanMultiClick
        {
            get { return (bool) GetValue(CanMultiClickProperty); }
            set { SetValue(CanMultiClickProperty, value); }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (e.ClickCount > 1 && !CanMultiClick)
                return;

            var shell = ServiceLocator.Current.GetInstance<IShell>();
            if (shell?.IsTransitioning ?? false)
                return;

            OnClick();
        }
    }
}