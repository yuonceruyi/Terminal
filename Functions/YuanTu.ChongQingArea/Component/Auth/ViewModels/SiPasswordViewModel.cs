using System;
using System.Threading;
using System.Windows.Threading;
using Prism.Commands;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    internal class SiPasswordViewModel : ViewModelBase
    {
        public SiPasswordViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
            SkipCommand = new DelegateCommand(Skip);
            CancelCommand = new DelegateCommand(Cancel);
            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += (s, a) =>
            {
                CountDown--;
                if (CountDown > 0)
                    return;
                timer.Stop();
                Cancel();
            };
            _timer = timer;
            _timer.Start();
        }

        private readonly DispatcherTimer _timer;

        public override string Title => "社保卡密码";


        public SiContext SiContext { get; set; }


        public ManualResetEvent ManualResetEvent { get; set; }

        public DelegateCommand ConfirmCommand { get; set; }

        public DelegateCommand SkipCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }

        private void Confirm()
        {
            if (string.IsNullOrEmpty(Password) || Password.Length != 6)
            {
                ErrorMessage = "请输入6位社保卡密码";
                return;
            }
            SiContext.社保卡密码 = Password;
            SiContext.InputPWD = 2;

            _timer.Stop();
            ManualResetEvent.Set();
        }

        private void Skip()
        {
            SiContext.社保卡密码 = "123456";
            SiContext.InputPWD = 2;

            _timer.Stop();
            ManualResetEvent.Set();
        }
        private void Cancel()
        {
            SiContext.InputPWD = -1;
            SiContext.CloseInputWindow();
            //Logger.Main.Info($"取消密码 密码状态{SiContext.InputPWD.ToString()}=>-1");
            _timer.Stop();
            ManualResetEvent.Set();
            ShowMask(false);
        }

        #region Bindings

        private string _hint = "请输入6位社保卡密码";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        private int _countDown = 30;

        public int CountDown
        {
            get { return _countDown; }
            set
            {
                _countDown = value;
                CancelText = $"取消 ({CountDown})";
                OnPropertyChanged();
            }
        }

        private string _skipText = "跳过";

        public string SkipText
        {
            get { return _skipText; }
            set
            {
                _skipText = value;
                OnPropertyChanged();
            }
        }

        private string _cancelText = "取消";

        public string CancelText
        {
            get { return _cancelText; }
            set
            {
                _cancelText = value;
                OnPropertyChanged();
            }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                PasswordMasked = new string('*', Password?.Length ?? 0);
                OnPropertyChanged();
            }
        }

        private string _passwordMasked;

        public string PasswordMasked
        {
            get { return _passwordMasked; }
            set
            {
                _passwordMasked = value;
                OnPropertyChanged();
            }
        }

        private string _tipContent = "若为默认密码123456可点击跳过按钮";

        public string TipContent
        {
            get { return _tipContent; }
            set
            {
                _tipContent = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        #endregion Bindings
    }
}