using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Models;
using YuanTu.Default.Component.Auth.Dialog.Views;
using YuanTu.Default.Component.RealAuth.Dialog.Views;

namespace YuanTu.Default.Component.RealAuth.ViewModels
{
    public class PatientInfoViewModel : ViewModelBase
    {
        private string _hint = "密码校验";
        private bool _showUpdatePassWord;

        public PatientInfoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);

            PwdConfirmCommand = new DelegateCommand(() =>
            {
                ShowUpdatePassWord = true;
            });
            PwdUpdateCancelCommand = new DelegateCommand(() => { ShowUpdatePassWord = false; });
            PwdUpdateConfirmCommand = new DelegateCommand(PwdUpdateConfirm);
        }

        public override string Title => "密码校验";

        public bool ShowUpdatePassWord
        {
            get { return _showUpdatePassWord; }
            set
            {
                _showUpdatePassWord = value;
                if (value)
                {
                    PassWord = null;
                    NewPassWord = null;
                    ShowMask(true, new ConfirmPwd { DataContext = this });
                }
                else
                {
                    ShowMask(false);
                }
            }
        }

        public ICommand ConfirmCommand { get; set; }
        public ICommand PwdConfirmCommand { get; set; }
        public ICommand PwdUpdateCancelCommand { get; set; }
        public ICommand PwdUpdateConfirmCommand { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            TopBottom.InfoItems = null;

            PassWord = null;
            NewPassWord = null;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            Name = patientInfo.name;
            Sex = patientInfo.sex;
            Birth = patientInfo.birthday.SafeToSplit(' ', 1)[0];
            Phone = patientInfo.phone.Mask(3, 4);
            IdNo = patientInfo.idNo.Mask(14, 3);
            GuardIdNo = patientInfo.guardianNo.Mask(14, 3);
        }

        public virtual void Confirm()
        {
            if (PassWord.IsNullOrWhiteSpace())
            {
                ShowUpdatePassWord = true;
                return;
            }
            if (!PatientModel.Res诊疗卡密码校验?.success ?? false)
            {
                ShowAlert(false, "温馨提示", "密码校验失败，请输入密码重新校验");
                PassWord = null;
                NewPassWord = null;
                ShowUpdatePassWord = true;
                return;
            }

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            ChangeNavigationContent($"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

            var resource = ResourceEngine;
            TopBottom.InfoItems = new ObservableCollection<InfoItem>(new[]
            {
                new InfoItem
                {
                    Title = "姓名",
                    Value = patientInfo.name,
                    Icon = resource.GetImageResourceUri("姓名图标_YellowBlue")
                },
                new InfoItem
                {
                    Title = "余额",
                    Value = patientInfo.accBalance.In元(),
                    Icon = resource.GetImageResourceUri("余额图标_YellowBlue")
                }
            });

            Next();
        }

        public virtual void PwdUpdateConfirm()
        {
            if (string.IsNullOrWhiteSpace(NewPassWord))
            {
                ShowAlert(false, "温馨提示", "请输入就诊卡密码");
                return;
            }
            if (!NewPassWord.Is6Number())
            {
                ShowAlert(false, "温馨提示", "请输入正确的6位数字密码");
                return;
            }

            PassWord = NewPassWord;

            DoCommand(lp =>
            {
                lp.ChangeText("正在校验就诊卡密码，请稍候...");
                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                PatientModel.Req诊疗卡密码校验 = new req诊疗卡密码校验
                {
                    patientId = patientInfo.patientId,
                    password = PassWord,
                    operId = FrameworkConst.OperatorId
                };
                PatientModel.Res诊疗卡密码校验 = DataHandlerEx.诊疗卡密码校验(PatientModel.Req诊疗卡密码校验);
                if (PatientModel.Res诊疗卡密码校验?.success ?? false)
                {
                    ShowUpdatePassWord = false;
                }
                else
                {
                    ShowAlert(false, "密码校验", "密码校验失败，请确认密码是否正确");
                }
            });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            ShowMask(false);
            return true;
        }

        #region Binding

        private string _name;
        private string _sex;
        private string _birth;
        private string _phone;
        private string _idNo;
        private string _tips;
        private string _guardIdNo;
        private string _passWord;
        private string _newPassWord;

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Sex
        {
            get { return _sex; }
            set
            {
                _sex = value;
                OnPropertyChanged();
            }
        }

        public string Birth
        {
            get { return _birth; }
            set
            {
                _birth = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string PassWord
        {
            get { return _passWord; }
            set
            {
                _passWord = value;
                OnPropertyChanged();
            }
        }

        public string IdNo
        {
            get { return _idNo; }
            set
            {
                _idNo = value;
                OnPropertyChanged();
            }
        }

        public string GuardIdNo
        {
            get { return _guardIdNo; }
            set
            {
                _guardIdNo = value;
                OnPropertyChanged();
            }
        }

        public string NewPassWord
        {
            get { return _newPassWord; }
            set
            {
                _newPassWord = value;
                OnPropertyChanged();
            }
        }

        public string Tips
        {
            get { return _tips; }
            set
            {
                _tips = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding

        #region Ioc

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ITopBottomModel TopBottom { get; set; }

        #endregion Ioc
    }
}