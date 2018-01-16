using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;

namespace YuanTu.TongXiangFirstHospital.Component.Register.ViewModels
{
    public class DeptsViewModel:TongXiangHospitals.Component.Register.ViewModels.DeptsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            IEnumerable<排班科室信息> list = DeptartmentModel.Res排班科室信息查询.data;
            if (Startup.IsDeptWhiteList)
            {
                list = list.Where(p => Startup.DeptWhiteList.Contains(p.deptCode));
            }
            var list2 = list.Select(p => new Info
            {
                Title = p.deptName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected override string GetScheduleViewModelName()
        {
            return typeof(ScheduleViewModel).FullName;
        }
    }
}
