using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.ZheJiangHospital.Component.Register.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            var confirmCommand = new DelegateCommand<Info>(Confirm);

            if (Register.IsDoctor)
            {
                var list = Register.DoctSchedules
                    .Distinct(Equality<PAIBAN_YISHENG>.CreateComparer(p => p.DEPTCODE))
                    .Select(p => new Info
                    {
                        Title = p.DEPTNAME,
                        Tag = p,
                        ConfirmCommand = confirmCommand
                    });
                Data = new ObservableCollection<Info>(list);
            }
            else
            {
                var list = Register.Depts
                    .OrderBy(p => p.PLXH)
                    .Select(p => new Info
                    {
                        Title = p.DEPTNAME,
                        Tag = p,
                        ConfirmCommand = confirmCommand
                    });
                Data = new ObservableCollection<Info>(list);
            }

            PlaySound(SoundMapping.选择挂号科室);
        }

        protected override void Confirm(Info i)
        {
            if (Register.IsDoctor)
            {
                var dept = i.Tag.As<PAIBAN_YISHENG>();
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询医生排班信息，请稍候...");

                    //var deptsResult = DataHandler.GetScheduleDept(dept.DEPTCODE);
                    //if (!deptsResult.IsSuccess)
                    //{
                    //    ShowAlert(false, "科室排班列表查询", "没有获得科室排班信息", debugInfo: deptsResult.Exception?.Message);
                    //    return;
                    //}
                    //if (!deptsResult.Value.Any())
                    //{
                    //    ShowAlert(false, "科室排班列表查询", "没有获得科室排班信息(返回列表为空)");
                    //    return;
                    //}
                    //Register.DeptSchedules = deptsResult.Value;
                    Register.DoctSchedulesSingleDept = Register.DoctSchedules
                        .Where(s => s.DEPTCODE == dept.DEPTCODE)
                        .ToList();
                    ChangeNavigationContent(i.Title);
                    Navigate(A.XC.Schedule);
                });
            }
            else
            {
                var dept = i.Tag.As<KESHI_GUAHAO>();
                Register.SelectedDept = dept;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询科室排班信息，请稍候...");

                    var deptsResult = DataHandler.GetScheduleDept(dept.DEPTCODE);
                    if (!deptsResult.IsSuccess)
                    {
                        ShowAlert(false, "科室排班列表查询", "没有获得科室排班信息", debugInfo: deptsResult.Exception?.Message);
                        return;
                    }
                    if (!deptsResult.Value.Any())
                    {
                        ShowAlert(false, "科室排班列表查询", "没有获得科室排班信息(返回列表为空)");
                        return;
                    }
                    Register.DeptSchedules = deptsResult.Value;
                    ChangeNavigationContent(i.Title);
                    Navigate(A.XC.Schedule);
                });
            }
        }
    }
}