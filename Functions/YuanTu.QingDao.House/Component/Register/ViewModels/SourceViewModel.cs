using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserCenter.Entities;
using YuanTu.Core.Extension;

namespace YuanTu.QingDao.House.Component.Register.ViewModels
{
    public class SourceViewModel : Default.House.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var list = RegisterModel.Res查询排班号量.data.sourceList.Select(p => new InfoTime
            {
                Time = $"{p.medBegTime}-{p.medEndTime}",
                Title = $"序号 {p.appoNo}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoTime>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            RegisterModel.当前选择号源 = i.Tag.As<SourceDO>();
            ChangeNavigationContent(".");

            Next();
        }
    }
}