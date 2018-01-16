using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tools;

namespace YuanTu.Default.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : ViewModelBase
    {
        public InPatientNoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入住院号";
        public string Hint { get; set; } = "请输入住院号";
        public ICommand ConfirmCommand { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            InPatientNo = string.Empty;
            ChangeNavigationContent(string.Empty);
            PlaySound(SoundMapping.请输入住院号);
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
                PatientModel.Req住院患者信息查询 = new req住院患者信息查询
                {
                    patientId = InPatientNo
                };
                var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
                if (res?.success ?? false)
                {
                    if (res?.data == null)
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                        return;
                    }
                    ChangeNavigationContent($"{InPatientNo}");
                    PatientModel.Res住院患者信息查询 = res;
                    Next();
                }
                else
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                }
            });
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("输入测试住院号");
            if (ret.IsSuccess)
            {
                InPatientNo = ret.Value;
                Confirm();
            }
        }

        #region Binding

        private string inPatientNo;

        public string InPatientNo
        {
            get { return inPatientNo; }
            set
            {
                inPatientNo = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding
    }
}