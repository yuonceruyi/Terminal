using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.PanYu.House.PanYuGateway;
using YuanTu.PanYu.House.PanYuService;

namespace YuanTu.PanYu.House.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.House.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list2 = HisService.Res排班科室信息查询.data.Select(p => new Info
            {
                Title = p.deptName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            HisService.排班科室信息 = i.Tag.As<排班科室信息>();
            DoCommand(lp =>
               {
                   lp.ChangeText("正在查询医生信息，请稍候...");
                   var result = HisService.Run排班医生信息查询();
                   if (!result.IsSuccess)
                   {
                       ShowAlert(false, "温馨提示", result.Message);
                       return;
                   }
                   ChangeNavigationContent(".");//导航栏状态改成已完成
                   Next();
               });
        }
    }
}