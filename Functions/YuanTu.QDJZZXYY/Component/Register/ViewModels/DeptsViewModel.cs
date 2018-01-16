using System;
using System.Linq;
using System.Collections.ObjectModel;
using Prism.Regions;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.QDJZZXYY.Component.Register.Models;

namespace YuanTu.QDJZZXYY.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.QDKouQiangYY.Component.Register.ViewModels.DeptsViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var config = GetInstance<IConfigurationManager>();
            var list = DeptartmentModel.Res排班科室信息查询.data;
            if (ChoiceModel.Business == Business.挂号 &&
                config.GetValueInt("RegDept:Enabled") == 1)
            {
                var depts = config.GetValue("RegDept:Dept").Split('|');
                list = list.Where(p => depts.Contains(p.deptCode)).ToList();
            }

            if (ChoiceModel.Business == Business.挂号 &&
                config.GetValueInt("RegDeptDisable:Enabled") == 1)
            {
                var depts = config.GetValue("RegDeptDisable:Dept").Split('|');
                list = list.Where(p => !depts.Contains(p.deptCode)).ToList();
            }

            if (ChoiceModel.Business == Business.挂号 &&
                RegTypesModel.SelectRegType.RegType == RegType.急诊门诊 &&
                config.GetValueInt("StopEmergencyDept:Enabled") == 1)
            {
                var timeSpans = config.GetValue("StopEmergencyDept:TimeSpans").SafeToSplit('|');
                var temp = timeSpans
                    .Select(p => new TimeSpanConfig
                    {
                        StartTime = FormatTime(p, 0),
                        EndTime = FormatTime(p, 1),
                    }).Select(p =>
                    {
                        return IsBetweenTimeSpan(p);
                    }).ToList();

                bool b = false;
                foreach (var a in temp)
                {
                    b |= a;
                }

                if (!b)
                {
                    var depts2 = config.GetValue("StopEmergencyDept:DeptCode").Split('|');
                    list = list.Where(p => !depts2.Contains(p.deptCode)).ToList();
                }
            }

            var list2 = list.Select(p => new Info
            {
                Title = deptNameFormat(p.deptName),
                Tag = p,
                ConfirmCommand = confirmCommand
            });

            if (list2?.Count() == 0)
            {
                ShowAlert(false, "友好提示", "当前没有可挂号的科室，请选择其他类别");
                Preview();
            }
            Data = new ObservableCollection<Info>(list2);


            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        private DateTime FormatTime(string p, int beginFlag)
        {
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var timeStart = beginFlag == 0 ? p.SafeSubstring(0, 5) : p.Substring(p.IndexOf('-') + 1);
            var start = DateTime.ParseExact($"{date} {timeStart}", "yyyy-MM-dd HH:mm", null);
            return start;
        }
        private bool IsBetweenTimeSpan(TimeSpanConfig p)
        {
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var start = DateTime.ParseExact($"{p.StartTime}", "yyyy-MM-dd HH:mm:ss", null);
            var end = DateTime.ParseExact($"{p.EndTime}", "yyyy-MM-dd HH:mm:ss", null);
            if (start < end)
            {
                if (DateTimeCore.Now > start && DateTimeCore.Now < end)
                {
                    return true;
                }
            }
            else
            {
                if (DateTimeCore.Now > start || DateTimeCore.Now < end)
                {
                    return true;
                }
            }

            return false;
        }
    }

}
