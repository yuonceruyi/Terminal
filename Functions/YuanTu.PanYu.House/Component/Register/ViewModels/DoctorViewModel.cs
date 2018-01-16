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
    public class DoctorViewModel:Default.House.Component.Register.ViewModels.DoctorViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var res = ResourceEngine;
            var list = HisService.Res排班医生信息查询.data.Select(p => new InfoDoc
            {
                Title = p.doctName,
                Tag = p,
                ConfirmCommand = confirmCommand,
                IconUri = res.GetImageResourceUri("默认医生头像_House"),
                Description = null,
                Rank = p.doctTech,
                // 无法获取
                Amount = null,
                Remain = null,
            });
            Data = new ObservableCollection<Info>(list);

            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }
        protected override void Confirm(Info i)
        {
            HisService.排班医生信息 = i.Tag.As<排班医生信息>();

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                var result = HisService.Run排班信息查询();
                if (!result.IsSuccess)
                {
                    ShowAlert(false,"温馨提示",result.Message);
                    return;
                }
                ChangeNavigationContent(".");//导航栏状态改成已完成
                Next();
            });
        }
    }
}
