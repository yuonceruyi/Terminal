using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tools;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : YuanTu.Default.Component.Auth.ViewModels.InPatientNoViewModel
    {
        public InPatientNoViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override string Title => "输入登记号";

        public override void OnEntered(NavigationContext navigationContext)
        {
            InPatientNo = string.Empty;
            ChangeNavigationContent(string.Empty);
            //PlaySound(SoundMapping.请输入登记号);
        }

        public override void Confirm()
        {
            if (InPatientNo == "")
            {
                ShowAlert(false, "住院患者信息查询", "登记号不能为空");
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
            var ret = InputTextView.ShowDialogView("输入测试登记号");
            if (ret.IsSuccess)
            {
                InPatientNo = ret.Value;
                Confirm();
            }
        }
    }
}