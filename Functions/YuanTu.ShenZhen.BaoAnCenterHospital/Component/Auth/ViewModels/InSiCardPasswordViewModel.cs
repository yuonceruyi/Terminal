using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Tools;
using YuanTu.Devices.CardReader;
using YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels;
using YuanTu.ShenZhenArea.CardReader;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Auth.ViewModels
{
    public class InSiCardPasswordViewModel  : SiCardPasswordViewModel
    {
        public InSiCardPasswordViewModel()
        {
            ConfirmCommand = new DelegateCommand(Do);
        }

        public override string Title => "请输入社保卡密码，没密码直接按确定键";

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            ShowAlert(true, "温馨提示", "社保卡密码是用户办理社保卡时设定的密码\n若您没有设置过密码可以直接点击确定键");
        }


        public override void Do()
        {
            if (string.IsNullOrEmpty(SiPassword))
                YBModel.医保密码 = "";
            else
                YBModel.医保密码 = SiPassword.Trim();
            var result = YBServices.医保个人基本信息查询();
            if (result.IsSuccess)
            {
                var GRXX = YBModel.医保个人基本信息;
                string cardNo = GRXX.DNH;
                if (cardNo.IsNullOrWhiteSpace())
                {
                    ShowAlert(false, "病人信息查询", "社保卡卡号为空，请确认插卡方向是否正确和卡片是否有效");
                    Navigate(A.Home);
                    return;
                }

                DoCommand(ctx =>
                {
                    var req = new req住院患者信息查询
                    {
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = cardNo,
                        //patientId = cardNo,
                    };

                    var res = DataHandlerEx.住院患者信息查询(req);
                    if (res?.success ?? false)
                    {
                        if (res?.data == null)
                        {
                            ShowAlert(false, "住院患者信息查询", "查询患者信息失败");
                            Navigate(A.Home);
                            return;
                        }
                        ChangeNavigationContent($"{cardNo}");
                        PatientModel.Res住院患者信息查询 = res;
                        Next();
                        return;
                    }
                    else
                    {
                        ShowAlert(false, "住院患者信息查询", "查询患者信息失败" + res?.msg);
                        Navigate(A.Home);
                        return;
                    }
                });
            }
            else
            {
                ShowAlert(false, "病人信息查询", "未找到该社保卡对应的信息。", debugInfo: result.Message);
                Preview();
                return;
            }
        }
    }
}
