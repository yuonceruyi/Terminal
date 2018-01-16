using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;

namespace YuanTu.YiWuFuBao.Component.Register.ViewModels
{
    public class RegDateViewModel: YuanTu.Default.Component.Register.ViewModels.RegDateViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            AppointingDays = 7*4;
            AppointingStartOffset = 1;
            base.OnEntered(navigationContext);
        }
    }
}
