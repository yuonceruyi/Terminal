using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangHospital.Component.Appoint.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IAppointModel Appoint { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list2 = Appoint.科室信息List.Select(p => new Info
            {
                Title = p.departName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);
            PlaySound(SoundMapping.选择预约科室);
        }

        protected override void Confirm(Info i)
        {
            Appoint.科室信息 = i.Tag.As<科室信息>();
            var req = new Req科室排班查询
            {
                deptid = Appoint.科室信息.departID
            };
            var result = AppointService.Run<Res科室排班查询, Req科室排班查询>(req);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "预约排班查询", "没有获得预约排班信息", debugInfo: result.Message);
                return;
            }
            var res = result.Value;

            Appoint.排班信息List = res.list;
            ChangeNavigationContent(i.Title);
            Next();
        }
    }
}