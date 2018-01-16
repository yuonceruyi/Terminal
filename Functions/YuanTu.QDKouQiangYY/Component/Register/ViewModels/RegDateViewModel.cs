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

namespace YuanTu.QDKouQiangYY.Component.Register.ViewModels
{
    public class RegDateViewModel: YuanTu.Default.Component.Register.ViewModels.RegDateViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {           
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = new InfoMore[7];
            for (var i = 0; i < Week.Length; i++)
            {
                var date = DateTimeCore.Today.AddDays(i + 1);
                if (FrameworkConst.Local)
                {
                    date = DateTimeCore.Today.AddDays(i);
                }
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
