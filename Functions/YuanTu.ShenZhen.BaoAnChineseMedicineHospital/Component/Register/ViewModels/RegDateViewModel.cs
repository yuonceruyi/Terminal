using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Insurance;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Register.ViewModels
{
    public class RegDateViewModel : Default.Component.Register.ViewModels.RegDateViewModel
    {

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            AppointingDays = 30;
            var list = new InfoMore[AppointingDays];

            for (var i = 0; i < AppointingDays; i++)
            {
                var date = DateTimeCore.Today.AddDays(i);
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