using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts.Dtos.Register;
using System.Collections.ObjectModel;
using YuanTu.Consts.Services;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Sounds;
using Prism.Commands;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.HuNanHangTianHospital.Common;
using YuanTu.Consts.Models.Register;
using Microsoft.Practices.Unity;

namespace YuanTu.HuNanHangTianHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : YuanTu.Default.Component.Register.ViewModels.RegTypesViewModel
    {
        [Dependency]
        public IScheduleModel ScheduleModel { get; set; }
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            if (DeptartmentModel?.所选科室?.parentDeptCode == null)
            {
                ShowAlert(false, "选择科室", "当前科室无类别信息,请选择其他科室");
                Preview();
            }
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();
            var bts = new List<RegTypeDto>();
            var regTypeCodes = DeptartmentModel.所选科室.parentDeptCode.Split('|');//和航天医院约定好 利用这个字段传 科室类别
            var filteList = new List<string>();
            foreach (string regTypeCode in regTypeCodes)
            {
                var regType = new RegType();
                if (!IsHasValue(regTypeCode, ref regType))
                {
                    continue;
                }
                if (regType == RegType.免费挂号 && ChoiceModel.Business == Business.预约)
                {
                    continue;
                }
                if (filteList.Contains(regTypeCode))
                {
                    continue;
                }
                filteList.Add(regTypeCode);
                var dto = RegTypeDto.Parse(config, resource, $"RegType:{regType}", regType.ToString());
                bts.Add(dto);
            }
            if (bts.Count <= 0)
            {
                ShowAlert(false, "科室类别选择提示", "该科室当前无数据");
                Preview();
                return;
            }


            var list = bts.OrderBy(p => p.Order).Select(p => new InfoType
            {
                Title = p.Name,
                ConfirmCommand = confirmCommand,
                IconUri = p.ImageSource,
                Tag = p,
                Color = p.Color,
                Remark = p.Remark
            });
            Data = new ObservableCollection<InfoType>(list);
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);

        }
        protected override void Confirm(Info i)
        {
            RegTypesModel.SelectRegType = (RegTypeDto)i.Tag;
            RegTypesModel.SelectRegTypeName = i.Title;

            if (!RegTypesModel.SelectRegType.SearchDoctor) //普通
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询排班信息，请稍候...");
                    ScheduleModel.排班信息查询 = new req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = RegTypeConvert.GetRegType(RegTypesModel.SelectRegType.RegType),
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
                });
            }
            else
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询医生信息，请稍候...");
                    DoctorModel.查询所有医生信息 = new req查询所有医生信息
                    {
                        deptCode = DeptartmentModel.所选科室.deptCode
                    };
                    DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                    if (DoctorModel.Res查询所有医生信息?.success ?? false)
                    {
                        if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                        {
                            ChangeNavigationContent(i.Title);
                            Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                        }
                        else
                        {
                            ShowAlert(false, "医生列表查询", "没有获得医生信息(列表为空)");
                        }
                    }
                    else
                    {
                        ShowAlert(false, "医生列表查询", "没有获得医生信息", debugInfo: DoctorModel.Res查询所有医生信息?.msg);
                    }
                });
            }
        }

        private bool IsHasValue(string regTypeCode, ref RegType type)
        {
            switch (regTypeCode)
            {
                case "00021":
                    type = RegType.急诊挂号;
                    return true;
                case "00022":
                    type = RegType.简易门诊;
                    return true;
                case "00016":
                    type = RegType.主治医师;
                    return true;
                case "00017":
                    type = RegType.副主任医师;
                    return true;
                case "00018":
                    type = RegType.主任医师;
                    return true;
                case "00000":
                    type = RegType.免费挂号;
                    return true;
                case "00009":
                    type = RegType.主治医生;
                    return true;
                case "00006":
                    type = RegType.副主任医生;
                    return true;
                case "00024":
                    type = RegType.主任医师_外院;
                    return true;
                default:
                    return false;
            }
        }
    }
}
