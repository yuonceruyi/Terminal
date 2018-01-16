using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.QDKouQiangYY.Component.Register.Services;
using Prism.Commands;
using System.Collections.ObjectModel;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QDQLYY.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.QDKouQiangYY.Component.Register.ViewModels.DeptsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var config = GetInstance<IConfigurationManager>();
            var list = DeptartmentModel.Res排班科室信息查询.data.Where(p => p.parentDeptCode == DeptartmentModel.所选父科室.parentDeptCode);
            if (ChoiceModel.Business == Business.挂号 &&
                config.GetValueInt("RegDept:Enabled") == 1)
            {
                var depts = config.GetValue("RegDept:Dept").Split('|');
                list = list.Where(p => depts.Contains(p.deptCode)).ToList();
            }

            var list2 = list.Select(p => new Info
            {
                Title = deptNameFormat(p.deptName),
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list2);


            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }
    }

}
