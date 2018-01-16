using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.YiWuFuBao.Dtos;
using YuanTu.YiWuFuBao.Models;

namespace YuanTu.YiWuFuBao.Component.Auth.ViewModels
{
    public class InPatientNoViewModel: YuanTu.Default.Component.Auth.ViewModels.InPatientNoViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }
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
                    var infoRest = YuanTu.YiWuArea.Commons.Tools.XmlDescribe<ZHUYUANRYXX_OUT>(res.data.extend);
                    if (!infoRest)
                    {
                        ShowAlert(false, "住院患者信息查询", infoRest.Message);
                        return;
                    }
                    var pInfo = PatientModel as PatientModel;
                    CardModel.CardNo = InPatientNo;
                    ChangeNavigationContent($"{InPatientNo}");
                    pInfo.Res住院患者信息查询 = res;
                    pInfo.ZhuyuanryxxOut = infoRest.Value;
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
