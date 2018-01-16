using System.Linq;
using YuanTu.Consts.Services;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using YuanTu.Devices.CardReader;
using Prism.Regions;
using YuanTu.Core.Log;
using YuanTu.Consts.Models;
using System;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Consts.Enums;
using YuanTu.Consts;

namespace YuanTu.BJJingDuETYY.Component.Auth.ViewModels
{
    public class CardViewModel : YuanTu.Default.Component.Auth.ViewModels.CardViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders) : base(rfCardReaders, magCardReaders)
        {
            _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "Act_A6_RF");
            _magCardReader = magCardReaders?.FirstOrDefault(p => p.DeviceId == "ACT_A6_Mag&IC");

        }

        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插卡口");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            LightBarService?.PowerOn(LightItem.银行卡);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
        protected override void OnGetInfo(string cardNo, string extendInfo = null)
        {
            Logger.Main.Info("开始获取信息");
            if (cardNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "病人信息查询", "卡片信息为空，请确认插卡方向是否正确和卡片是否有效",
                    extend: new AlertExModel { HideCallback = HideCallback });
                return;
            }
            DoCommand(_ =>
            {
                var req = new req病人信息查询
                {
                    Timeout = new TimeSpan(0, 1, 0), //读取病人信息设置超时1分钟
                    cardNo = cardNo,
                    cardType = ((int)CardModel.CardType).ToString()
                };
                PatientModel.Req病人信息查询 = req;
                var choiceModel = GetInstance<IChoiceModel>();
                if (choiceModel.Business == Business.检验结果)
                {
                    PatientModel.Req病人信息查询.extend = "hisOnly";
                }
                var res = DataHandlerEx.病人信息查询(req);
                PatientModel.Res病人信息查询 = res;
                if ((PatientModel.Res病人信息查询.msg??"").Contains("身份证"))
                {
                    ShowAlert(false, "病人信息查询", PatientModel.Res病人信息查询.msg);
                    Navigate(A.Home);
                    return;
                    //ShowInfoFix(PatientModel.Res病人信息查询.data.FirstOrDefault());
                }
                if (!res.success)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息", debugInfo: res.msg,
                        extend: new AlertExModel { HideCallback = HideCallback });
                    return;
                }
                if (res.data == null || res.data.Count == 0)
                {
                    ShowAlert(false, "病人信息查询", "未查询到病人的信息(列表为空)",
                        extend: new AlertExModel { HideCallback = HideCallback });
                    return;
                }
                CardModel.CardNo = cardNo;
                Next();
            });
        }
    }
}