using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;

namespace YuanTu.Consts.Models
{
    public enum AlertHideType
    {
        ButtonClick,
        TimeOut
    }
    public class AlertModel : BindableBase
    {
        private string _content;
        private int _countDown;
        private string _countDownString;
        private bool _display;
        private BitmapImage _image;
        private Guid _seed = Guid.Empty;
        private string _title;
        private string _extrContent;

        public AlertModel()
        {
            Command = new DelegateCommand(() => { Display = false; HideAction?.Invoke(AlertHideType.ButtonClick); });
        }

        public int CountDown
        {
            get { return _countDown; }
            set
            {
                _countDown = value;
                var k = _seed = Guid.NewGuid();
                CountDownString = "";
                if (value > 0)
                {
                    Task.Run(() =>
                    {
                        while (Display && (k == _seed))
                        {
                            if (--_countDown <= 0)
                            {
                                //if (Command.CanExecute())
                                //{
                                //    Command.Execute();
                                //}
                                Display = false;
                                HideAction?.Invoke(AlertHideType.TimeOut);
                                break;
                            }
                            CountDownString = $"{CountDown.ToString("D2")}";
                            Thread.Sleep(1000);
                        }
                    });
                }
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

        /// <summary>
        ///     控制是否显示弹窗
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

        /// <summary>
        ///     弹窗标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     弹窗图标
        /// </summary>
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     弹窗内容
        /// </summary>
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 附加内容(可显示原始错误信息)
        /// </summary>
        public string ExtrContent
        {
            get { return _extrContent; }
            set
            {
                _extrContent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     点击按钮事件
        /// </summary>
        public DelegateCommand Command { get; set; }
        /// <summary>
        /// 弹窗消失后触发事件
        /// </summary>
        public Action<AlertHideType> HideAction { get; set; }
    }

    public class AlertExModel
    {
        public Action<AlertHideType> HideCallback { get; set; }
    }
}