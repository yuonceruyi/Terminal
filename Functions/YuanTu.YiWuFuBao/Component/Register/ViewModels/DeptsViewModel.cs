﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Core.Extension;

namespace YuanTu.YiWuFuBao.Component.Register.ViewModels
{
    public class DeptsViewModel: YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {
        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        protected override void Confirm(Info i)
        {
           var dept= i.Tag.As<排班科室信息>();
            ;
            //成人卡不允许挂儿科，儿保科，小儿外科，小儿内分泌科，小儿呼吸专科，新生儿科
           
            var abadonDeptWithSex = new Dictionary<Sex,string[]>()
            {
                {Sex.男, new []{"妇科", "计生科","孕产期保健" } },
                {Sex.女, new []{"男科" } },
            };
            //大于XX周岁不能挂XXX科室
            var abadonDeptWithOverAge= new Dictionary<int, string[]>
            {
                [14] = new[] { "儿科", "儿保科", "小儿外科", "小儿内分泌科", "小儿呼吸专科", "新生儿科" },
            };
            //小于XX周岁不能挂XXX科室
            var abadonDeptWithBelowAge = new Dictionary<int, string[]>
            {
                [14] = new[] {"外科"},
            };
            //28天以内才能挂新生儿科
            var abadonDeptWithOverDayAge = new Dictionary<int, string[]>
            {
                [28] = new[] { "新生儿门诊" },
            };
            DateTime birth;
            if (!DateTime.TryParseExact(PatientModel.当前病人信息.birthday,"yyyy-MM-dd",null,DateTimeStyles.None, out birth))
            {
                var idcardSafe = PatientModel.当前病人信息.idNo.SafeSubstring(6, 8);
                DateTime.TryParseExact(idcardSafe, "yyyyMMdd", null, DateTimeStyles.None, out birth);
            }

            var year = DateTimeCore.Now.Year - birth.Year;
            var days = (DateTimeCore.Now - birth).Days;
            foreach (var kv in abadonDeptWithOverAge)
            {
                if (year>kv.Key)
                {
                    if (kv.Value.Contains(dept.deptName))
                    {
                        ShowAlert(false, "不能挂号", $"大于{kv.Key}周岁不能挂{dept.deptName}的号！");
                        return;
                    }
                }
            }
            foreach (var kv in abadonDeptWithBelowAge)
            {
                if (year<kv.Key)
                {
                    if (kv.Value.Contains(dept.deptName))
                    {
                        ShowAlert(false, "不能挂号", $"小于{kv.Key}周岁不能挂{dept.deptName}的号！");
                        return;
                    }
                }
            }
            foreach (var kv in abadonDeptWithOverDayAge)
            {
                if (days > kv.Key)
                {
                    if (kv.Value.Contains(dept.deptName))
                    {
                        ShowAlert(false, "不能挂号", $"大于{kv.Key}周天不能挂{dept.deptName}的号！");
                        return;
                    }
                }
            }
            //if (year > 14)//说明是本人
            //{
            //    if (abadonDept.Contains(dept.deptName))
            //    {
            //        ShowAlert(false,"不能挂号",$"大于14周岁不能挂{dept.deptName}的号！");
            //        return;
            //    }
            //}
            if (abadonDeptWithSex.Keys.Contains(PatientModel.当前病人信息.sex.SafeToSex()))
            {
                var list = abadonDeptWithSex[PatientModel.当前病人信息.sex.SafeToSex()];
                if (list.Contains(dept.deptName))
                {
                    ShowAlert(false, "不能挂号", $"{PatientModel.当前病人信息.sex.SafeToSex()}性不能挂{dept.deptName}的号！");
                    return;
                }
            }
            //妇保院特例：孕产期保健可以挂到医生，因此做特殊判断

            DoCommand(lp =>
            {
                var showdoctor = RegTypesModel.SelectRegType.SearchDoctor /*|| dept.deptName == "孕产期保健"*/;
                DeptartmentModel.所选科室 = dept;
                if (!showdoctor) //普通
                {
                    lp.ChangeText("正在查询排班信息，请稍候...");
                    ScheduleModel.排班信息查询 = new req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int) RegTypesModel.SelectRegType.RegType).ToString(),
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                        startDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        endDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        medAmPm = ((int) RegDateModel.AmPm).ToString(),
                        extend = CardModel.CardNo
                    };
                    ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                    if (ScheduleModel.Res排班信息查询?.success ?? false)
                    {
                        if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                        {
                            ScheduleModel.Res排班信息查询.data.ForEach(p=>p.medAmPm= ((int)RegDateModel.AmPm).ToString());
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Schedule : A.YY.Schedule);
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
                }
                else
                {
                    lp.ChangeText("正在查询医生信息，请稍候...");
                    DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
                    var sreq = new req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int) RegTypesModel.SelectRegType.RegType).ToString(),
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                        startDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        endDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        medAmPm = ((int) RegDateModel.AmPm).ToString(),
                        extend = CardModel.CardNo
                    };
                    var sres = DataHandlerEx.排班信息查询(sreq);
                    if (sres?.success ?? false)
                    {
                        if (sres?.data?.Count > 0)
                        {
                            
                            var docts = sres.data
                                .Where(p => !p.doctCode.IsNullOrWhiteSpace() && p.doctCode != "*")
                                .Select(p => new 医生介绍()
                                {
                                    doctCode = p.doctCode,
                                    doctName = p.doctName,
                                    doctLevel = p.doctTech
                                })
                                .ToList();
                            if (!docts.Any())
                            {
                                ShowAlert(false, "医生信息查询", "没有获得医生信息。");
                            }
                            DoctorModel.Res查询所有医生信息 = new res查询所有医生信息() {success = true, data = docts};
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                        }
                        else
                        {
                            ShowAlert(false, "医生信息查询", "没有获得医生信息(列表为空)");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "医生信息查询", "没有获得医生信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
                    }
                }
            });
        }
    }
}
