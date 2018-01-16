using System.Windows.Input;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class InputPhoneViewModel : ViewModelBase
    {
        private string _hint = "输入手机号";

        private string _phone;

        public InputPhoneViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入手机号";
        public ICommand ConfirmCommand { get; set; }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value;
                OnPropertyChanged();
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            Phone = null;
        }

        private void Confirm()
        {
            if (Phone.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "温馨提示", "请输入手机号");
                return;
            }
            if (!Phone.IsHandset())
            {
                ShowAlert(false, "温馨提示", "请输入正确的手机号");
                return;
            }
            GetInstance<ICardModel>().ExternalCardInfo = Phone;
            DoCommand(lp =>
            {
                var req = new req住院患者信息查询
                {
                    //patientId = GetInstance<IIpPatientModel>().IpPatientNo,
                    cardNo = GetInstance<IIdCardModel>().IdCardNo,
                    cardType = "0",
                    extend = Phone
                };
                var res = DataHandlerEx.住院患者信息查询(req);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院日清单", "查询患者信息失败");
                        return;
                    }
                    ChangeNavigationContent($"手机号\r\n{Phone}");
                    GetInstance<IPatientModel>().Res住院患者信息查询 = res;
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院日清单", "查询患者信息失败\n" + res?.msg);
                }
            });
        }
    }
}