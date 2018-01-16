using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using Prism.Regions;

namespace YuanTu.VirtualHospital.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            LightBarService?.PowerOn(LightItem.银行卡);
            base.OnEntered(navigationContext);
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
    }
}
