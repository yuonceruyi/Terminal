using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;

namespace YuanTu.ChongQingArea.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : Default.Component.Auth.ViewModels.InPatientNoViewModel
    {
        public override void Confirm()
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
                    patientId = InPatientNo,
                    cardType = CardType.住院号.ToString(),
                };
                var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
                if (res?.success ?? false)
                {
                    if (res.data == null)
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
    }
}