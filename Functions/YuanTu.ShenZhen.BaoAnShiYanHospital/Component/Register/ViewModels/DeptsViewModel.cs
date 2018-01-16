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

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : YuanTu.Default.Component.Register.ViewModels.DeptsViewModel
    {

        [Dependency]
        public IPatientModel PatientModel { get; set; }


        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            if (!RegTypesModel.SelectRegType.SearchDoctor) //普通
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询排班信息，请稍候...");
                    var req排班信息查询 = new YuanTu.ShenZhenArea.Gateway.req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
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

    }
}