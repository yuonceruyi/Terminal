using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.QDKouQiangYY.Component.Register.Models;
using YuanTu.QDKouQiangYY.Component.Register.Services;

namespace YuanTu.QDHD2ZY.Component.Register.ViewModels
{
    public class RegTypesViewModel : YuanTu.QDKouQiangYY.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            var config = GetInstance<IConfigurationManager>();
            var no急诊门诊 = false;
            if (ChoiceModel.Business == Business.挂号 &&
                config.GetValueInt("StopEmergency:Enabled") == 1)
            {
                DateTime dBegintime;
                DateTime dEndtime;
                var beginTime = config.GetValue("StopEmergency:BeginTime");
                var endTime = config.GetValue("StopEmergency:EndTime");
                if (DateTime.TryParseExact(beginTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dBegintime) &&
                    DateTime.TryParseExact(endTime, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dEndtime))
                {
                    var realbegin = DateTimeCore.Today.AddHours(dBegintime.Hour).AddMinutes(dBegintime.Minute);
                    var realend = DateTimeCore.Today.AddHours(dEndtime.Hour).AddMinutes(dEndtime.Minute);

                    if (realbegin < realend)
                    {
                        if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                        {
                            no急诊门诊 = true;
                        }
                    }
                    else
                    {
                        if (DateTimeCore.Now > realbegin || DateTimeCore.Now < realend)
                        {
                            no急诊门诊 = true;
                        }
                    }
                }
            }
            var list = RegTypeDto.GetInfoTypes(
                config,
                ResourceEngine,
                "RegType",
                new DelegateCommand<Info>(Confirm),
                p =>
                {
                    if (p.RegType == RegType.急诊门诊 && no急诊门诊)
                        p.Visabled = false;

                    if (ChoiceModel.Business == Business.挂号 &&
                        DeptartmentModel.所选科室.configList
                        .Where(n => n.regMode == "2")
                        .All(n => n.regType != ((int)p.RegType).ToString()))
                    {
                        p.Visabled = false;
                    }
                });
            Data = new ObservableCollection<InfoType>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);
        }

        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;

            //预约走原流程
            if (ChoiceModel.Business == Business.预约)
            {
                base.Confirm(i);
            }
            #region 校验
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
                    if (realbegin < realend)
                    {
                        if (DateTimeCore.Now > realbegin && DateTimeCore.Now < realend)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                            return;
                        }
                    }
                    else
                    {
                        if (DateTimeCore.Now > realbegin || DateTimeCore.Now < realend)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}", $"{beginTime}至{endTime}期间非急诊停止挂号\r\n如需帮助请咨询服务台。");
                            return;
                        }
                    }
                }
            }
            #endregion 校验         

            GetSchedule(i);
        }
        protected void GetSchedule(Info i)
        {
            (ScheduleModel as ScheduleModel).IsDoctor = false;
            (ScheduleModel as ScheduleModel).DoctorCode = "";
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

                            //过滤掉当日挂号、非当前午别、挂到医生  的数据后，如没有则直接提示无排班
                            var curAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";

                            if (ChoiceModel.Business == Business.挂号 && ScheduleModel.Res排班信息查询.data.Where(p => p.medAmPm.SafeToAmPm() == curAmpm).ToList().Count == 0)
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
