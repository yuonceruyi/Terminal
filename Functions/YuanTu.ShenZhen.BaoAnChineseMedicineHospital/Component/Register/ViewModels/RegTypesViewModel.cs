using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Insurance;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : Default.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public ICardModel CardModel { get; set; }


        [Dependency]
        public IYBModel YBModel { get; set; }

        //[Dependency]
        //public IPatientModel PatientModel { get; set; }
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    startDate =ChoiceModel.Business == Business.挂号? DateTimeCore.Today.ToString("yyyy-MM-dd"): RegDateModel.RegDate,
                    endDate =ChoiceModel.Business == Business.挂号? DateTimeCore.Today.ToString("yyyy-MM-dd"): RegDateModel.RegDate
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        for (int index = 0; index < DeptartmentModel.Res排班科室信息查询.data.Count; index++)
                        {
                            if (DeptartmentModel.Res排班科室信息查询.data[index].deptName.Contains("中医儿科"))
                            {
                                DeptartmentModel.Res排班科室信息查询.data[index].deptName = "儿科(中医儿科诊疗中心)";
                                break;
                            }
                        }


                        #region 康复分院的判断
                        if (ConfigBaoAnChineseMedicineHospital.康复分院的机器)
                        {
                            DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询?.data.Where(d => d.deptName.Contains("针灸康复医院") && (!d.deptName.Contains("内科")) && (!d.deptName.Contains("失眠科")) && (!d.deptName.Contains("脑病科"))).ToList();
                        }
                        else
                        {
                            DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询?.data.Where(d => !d.deptName.Contains("针灸康复医院")).ToList();
                        }
                        #endregion

                        #region 二三档医保的提示与判断
                        //二三档医疗保险
                        if (CardModel.CardType == CardType.社保卡 && (YBModel.参保类型 == ShenZhenArea.Enums.Cblx.基本医疗保险二档 || YBModel.参保类型 == ShenZhenArea.Enums.Cblx.基本医疗保险三档))
                        {
                            //绑定本部就医点
                            if (YBModel?.医保个人基本信息?.BDSK.ToLower() == ConfigBaoAnChineseMedicineHospital.BenBuBianMa)
                            {
                                //产科门诊、耳鼻喉科门诊、儿科门诊、妇科门诊、发热门诊、肛肠科门诊、口腔科门诊、皮肤科门诊、眼科门诊、中医儿科诊疗中心
                                DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询?.data.Where(d =>
                                d.deptName.Contains("产科门诊")
                                || d.deptName.Contains("耳鼻喉科门诊")
                                || d.deptName == "儿科门诊"
                                || d.deptName.Contains("妇科门诊")
                                || d.deptName.Contains("发热门诊")
                                || d.deptName.Contains("肛肠科门诊")
                                //|| d.deptName.Contains("口腔科门诊")
                                || d.deptName.Contains("皮肤科门诊")
                                || d.deptName.Contains("眼科门诊")
                                || d.deptName.Contains("中医儿科诊疗中心")
                                ).ToList();

                                for (int index = 0; index < DeptartmentModel.Res排班科室信息查询?.data?.Count; index++)
                                {
                                    if (DeptartmentModel.Res排班科室信息查询.data[index].deptName.Contains("产科门诊"))
                                    {
                                        DeptartmentModel.Res排班科室信息查询.data[index].deptName += "(限生育)";
                                    }
                                    if (DeptartmentModel.Res排班科室信息查询.data[index].deptName.Contains("儿科门诊"))
                                    {
                                        DeptartmentModel.Res排班科室信息查询.data[index].deptName += "(限少儿)";
                                    }
                                    if (DeptartmentModel.Res排班科室信息查询.data[index].deptName.Contains("中医儿科诊疗中心"))
                                    {
                                        DeptartmentModel.Res排班科室信息查询.data[index].deptName += "(限少儿)";
                                    }
                                    if (DeptartmentModel.Res排班科室信息查询.data[index].deptName.Contains("发热门诊"))
                                    {
                                        DeptartmentModel.Res排班科室信息查询.data[index].deptName += "(限发热)";
                                    }
                                }
                                DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询.data.Where(d => 1 == 1).OrderBy(d => d.deptName).ToList();

                                ChangeNavigationContent(i.Title);
                                Next();
                                return;
                            }
                            else
                            {
                                //绑定我院社康
                                if (ConfigBaoAnChineseMedicineHospital.SheKangBianMa.ContainsKey(YBModel?.医保个人基本信息?.BDSK.ToLower()))
                                {
                                    ShowAlert(true, "温馨提示", "您的社保卡绑定我院" + ConfigBaoAnChineseMedicineHospital.SheKangBianMa[YBModel?.医保个人基本信息?.BDSK.ToLower()] + "\n需要转诊单才能享受社保报销", 10);
                                }
                                else
                                {
                                    ShowAlert(true, "温馨提示", "您的社保卡未绑定我院，本次诊疗只能自费", 10);
                                }
                            }
                        }
                        #endregion

                        switch (ChoiceModel.Business)
                        {
                            case Business.挂号:
                                if (!ConfigBaoAnChineseMedicineHospital.康复分院的机器)
                                {
                                    DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询?.data.Where(d =>
                                       (!d.deptName.Contains("口腔"))
                                       && ((!d.deptName.Contains("社康中心")))
                                       && ((!d.deptName.Contains("护理门诊")))
                                       && ((!d.deptName.Contains("门诊手术室")))
                                       && ((!d.deptName.Contains("结石病科门诊")))
                                       //&& ((!d.deptName.Contains("慢性病管理门诊")))
                                       && ((!d.deptName.Contains("麻醉科")))
                                       && ((!d.deptName.Contains("助产士")))
                                       && ((!d.deptName.Contains("呼吸科门诊")))
                                       && ((!d.deptName.Contains("腔镜中心门诊")))
                                       && ((!d.deptName.Contains("作废")))
                                       && ((!d.deptName.Contains("发热门诊")))
                                       && ((d.deptName != "院前科"))
                                       && ((!d.deptName.Contains("全科门诊")))
                                       && ((!d.deptName.Contains("名中医馆")))
                                       && ((!d.deptName.Contains("高危及体弱儿门诊")))
                                       && ((!d.deptName.Contains("推拿科门诊")))
                                       && ((!d.deptName.Contains("疼痛科门诊")))
                                       && ((!d.deptName.Contains("中医乳腺保健门诊")))
                               ).ToList();
                                }
                                break;
                            case Business.预约:
                                if (!ConfigBaoAnChineseMedicineHospital.康复分院的机器)
                                {
                                    DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询?.data.Where(d =>
                                      (!d.deptName.Contains("口腔"))
                                      && ((!d.deptName.Contains("社康中心")))
                                      && ((!d.deptName.Contains("护理门诊")))
                                      && ((!d.deptName.Contains("门诊手术室")))
                                      && ((!d.deptName.Contains("结石病科门诊")))
                                      //&& ((!d.deptName.Contains("慢性病管理门诊")))
                                      && ((!d.deptName.Contains("麻醉科")))
                                      && ((!d.deptName.Contains("助产士")))
                                      && ((!d.deptName.Contains("呼吸科门诊")))
                                      && ((!d.deptName.Contains("腔镜中心门诊")))
                                      && ((!d.deptName.Contains("作废")))
                                      && ((!d.deptName.Contains("发热门诊")))
                                      && ((d.deptName != "院前科"))
                                      && ((!d.deptName.Contains("全科门诊")))
                                      //&& ((!d.deptName.Contains("名中医馆")))
                                      && ((!d.deptName.Contains("高危及体弱儿门诊")))
                                      && ((!d.deptName.Contains("推拿科门诊")))
                                      && ((!d.deptName.Contains("疼痛科门诊")))
                                      && ((!d.deptName.Contains("中医乳腺保健门诊")))
                              ).ToList();
                                }
                                break;
                            default:
                                DeptartmentModel.Res排班科室信息查询.data = new List<排班科室信息>();
                                break;
                        }

                        DeptartmentModel.Res排班科室信息查询.data = DeptartmentModel.Res排班科室信息查询.data.Where(d => 1 == 1).OrderBy(d => d.deptName).ToList();

                        ChangeNavigationContent(i.Title);
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                }
            });
        }
    }
}