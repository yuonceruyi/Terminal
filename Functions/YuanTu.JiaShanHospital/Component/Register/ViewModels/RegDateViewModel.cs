using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;

namespace YuanTu.JiaShanHospital.Component.Register.ViewModels
{
    public class RegDateViewModel:YuanTu.Default.Component.Register.ViewModels.RegDateViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            AppointingDays -= 1;
            var list = new InfoMore[AppointingDays];

            for (var i = 0; i < AppointingDays; i++)
            {
                var date = DateTimeCore.Today.AddDays(i + AppointingStartOffset);
                list.SetValue(new InfoMore
                {
                    Title = date.ToString("yyyy-MM-dd"),
                    SubTitle = DayOfWeek(date),
                    Amount = null,
                    ConfirmCommand = confirmCommand,
                    Tag = i
                }, i);
            }
            Data = new ObservableCollection<InfoMore>(list);


            PlaySound(SoundMapping.选择预约日期);
        }
    }
}
