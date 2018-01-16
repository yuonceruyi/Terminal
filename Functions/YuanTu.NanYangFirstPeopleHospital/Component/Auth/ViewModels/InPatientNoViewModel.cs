using YuanTu.Core.Extension;
using YuanTu.NanYangFirstPeopleHospital.Component.Auth.Models;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : Default.Component.Auth.ViewModels.InPatientNoViewModel
    {
        public override void Confirm()
        {
            if (InPatientNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "住院日清单", "住院号不能为空");
                return;
            }
            ChangeNavigationContent($"住院号\r\n{InPatientNo}");
            GetInstance<IIpPatientModel>().IpPatientNo = InPatientNo;
            Next();
        }
    }
}