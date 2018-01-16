using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.ShenZhenArea.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.ShenZhenArea.Models.DepartModel;
using System.Collections.Generic;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            if (ChoiceModel.Business == Business.预约)
            {
                base.OnEntered(navigationContext);
                return;
            }

            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);


            var NowDepartment=ConfigBaoAnCenterHospital.ListDepartments.FirstOrDefault(p=>p.DepartName== RegTypesModel.SelectRegTypeName);
            if (NowDepartment == null || string.IsNullOrEmpty(NowDepartment.DepartName))  //没选择
            {
                ShowAlert(true, "请先选择科室大类", "请先选择科室大类");
                Preview();
                return;
            }


            var listSecond = NowDepartment.ChildDepartments.Select(p => new Info
            {
                Title = p.DepartName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(listSecond);
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室 : SoundMapping.选择预约科室);
        }





        protected override void Confirm(Info i)
        {
            if (ChoiceModel.Business == Business.挂号)
            {
                var temp = i.Tag.As<BaoAnCenterHospitalDepartment>();
                DeptartmentModel.所选科室 = DeptartmentModel.Res排班科室信息查询.data.FirstOrDefault(d => d.deptCode == temp.DepartCode);
                if (DeptartmentModel.所选科室 == null)
                {
                    ShowAlert(true, "当前科室没有相应的HIS对照", "当前科室没有相应的HIS对照！请确定相应的科室有排班且有对照。");
                    Preview();
                    return;
                }

                if ((!string.IsNullOrEmpty(DeptartmentModel.所选科室.deptName)) && string.IsNullOrEmpty(DeptartmentModel.所选科室.deptCode))
                {
                    ShowAlert(true, "当前业务不用挂号", DeptartmentModel.所选科室.deptName + "业务无需挂号");
                    Preview();
                    return;
                }
            }
            else
            {
                DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            }

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                var req排班信息查询 = new YuanTu.ShenZhenArea.Gateway.req排班信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegType.普通门诊).ToString(),
                    deptCode = DeptartmentModel.所选科室.deptCode,
                    parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                    startDate = ChoiceModel.Business == Business.挂号 ? DateTimeCore.Today.ToString("yyyy-MM-dd") : RegDateModel.RegDate,
                    endDate = ChoiceModel.Business == Business.挂号 ? DateTimeCore.Today.ToString("yyyy-MM-dd") : RegDateModel.RegDate,
                    PatientId = patientInfo.patientId
                };
                var Res排班信息查询 = SZDataHandler.排班信息查询(req排班信息查询);
                if (Res排班信息查询?.success ?? false)
                {
                    if (Res排班信息查询?.data?.Count > 0)
                    {
                        if (DateTimeCore.Now.Hour > 11)  //12点以后去掉上午的号
                        {
                            Res排班信息查询.data = Res排班信息查询.data.Where(d => !d.medAmPm.Contains("上午")).ToList();
                        }

                        ScheduleModel.Res排班信息查询 = new Consts.Gateway.res排班信息查询()
                        {
                            code = Res排班信息查询.code,
                            data = Res排班信息查询.data,
                            extend = Res排班信息查询.extend,
                            msg = Res排班信息查询.msg,
                            startTime = Res排班信息查询.startTime,
                            success = Res排班信息查询.success,
                            timeConsum = Res排班信息查询.timeConsum
                        };
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
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: Res排班信息查询?.msg);
                }
            });
        }
    }
}