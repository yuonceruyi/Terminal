using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.YanTaiYDYY.Component.Register.ViewModels
{
    public class RegDateViewModel : YuanTu.Default.Component.Register.ViewModels.RegDateViewModel
    {

        public override void OnEntered(NavigationContext navigationContext)
        {
            var config = GetInstance<IConfigurationManager>();
            var canAppointToday = config.GetValue("AppointSet:CanAppointToday");
            if (canAppointToday == "1")
            {
                AppointingStartOffset = 0; //预约7天含当天
            }
            else
            {
                AppointingStartOffset = 1; //不含当天  
            }
            base.OnEntered(navigationContext);
        }
    }
}
