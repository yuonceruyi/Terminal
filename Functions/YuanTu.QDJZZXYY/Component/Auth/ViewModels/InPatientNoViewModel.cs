using Microsoft.Practices.Unity;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using Prism.Regions;

namespace YuanTu.QDJZZXYY.Component.Auth.ViewModels
{
    public class InPatientNoViewModel : YuanTu.QDKouQiangYY.Component.Auth.ViewModels.InPatientNoViewModel
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
                    patientId = InPatientNo
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
