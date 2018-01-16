using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models
{
    public class BusyModel : BindableBase
    {
        private bool _isBusy;
        private string _busyContent;
        private FrameworkElement _busyMutiContent;
        private string _extraMessage;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string BusyContent
        {
            get { return _busyContent; }
            set
            {
                _busyContent = value;
                OnPropertyChanged();
            }
        }

        public string ExtraMessage
        {
            get { return _extraMessage; }
            set
            {
                _extraMessage = value?? "请耐心等待...";
                OnPropertyChanged();
            }
        }

        public FrameworkElement BusyMutiContent
        {
            get { return _busyMutiContent; }
            set
            {
                _busyMutiContent = value;
                var shell = ServiceLocator.Current.GetInstance<IShell>() as Window;
                shell?.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
                {
                    var content = shell.FindName("LoadingContent") as ContentControl;
                    if (content != null)
                    {
                        content.Content = value;
                    }
                }));
            }
        }
    }
}