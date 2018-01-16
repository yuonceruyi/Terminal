using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.Register.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    class DeptsViewModel:Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(string.Empty);
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            Data = new ObservableCollection<Info>(
                Register.PAIBANLBItems.Select(p => new Info
            {
                Title = p.DeptName,
                Tag = p,
                ConfirmCommand = confirmCommand
            }));

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            var item = i.Tag.As<PAIBANLBItem>();
            Register.所选排班Item = item;
            ChangeNavigationContent(i.Title);
            Next();
        }
    }
}
