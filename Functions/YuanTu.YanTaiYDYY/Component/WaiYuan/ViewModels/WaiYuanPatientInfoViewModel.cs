using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Create;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.YanTaiYDYY.Component.WaiYuan.Dialog;
using YuanTu.YanTaiYDYY.Component.WaiYuan.Models;
using UpdatePhone = YuanTu.YanTaiYDYY.Component.WaiYuan.Dialog.UpdatePhone;

namespace YuanTu.YanTaiYDYY.Component.WaiYuan.ViewModels
{
    public class WaiYuanPatientInfoViewModel : ViewModelBase
    {
        private string _newPhone;
        private string _newAddress;
        private string _newPassword;
        private string _newPasswordRepeat;
        private bool _passwordKeyboardShow;
        private string _pwdTips;

        [Dependency]
        public IWaiYuanModel WaiYuanModel { get; set; }
        public override string Title => "外院卡注册信息";
        public string Hint => "完善建档信息";
        public ICommand UpdateCommand { get; set; }
        public ICommand UpdateConfirmCommand { get; set; }
        public ICommand UpdateCancelCommand { get; set; }
        public ICommand ModifyAddressCommand { get; set; }
        public ICommand ConfirmCommand { get; set; }
        public ICommand UpdatePassword { get; set; }
        public ICommand NewPasswordChange { get; set; }
        public ICommand ConfirmNewPasswordCommand { get; set; }
        public ICommand CancelNewPasswordCommand { get; set; }

        [Dependency]
        public ICreateModel CreateModel { get; set; }

        public string PwdTips
        {
            get { return _pwdTips; }
            set { _pwdTips = value; OnPropertyChanged(); }
        }

        public string NewPhone
        {
            get { return _newPhone; }
            set { _newPhone = value; OnPropertyChanged(); }
        }

        public string NewAddress
        {
            get { return _newAddress; }
            set { _newAddress = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                PwdTips = "请输入6位数字密码";
                _newPassword = value;
                
                
                PlaySound(SoundMapping.请设置卡密码);
                OnPropertyChanged();
            }
        }

        public string NewPasswordRepeat
        {
            get { return _newPasswordRepeat; }
            set
            {
                PwdTips = "请重复输入6位数字密码";
                _newPasswordRepeat = value;
                OnPropertyChanged();
            }
        }

        public bool PasswordKeyboardShow
        {
            get { return _passwordKeyboardShow; }
            set { _passwordKeyboardShow = value; OnPropertyChanged(); }
        }

        public WaiYuanPatientInfoViewModel()
        {
            UpdateCommand = new DelegateCommand(ShowUpdatePhone);
            UpdateCancelCommand = new DelegateCommand(CancelUpdatePhone);
            UpdateConfirmCommand = new DelegateCommand(ConfirmUpdatePhone);
            ModifyAddressCommand = new DelegateCommand(ShowModifyAddress);
            UpdatePassword = new DelegateCommand(ShowUpdatePassword);
            NewPasswordChange = new DelegateCommand<string>(NewPasswordChangeCmd);
            ConfirmNewPasswordCommand = new DelegateCommand(ConfirmNewPassword);
            CancelNewPasswordCommand = new DelegateCommand(CancelNewPassword);
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        private void ShowUpdatePhone()
        {
            NewPhone = String.Empty;
            
            
            PlaySound(SoundMapping.请输入手机号码);
            ShowMask(true, new UpdatePhone() { DataContext = this });
        }

        private void CancelUpdatePhone()
        {
            NewPhone = string.Empty;
            ShowMask(false);
        }

        private void ConfirmUpdatePhone()
        {
            if (NewPhone.IsNullOrWhiteSpace())
            {
                ShowMask(true, new UpdatePhone() { DataContext = this });
                return;
            }
            if (NewPhone.IsHandset())
            {
                WaiYuanModel.Phone = NewPhone;
                ShowMask(false);

            }
            else
            {
                ShowAlert(false, "格式错误", "手机号码格式不正确");
            }
        }

        private void ShowModifyAddress()
        {
            ShowMask(true, new FullInputBoard()
            {
                SelectWords = p => { NewAddress = p; },
                KeyAction = p => { StartTimer(); if (p == KeyType.CloseKey) ShowMask(false); }
            }, 0.1, pt => { ShowMask(false); });
        }

        private void ShowUpdatePassword()
        {
            NewPassword = NewPasswordRepeat = string.Empty;
            PasswordKeyboardShow = true;
            ShowMask(true, new PasswordDialog() { DataContext = this });
        }

        private void NewPasswordChangeCmd(string parameter)
        {
            PasswordKeyboardShow = parameter != "repeat";
        }

        private void CancelNewPassword()
        {
            ShowMask(false);
        }

        private void ConfirmNewPassword()
        {
            if (NewPassword.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "请输入密码", "请输入新密码");
                return;
            }
            if (NewPassword != NewPasswordRepeat)
            {
                ShowAlert(false, "密码格式不正确", "两次输入的密码不一致，请重新输入");
                return;
            }
            WaiYuanModel.Password = NewPassword;
            ShowMask(false);
        }


        private void Confirm()
        {
            if (NewPhone.IsNullOrWhiteSpace())
            {
                UpdateConfirmCommand.Execute(null);
                return;
            }
            if (WaiYuanModel.Password.IsNullOrWhiteSpace())
            {
                UpdatePassword.Execute(null);
                return;
            }
            if (NewAddress.IsNullOrWhiteSpace())
            {
                ModifyAddressCommand.Execute(null);
                return;
            }

            DoCommand(ctx =>
            {
                ctx.ChangeText("正在注册外院卡，请稍候...");
                CreateModel.Req病人建档发卡 = new req病人建档发卡
                {
                    operId = FrameworkConst.OperatorId,
                    cardNo = WaiYuanModel.CardNo,
                    cardType = "2",
                    name = WaiYuanModel.PatientName,
                    sex = WaiYuanModel.病人信息_外院.sex.ToString(),
                    birthday = WaiYuanModel.病人信息_外院.birthday,
                    idNo = WaiYuanModel.病人信息_外院.idNo,
                    idType = "1", //测试必传
                    nation = "",
                    address = NewAddress,
                    phone = WaiYuanModel.Phone,
#pragma warning disable 612
                    guardianName = null,
                    school = null,
#pragma warning restore 612
                    pwd = WaiYuanModel.Password, //卡密码
                    cash = null,
                    accountNo = null,
                    patientType = null,
                    setupType = ((int)CreateModel.CreateType).ToString(),
                    extend = "2"//外院卡
                };
                CreateModel.Res病人建档发卡 = DataHandlerEx.病人建档发卡(CreateModel.Req病人建档发卡);
                if (CreateModel.Res病人建档发卡.success)
                {
                    ShowAlert(true, "成功提示", "外院卡注册成功！");
                }
                else
                {
                    ShowAlert(false, "外院卡注册", "失败原因：" + CreateModel.Res病人建档发卡.msg);
                }
                Navigate(A.Home);
            });

        }
    }
}
