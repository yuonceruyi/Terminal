using YuanTu.Consts.Gateway;

namespace YuanTu.YiWuBeiYuan.Component.Auth.ViewModels
{
    public class InPatientNoViewModel: YuanTu.Default.Component.Auth.ViewModels.InPatientNoViewModel
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
                req住院患者信息查询 req = new req住院患者信息查询
                {
                    //patientId = InPatientNo.PadLeft(10, '0'),
                    patientId = InPatientNo,
                    //cardType = "1",
                    cardType =null
                };
                var res = DataHandlerEx.住院患者信息查询(req);
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
