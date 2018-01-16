using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Triage;
using System.Windows.Threading;

namespace YuanTu.BJJingDuETYY.Component.Register.ViewModels
{
    public class RegTypesViewModel : YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            var config = GetInstance<IConfigurationManager>();

            if (config.GetValueInt("IsTriager") == 1)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询您已预检的科室，请稍候...");
                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    TriageModel.Req患者预检记录信息查询 = new Consts.Triage.req患者预检记录信息查询
                    {
                        patientId = patientInfo.patientId
                    };
                    TriageModel.Res患者预检记录信息查询 = Consts.Triage.DataHandlerEx.患者预检记录信息查询(TriageModel.Req患者预检记录信息查询);
                });
            }
        }
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;

            var config = GetInstance<IConfigurationManager>();

            if (ChoiceModel.Business == Business.挂号 && RegTypesModel.SelectRegType.RegType != RegType.急诊门诊 && config.GetValueInt("StopReg:Enabled") == 1)
            {
                DateTime dBegintime;
                DateTime dEndtime;

                var beginTime = config.GetValue("StopReg:BeginTime");
                var endTime = config.GetValue("StopReg:EndTime");
                if (DateTime.TryParseExact(beginTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dBegintime) &&
                    DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dEndtime))
                {
                    var realbegin = DateTimeCore.Today.AddHours(dBegintime.Hour).AddMinutes(dBegintime.Minute);
                    var realend = DateTimeCore.Today.AddHours(dEndtime.Hour).AddMinutes(dEndtime.Minute);
                    if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                    {
                        ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                        return;
                    }
                }
            }
            #region 预检分诊
            DoCommand(lp =>
            {
                if (config.GetValueInt("IsTriager") == 1)
                {
                    TriageModel.IsRegTypeNeedTriage = false;
                    lp.ChangeText("正在查询需预检挂号类别信息，请稍候...");
                    TriageModel.Req需预检挂号类别信息查询 = new Consts.Triage.req需预检挂号类别信息查询 { };
                    TriageModel.Res需预检挂号类别信息查询 = Consts.Triage.DataHandlerEx.需预检挂号类别信息查询(TriageModel.Req需预检挂号类别信息查询);
                    if (TriageModel.Res需预检挂号类别信息查询?.success ?? false)
                    {
                        if (TriageModel.Res需预检挂号类别信息查询?.data?.Where(p => p.registrationTypeStatus == "1").ToList().Count > 0)
                        {
                            var resData = TriageModel.Res需预检挂号类别信息查询.data.Where(p => p.registrationTypeCode == RegTypesModel.SelectRegType.RegType.GetHashCode().ToString());
                            if (resData?.ToList().Count > 0)//该类别需要预检分诊
                            {
                                TriageModel.IsRegTypeNeedTriage = true;
                                if (TriageModel.Res患者预检记录信息查询?.success ?? false)
                                {
                                    if (TriageModel.Res患者预检记录信息查询?.data == null || TriageModel.Res患者预检记录信息查询?.data?.ToList().Count <= 0)
                                    {
                                        ShowAlert(false, "预检分诊", "没有预检分诊记录(列表为空)，请至护士站进行预检分诊");
                                        return false;
                                    }
                                }
                                else
                                {
                                    ShowAlert(false, "预检分诊", "获取预检分诊记录失败", debugInfo: TriageModel.Res患者预检记录信息查询.msg);
                                    return true;
                                }
                            }
                        }
                    }

                    return true;
                }
                else
                {
                    return true;
                }

            }).ContinueWith(ctx =>
            {
                View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, (Action)(() =>
                {
                    if (ctx.Result)
                    {
                        if (RegTypesModel.SelectRegType.RegType == RegType.急诊门诊)
                        {
                            ShowConfirm("温馨提示", "急诊挂号前请至分诊台分诊，\r\n以便明确就诊科室，避免延误就诊时机。\r\n是否已分诊？", cp =>
                            {
                                if (!cp)
                                {
                                    Navigate(A.Home);
                                    return;
                                }
                            }, 30, ConfirmExModel.Build("是", "否", true));
                        }

                        base.Confirm(i);
                    }

                }));

            });
            #endregion

        }
    }
}
