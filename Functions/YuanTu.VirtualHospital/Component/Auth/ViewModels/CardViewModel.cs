using System.Linq;
using YuanTu.Devices.CardReader;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Services;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;

namespace YuanTu.VirtualHospital.Component.Auth.ViewModels
{
    public class CardViewModel : Default.Component.Auth.ViewModels.CardViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        public CardViewModel(IRFCardReader[] rfCardReaders, IMagCardReader[] magCardReaders)
            : base(rfCardReaders, magCardReaders)
        {
            var tp = CurrentStrategyType();
            if (tp == DeviceType.HAtm)
            {
                _rfCardReader = rfCardReaders?.FirstOrDefault(p => p.DeviceId == "HuaDa_RF");
            }
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