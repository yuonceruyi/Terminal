using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Devices.CardReader;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Core.Log;

namespace YuanTu.QDKouQiangYY.Component.Auth.ViewModels
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
            LightBarService?.PowerOn(LightItem.就诊卡社保卡);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
    }
}