using System.Threading;
using System.Windows;
using YuanTu.Default.Tablet.Component;

namespace YuanTu.Default.Tablet
{
    internal class MainWindowViewModel : Default.MainWindowViewModel
    {
        public override void OnViewInit()
        {
            base.OnViewInit();

            var window = View as Window;
            if (window != null)
                window.Visibility = Visibility.Hidden;
            var t = new Thread(() =>
            {
                var bubble = new FloatingBubble
                {
                    DataContext = new FloatingBubbleViewModel()
                };
                bubble.ShowDialog();
            })
            {
                IsBackground = true
            };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}