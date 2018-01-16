using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Devices.CardReader;
using YuanTu.FuYangRMYY.Component.Auth.Models;

namespace YuanTu.FuYangRMYY.Component.Auth.ViewModels
{
    public class InSiCardViewModel:SiCardViewModel
    {
        public InSiCardViewModel(IIcCardReader[] icCardReaders, IRFCpuCardReader[] rfCpuCardReaders) : base(icCardReaders, rfCpuCardReaders)
        {
        }

        protected override void OnGetInfo(LoadingProcesser lp, string cardNo, string extendInfo = null)
        {
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效", extend: new Consts.Models.AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == AlertHideType.ButtonClick)
                                RunLoop();
                        }
                });
                return;
            }
            lp.ChangeText("正在查询住院病人信息，请稍后...");
            PatientModel.Req住院患者信息查询 = new req住院患者信息查询
            {
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = cardNo,
                patientId = cardNo,

            };
            var res = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
            if (res?.success ?? false)
            {
                if (res?.data == null)
                {
                    ShowAlert(false, "住院患者信息查询", "查询患者信息失败（列表为空）", extend: new Consts.Models.AlertExModel
                    {
                        HideCallback =
                            p =>
                            {
                                if (p == AlertHideType.ButtonClick)
                                    RunLoop();
                            }
                    });
                    return;
                }
                ChangeNavigationContent($"{cardNo}");
                PatientModel.Res住院患者信息查询 = res;
                Next();
            }
            else
            {
                ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg, extend: new Consts.Models.AlertExModel
                {
                    HideCallback =
                        p =>
                        {
                            if (p == AlertHideType.ButtonClick)
                                RunLoop();
                        }
                });
            }
            
        }
    }
}
