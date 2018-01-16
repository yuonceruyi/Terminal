using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Systems;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.VirtualHospital.Component.WaiYuan.Dialog;
using YuanTu.VirtualHospital.Component.WaiYuan.Models;
using UpdatePhone = YuanTu.VirtualHospital.Component.WaiYuan.Dialog.UpdatePhone;

namespace YuanTu.VirtualHospital.Component.WaiYuan.ViewModels
{
    public class WaiYuanPatientInfoViewModel:ViewModelBase
    {
        private string _newPhone;
        private string _newAddress;
        private string _newPassword;
        private string _newPasswordRepeat;
        private bool _passwordKeyboardShow;

        [Dependency]
        public IWaiYuanModel WaiYuanModel { get; set; }
        public override string Title => "外院卡激活信息";
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

        public string NewPhone
        {
            get { return _newPhone; }
            set { _newPhone = value;OnPropertyChanged(); }
        }

        public string NewAddress
        {
            get { return _newAddress; }
            set { _newAddress = value; OnPropertyChanged(); }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set { _newPassword = value;OnPropertyChanged(); }
        }

        public string NewPasswordRepeat
        {
            get { return _newPasswordRepeat; }
            set { _newPasswordRepeat = value;OnPropertyChanged(); }
        }

        public bool PasswordKeyboardShow
        {
            get { return _passwordKeyboardShow; }
            set { _passwordKeyboardShow = value;OnPropertyChanged(); }
        }

        public WaiYuanPatientInfoViewModel()
        {
            UpdateCommand=new DelegateCommand(ShowUpdatePhone);
            UpdateCancelCommand=new DelegateCommand(CancelUpdatePhone);
            UpdateConfirmCommand=new DelegateCommand(ConfirmUpdatePhone);
            ModifyAddressCommand=new DelegateCommand(ShowModifyAddress);
            UpdatePassword=new DelegateCommand(ShowUpdatePassword);
            NewPasswordChange=new DelegateCommand<string>(NewPasswordChangeCmd);
            ConfirmNewPasswordCommand=new DelegateCommand(ConfirmNewPassword);
            CancelNewPasswordCommand=new DelegateCommand(CancelNewPassword);
            ConfirmCommand =new DelegateCommand(Confirm);
        }

        private void ShowUpdatePhone()
        {
            NewPhone=String.Empty;
            ShowMask(true,new UpdatePhone() {DataContext = this});
        }

        private void CancelUpdatePhone()
        {
            ShowMask(false);
        }

        private void ConfirmUpdatePhone()
        {
           
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
                ShowAlert(false,"请输入密码","请输入新密码");
                return;
            }
            if (NewPassword!=NewPasswordRepeat)
            {
                ShowAlert(false, "密码格式不正确", "两次输入的密码不一致，请重新输入");
                return;
            }
            WaiYuanModel.Password = NewPassword;
            ShowMask(false);
        }



        private void Confirm()
        {
            ShowAlert(true,"成功提示","激活成功！");
            Navigate(A.Home);
        }
    }
}
