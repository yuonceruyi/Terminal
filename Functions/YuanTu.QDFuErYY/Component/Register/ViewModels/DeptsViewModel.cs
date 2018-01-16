using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.QDKouQiangYY.Component.Register.Models;
using Prism.Regions;
using Prism.Commands;
using System.Collections.ObjectModel;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.QDFuErYY.Component.Register.ViewModels
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

            GetTriageDept();

            var list2 = list.Select(p => new Info
            {
                Title = deptNameFormat(p.deptName),
                Tag = p,
                ConfirmCommand = confirmCommand,
                DisableText = GetDisableText(p)
            });
            Data = new ObservableCollection<Info>(list2);


            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }

        protected virtual string GetDisableText(排班科室信息 p)
        {
            if (TriageModel.IsRegTypeNeedTriage) //存在预检流程
            {
                return "需预检";
            }

            if (TriageModel.IsDeptNeedTriage)
            {
                //标记需预检、已预检，除此之外不标记
                if (TriageModel.Res需预检科室信息查询?.data?.Where(a =>
                    a.departCode.Contains(p.deptCode))?.ToList()?.Count > 0)
                {
                    return "需预检";
                }
            }
            return null;
        }

        protected virtual void GetTriageDept()
        {
            var config = GetInstance<IConfigurationManager>();

            if (config.GetValueInt("IsTriager") == 1)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询需预检科室信息，请稍候...");
                    TriageModel.Req需预检科室信息查询 = new Consts.Triage.req需预检科室信息查询 { };
                    TriageModel.Res需预检科室信息查询 = Consts.Triage.DataHandlerEx.需预检科室信息查询(TriageModel.Req需预检科室信息查询);
                    if (TriageModel.Res需预检科室信息查询?.success ?? false)
                    {
                        if (TriageModel.Res需预检科室信息查询?.data != null && TriageModel.Res需预检科室信息查询?.data?.Count > 0)
                        {
                            TriageModel.IsDeptNeedTriage = true;
                        }
                    }
                }, false);
            }
        }
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            (ScheduleModel as ScheduleModel).IsDoctor = false;
            (ScheduleModel as ScheduleModel).DoctorCode = "";

            if (TriageModel.IsRegTypeNeedTriage)
            {
                if (TriageModel.Res患者预检记录信息查询?.data == null || TriageModel.Res患者预检记录信息查询?.data?.ToList()?.Count <= 0)
                {
                    ShowAlert(false, "预检分诊", "没有预检分诊记录(列表为空)，请至护士站进行预检分诊");
                    return;
                }

                if (TriageModel.Res患者预检记录信息查询?.data?.Where(p =>
                    p.departCode.Contains(DeptartmentModel.所选科室.deptCode))?.ToList()?.Count <= 0)
                {
                    ShowAlert(false, "预检分诊", "您未分诊到本科室，请选择您的分诊科室");
                    return;
                }
                else
                {
                    ChangeNavigationContent(i.Title);
                    Next();
                }
            }
            else if (TriageModel.IsDeptNeedTriage)
            {
                var NeedTriage = TriageModel.Res需预检科室信息查询?.data?.Where(p =>
                     p.departCode.Contains(DeptartmentModel.所选科室.deptCode))?.ToList()?.Count > 0;

                if (NeedTriage)
                {
                    if (TriageModel.Res患者预检记录信息查询?.data == null || TriageModel.Res患者预检记录信息查询?.data?.ToList()?.Count <= 0)
                    {
                        ShowAlert(false, "预检分诊", "没有预检分诊记录(列表为空)，请至护士站进行预检分诊");
                        return;
                    }

                    if (TriageModel.Res患者预检记录信息查询?.data?.Where(p =>
                        p.departCode.Contains(DeptartmentModel.所选科室.deptCode))?.ToList()?.Count <= 0)
                    {
                        ShowAlert(false, "预检分诊", "您未分诊到本科室，请选择您的分诊科室");
                        return;
                    }
                    else
                    {
                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                }
                else
                {
                    ChangeNavigationContent(i.Title);
                    Next();
                }
            }
            else
            {
                ChangeNavigationContent(i.Title);
                Next();
            }
        }
    }

}
