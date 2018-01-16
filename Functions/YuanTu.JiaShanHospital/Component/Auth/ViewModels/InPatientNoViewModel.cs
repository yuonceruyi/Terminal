using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tools;

namespace YuanTu.JiaShanHospital.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : ViewModelBase
    {
        public InPatientNoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入住院号和手机号后四位";
        public string Hint { get; set; } = "请输入住院号和手机号后四位";
        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            InPatientNo = string.Empty;
            ChangeNavigationContent(string.Empty);
        }

        public virtual void Confirm()
        {
            if (InPatientNo == "")
            {
                ShowAlert(false, "住院患者信息查询", "住院号不能为空");
                return;
            }
            DoCommand(lp =>
            {
                var req = new req住院患者信息查询
                {
                    patientId = InPatientNo
                };
                var res = DataHandlerEx.住院患者信息查询(req);
                
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
                    if (string.IsNullOrEmpty(res.data.phone))
                    {
                        ShowAlert(false, "住院患者信息查询", "您未登记过手机号码,请到窗口处理");
                        return;
                    }
                    if (res.data.phone.Substring(res.data.phone.Length-4) != PhoneNo)
                    {
                        ShowAlert(false, "住院患者信息查询", "输入的住院号或者手机号后四位有误");
                        return;
                    }
                    ChangeNavigationContent($"{InPatientNo}");
                    PatientModel.Res住院患者信息查询 = res;
                    PatientModel.Res住院患者信息查询.extend = InPatientNo;
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                }
            });
        }

        #region Binding

        private string _inPatientNo;
        public string InPatientNo
        {
            get { return _inPatientNo; }
            set
            {
                _inPatientNo = value;
                OnPropertyChanged();
            }
        }

        private string _phoneNo;
        public string PhoneNo
        {
            get { return _phoneNo; }
            set
            {
                _phoneNo = value;
                OnPropertyChanged();
            }
        }

        private string _keyPanelValue;
        public string KeyPanelValue
        {
            get { return _keyPanelValue; }
            set
            {
                _keyPanelValue = value;
                if (_inPatientNoInputFocus)
                {
                    InPatientNo = _keyPanelValue;
                }
                else
                {
                    PhoneNo = _keyPanelValue;
                }
                OnPropertyChanged();
            }
        }

        private string _inPatientNoBorderBrush = "#00ffff";
        public string InPatientNoBorderBrush
        {
            get { return _inPatientNoBorderBrush; }
            set
            {
                _inPatientNoBorderBrush = value;
                OnPropertyChanged();
            }
        }

        private string _phoneNoBorderBrush = "#717171";
        public string PhoneNoBorderBrush
        {
            get { return _phoneNoBorderBrush; }
            set
            {
                _phoneNoBorderBrush = value;
                OnPropertyChanged();
            }
        }

        private bool _inPatientNoInputFocus = true;
        private bool _phoneNoInputFocus;

        public ICommand FocusChangeCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    _inPatientNoInputFocus = !_inPatientNoInputFocus;
                    InPatientNoBorderBrush = InPatientNoBorderBrush == "#00ffff" ? "#717171" : "#00ffff";
                    _phoneNoInputFocus = !_phoneNoInputFocus;
                    PhoneNoBorderBrush = PhoneNoBorderBrush == "#00ffff" ? "#717171" : "#00ffff";
                    KeyPanelValue = string.Empty;
                });
            }
        }

        #endregion Binding
    }
}