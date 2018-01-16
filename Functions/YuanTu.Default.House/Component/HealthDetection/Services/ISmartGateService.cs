using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.House.Device.Gate;

namespace YuanTu.Default.House.Component.HealthDetection.Services
{
    public interface ISmartGateService:IService
    {
        void OnEntered(NavigationContext navigationContext);
        void OnLeaving(NavigationContext navigationContext);
    }

    class SmartGateService : ISmartGateService
    {
        public string ServiceName => "健康小屋闸门智能控制";

        [Dependency]
        public IGateService GateService { get; set; }

        public List<string> ShouldOpenAddress { get; set; } = new List<string>()
        {
            AInner.Health.SpO2,
            AInner.Health.Temperature,
        };

        bool ShouldOpen(NavigationContext navigationContext, string param)
        {
            var address = navigationContext.Parameters[param];
            return ShouldOpenAddress.Contains(address);
        }

        public void OnEntered(NavigationContext navigationContext)
        {
            var src = ShouldOpen(navigationContext, "From");
            var dest = ShouldOpen(navigationContext, "To");
            if (!src && dest && GateService.ServiceStatus != ServiceStatus.Opened)
                GateService.OpenGateAsync();
        }

        public void OnLeaving(NavigationContext navigationContext)
        {
            var src = ShouldOpen(navigationContext, "From");
            var dest = ShouldOpen(navigationContext, "To");
            if (src && !dest && GateService.ServiceStatus != ServiceStatus.Closed)
                GateService.CloseGateAsync();
        }
    }
}
