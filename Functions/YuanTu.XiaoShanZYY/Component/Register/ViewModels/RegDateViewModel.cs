using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.Register.Models;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class RegDateViewModel : Default.Component.Register.ViewModels.RegDateViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        [Dependency]
        public IRegisterService RegisterService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = Register.HAOYUANMXItems.Select(i => new InfoMore
            {
                Title = i.RegDate,
                SubTitle = DayOfWeek(DateTime.Parse(i.RegDate)),
                ConfirmCommand = confirmCommand,
                Tag = i
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(SoundMapping.选择预约日期);
        }

        protected override void Confirm(Info i)
        {
            var item = i.Tag.As<HAOYUANMXItem>();
            Register.RegDate = i.Title;
            Register.所选号源Item = item;

            ChangeNavigationContent(i.Title);
            Next();
        }
    }
}