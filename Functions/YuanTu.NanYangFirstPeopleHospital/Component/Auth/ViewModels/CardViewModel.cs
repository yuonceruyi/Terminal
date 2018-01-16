using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Prism.Commands;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Devices.CardReader;
using YuanTu.NanYangFirstPeopleHospital.Component.Auth.Dialog.Views;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        private string _cardNo;
        private string _password;
        private bool _showPassword;

        private string _tips;

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            PasswordCancelCommand = new DelegateCommand(() =>
            {
                ShowPassword = false;
                Preview();
            });
            
            PasswordConfirmCommand = new DelegateCommand(PasswordConfirm);
        }

        #region binding
        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (value?.Length >= 6)
                {
                    _password = value;
                    PasswordConfirm();
                    return;
                }
                _password = value;
                OnPropertyChanged();
            }
        }

        public string CardNo
        {
            get { return _cardNo; }
            set
            {
                _cardNo = value;
                OnPropertyChanged();
            }
        }

        public bool ShowPassword
        {
            get { return _showPassword; }
            set
            {
                _showPassword = value;
                if (value)
                {
                    Password = null;
                    ShowMask(true, new Password {DataContext = this});
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        public ICommand PasswordCancelCommand { get; set; }
        public ICommand PasswordConfirmCommand { get; set; }
#endregion

        protected override void StartRead()
        {
            Task.Run(() => StartMag());
        }

        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效");
                StartRead();
                return;
            }
            CardNo = cardNo;
            //todo 输入密码
            Tips = "请输入密码(默认密码: 6个 6)";

            View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
            {
                ShowPassword = true;
            }));

        }

        protected virtual void PasswordConfirm()
        {
            if (Password.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "密码不能为空");
                return;
            }
            ShowPassword = false;
            DoCommand(ctx =>
            {
                CardModel.CardType = Consts.Enums.CardType.就诊卡;
                PatientModel.Req病人信息查询 = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = CardNo,
                    cardType = ((int) CardModel.CardType).ToString(),
                    extend = Password
                };
                PatientModel.Res病人信息查询 = DataHandlerEx.病人信息查询(PatientModel.Req病人信息查询);

                if (PatientModel.Res病人信息查询.success)
                {
                    if (PatientModel.Res病人信息查询.data == null || PatientModel.Res病人信息查询.data.Count == 0)
                    {
                        ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)");
                        StartRead();
                        return;
                    }
                    CardModel.CardNo = CardNo;
                    Next();
                }
                else
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: PatientModel.Res病人信息查询.msg);
                    StartRead();
                }
            });
        }

        protected override void StopRead()
        {
            StopMag();
        }
    }
}