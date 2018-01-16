using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.ShengZhouZhongYiHospital.Component.Register.Models;
using YuanTu.ShengZhouZhongYiHospital.Extension;

namespace YuanTu.ShengZhouZhongYiHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var list = RegTypeDto.GetInfoTypes(
                GetInstance<IConfigurationManager>(),
                ResourceEngine,
                "RegType",
                new DelegateCommand<Info>(Confirm),
                t =>
                {
                    if (ChoiceModel.Business == Business.预约 && (t.Name == "急诊门诊" || t.Name == "夜间特需"))
                    {
                        t.Visabled = false;
                    }
                });
            Data = new ObservableCollection<InfoType>(list);
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);

        }
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;
            ChangeNavigationContent(i.Title);
            if (ChoiceModel.Business == Business.挂号)
            {
                var ret = Judge();
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", ret.Message);
                    return;
                }
            }
            if (RegTypesModel.SelectRegType.RegType == RegType.夜间特需 ||
                RegTypesModel.SelectRegType.RegType == RegType.急诊门诊 || ChoiceModel.Business == Business.挂号)
            {
                跳过上下午选择(i);
            }
            else
            {
                Next();
            }

        }

        private Result Judge()
        {
            var birth = DateTime.Parse(PatientModel.当前病人信息.birthday.BackNotNullOrEmpty("1970-01-01"));
            var age = (DateTimeCore.Now.Year - birth.Year) + ((DateTimeCore.Now.Month - birth.Month) > 0 ? 0 : -1);

            var limit = RegisterLimitTools.JudgeDeptTypes(RegTypesModel.SelectRegTypeName, age,
                (int)PatientModel.当前病人信息.sex.SafeToSex());
            return limit;
        }

        private void 跳过上下午选择(Info i)
        {
            string regType;
            if (RegTypesModel.SelectRegType.RegType == RegType.专科普通)
            {
                regType = "03";
            }
            else if (RegTypesModel.SelectRegType.RegType == RegType.专科专家)
            {
                regType = "06";
            }
            else if (RegTypesModel.SelectRegType.RegType == RegType.夜间特需)
            {
                regType = "05";
            }
            else
            {
                regType = "0" + ((int)RegTypesModel.SelectRegType.RegType);
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "01" : "02",
                    regType = regType,
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        var data = new List<排班科室信息>();
                        DeptartmentModel.Res排班科室信息查询?.data.ForEach(p =>
                        {
                            var now = DateTimeCore.Now.JudgeAmPm() == AmPmSession.上午 ? "1" : "2";
                            if (p.parentDeptCode== now && data.FirstOrDefault(t => t.deptName == p.deptName) == null)
                            {
                                data.Add(p);
                            }
                        });
                        if (!data.Any())
                        {
                            ShowAlert(false, "科室列表查询", "该时段科室信息已经为空");
                            return;
                            ;
                        }
                        DeptartmentModel.Res排班科室信息查询.data = data;
                        Navigate(A.XC.Dept);
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "该时段科室信息已经为空");
                    }
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "该时段科室信息已经为空", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                }
            });
        }
    }
}
