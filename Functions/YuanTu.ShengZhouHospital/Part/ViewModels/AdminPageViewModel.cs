using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Devices.CardReader;

namespace YuanTu.ShengZhouHospital.Part.ViewModels
{
    public class AdminPageViewModel: Default.Part.ViewModels.AdminPageViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "CRT310_IC");
            vm?.DisConnect();
        }
    }
}
