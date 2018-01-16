using System.Linq;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.ChongQingArea.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {
        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();

            //if (!RegTypesModel.SelectRegType.SearchDoctor) //普通
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
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
                            : RegDateModel.RegDate,
                    // 人民医院
                    extend = DeptartmentModel.所选科室.deptName
                };
                ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                if (ScheduleModel.Res排班信息查询?.success ?? false)
                {
                    var ScheduleList = ScheduleModel.Res排班信息查询.data;

                    ScheduleList = ScheduleModel.Res排班信息查询.data
                    .Where(t => /*(t.medAmPm == "0") || (t.medAmPm == "3") ||*/
                     ChoiceModel.Business == Business.挂号 ? (t.medAmPm == "1" && DateTimeCore.Now.Hour < 12) || (t.medAmPm == "2" && DateTimeCore.Now.Hour >= 12) ||( t.medAmPm == "3") : (t.medAmPm == "1" )|| (t.medAmPm == "2") || (t.medAmPm == "3")
                    ).ToList();

                    if (ScheduleList.Count > 0)
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
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
            });
            /* else
                 DoCommand(lp =>
                 {
                     lp.ChangeText("正在查询医生信息，请稍候...");
                     DoctorModel.查询所有医生信息 = new req查询所有医生信息
                     {
                         deptCode = DeptartmentModel.所选科室.deptCode
                     };
                     DoctorModel.Res查询所有医生信息 = DataHandlerEx.查询所有医生信息(DoctorModel.查询所有医生信息);
                     if (DoctorModel.Res查询所有医生信息?.success ?? false)
                         if (DoctorModel.Res查询所有医生信息?.data?.Count > 0)
                         {
                             ChangeNavigationContent(i.Title);
                             Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Doctor : A.YY.Doctor);
                         }
                         else
                         {
                             ShowAlert(false, "医生列表查询", "没有获得医生信息(列表为空)");
                         }
                     else
                         ShowAlert(false, "医生列表查询", "没有获得医生信息", debugInfo: DoctorModel.Res查询所有医生信息?.msg);
                 });*/
        }
    }
}