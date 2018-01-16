using System;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Register.Models;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Register;
using YuanTu.BJArea.Services.Register;

namespace YuanTu.BJJingDuETYY.Component.Register.ViewModels
{
    public class RegAmPmViewModel : YuanTu.Default.Component.Register.ViewModels.RegAmPmViewModel
    {
        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }
        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        protected override void Confirm(Info i)
        {
            var cfg = i.Tag as AmPmConfig;
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var start = DateTime.ParseExact($"{date} {cfg.StartTime}", "yyyy-MM-dd HH:mm", null);
            var end = DateTime.ParseExact($"{date} {cfg.EndTime}", "yyyy-MM-dd HH:mm", null);
            //if (ChoiceModel.Business == Business.挂号 && (DateTimeCore.Now < start || DateTimeCore.Now > end))
            //{
            //    ShowAlert(false, "挂号限制", $"该场次仅能在{cfg.StartTime}-{cfg.EndTime}时间范围内操作");
            //    return;
            //}
            RegDateModel.AmPm = i.Title.SafeToAmPmEnum();

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");

                ScheduleModel.排班信息查询 = new req排班信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    deptCode = DeptartmentModel.所选科室.deptCode,
                    parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                if (ScheduleModel.Res排班信息查询?.success ?? false)
                {
                    if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                    {
                        //只有科室排班
                        if (ScheduleModel.Res排班信息查询?.data.Where(p => p.doctCode.IsNullOrWhiteSpace()).Count(p => p.deptCode != null) ==
                            ScheduleModel.Res排班信息查询?.data?.Count)
                        {
                            if (ScheduleModel.Res排班信息查询?.data?.Where(p => p.medAmPm.SafeToAmPmEnum() == RegDateModel.AmPm && p.doctCode.IsNullOrWhiteSpace()).ToList().Count == 0)
                            {
                                ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                                return;
                            }
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
                        }
                        else
                        {
                            #region 医生简介信息
                            var isExist =
                                ScheduleModel.Res排班信息查询?.data.Where(p => !p.doctCode.IsNullOrWhiteSpace())
                                    .Select(p => p.doctCode)
                                    .All(p => DocExtendDic.DocDictionary.ContainsKey(p));
                            if ((bool)!isExist)
                            {
                                DoctorModel.查询所有医生信息 = new req查询所有医生信息
                                {
                                    deptCode = DeptartmentModel.所选科室.deptCode
                                };
                                DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                                if (DoctorModel.Res查询所有医生信息?.success ?? false)
                                {
                                    if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                                    {
                                        DoctorModel.Res查询所有医生信息?.data.ForEach(p =>
                                        {
                                            if (!DocExtendDic.DocDictionary.ContainsKey(p.doctCode))
                                            {
                                                DocExtendDic.DocDictionary.Add(p.doctCode, p);
                                            }
                                        });
                                    }
                                }
                            }
                            //过滤掉非当前选择的午别、挂到医生  的数据后，如没有则直接提示无排班
                            if (ScheduleModel.Res排班信息查询?.data?.Where(p => p.medAmPm.SafeToAmPmEnum() == RegDateModel.AmPm).ToList().Count == 0)
                            {
                                ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                            }
                            else
                            {
                                ChangeNavigationContent(i.Title);
                                Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                }


            });
        }
    }
}
