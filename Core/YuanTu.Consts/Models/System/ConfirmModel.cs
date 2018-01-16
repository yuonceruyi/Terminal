using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Mvvm;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;

namespace YuanTu.Consts.Models
{
    public class ConfirmModel : BindableBase
    {
        private ConfirmExModel _confirmEx;
        private int _countDown;
        private FrameworkElement _mutiContent;
        private Guid _seed = Guid.Empty;

        public ConfirmModel()
        {
            Command = new DelegateCommand<string>(p =>
            {
                Display = false;
                MutiContent = null;
                ClickResult?.Invoke(p == "0");
                ClickResult = null;
            });
        }

        public FrameworkElement MutiContent
        {
            get { return _mutiContent; }
            set
            {
                _mutiContent = value;
                var shell = ServiceLocator.Current.GetInstance<IShell>() as Window;
                shell?.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
                {
                    var content = shell.FindName("ConfirmContent") as ContentControl;
                    if (content != null)
                        content.Content = value;
                }));
            }
        }

        public ConfirmExModel ConfirmEx
        {
            get { return _confirmEx; }
            set
            {
                _confirmEx = value;
                OkText = _confirmEx?.OkContent;
                CancelText = _confirmEx?.CancelContent;
                Image = _confirmEx?.ImageSource;
            }
        }

        public int CountDown
        {
            get { return _countDown; }
            set
            {
                _countDown = value;
                var k = _seed = Guid.NewGuid();
                CountDownString = "";
                if (value <= 0)
                    return;
                Task.Run(() =>
                {
                    while (Display && (k == _seed))
                    {
                        if (--_countDown <= 0)
                        {
                            if (Command.CanExecute("1"))
                                Command.Execute("1");
                            break;
                        }
                        CountDownString = $"{CountDown:D2}";
                        Thread.Sleep(1000);
                    }
                });
            }
        }

        public DelegateCommand<string> Command { get; }

        public Action<bool> ClickResult { get; set; }

        #region Binding

        private bool _display;
        private string _title;
        private string _content;
        private BitmapImage _image;
        private string _okText;
        private string _cancelText;
        private string _countDownString;

        /// <summary>
        ///     是否显示弹窗
        /// </summary>
        public bool Display
        {
            get { return _display; }
            set
            {
                _display = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage Image
        {
            get { return _image; }
            private set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public string OkText
        {
            get { return _okText; }
            private set
            {
                _okText = value;
                OnPropertyChanged();
            }
        }

        public string CancelText
        {
            get { return _cancelText; }
            private set
            {
                _cancelText = value;
                OnPropertyChanged();
            }
        }

        public string CountDownString
        {
            get { return _countDownString; }
            private set
            {
                _countDownString = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }

    public class ConfirmExModel
    {
        private ConfirmExModel()
        {
        }

        public string OkContent { get; private set; }
        public string CancelContent { get; private set; }
        public BitmapImage ImageSource { get; set; }

        public static ConfirmExModel Build(string okContent = null, string cancelContent = null, bool showImg = true,
            BitmapImage img = null)
        {
            var model = new ConfirmExModel
            {
                OkContent = string.IsNullOrWhiteSpace(okContent) ? "确定" : okContent,
                CancelContent = string.IsNullOrWhiteSpace(cancelContent) ? "取消" : cancelContent,
                ImageSource = showImg
                        ? (img ?? ServiceLocator.Current.GetInstance<IResourceEngine>()?.GetImageResource("提示_感叹号"))
                        : null
            };
            return model;
        }
    }
}