using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Register.ViewModels
{
    public class DeptsViewModel : Default.Component.Register.ViewModels.DeptsViewModel
    {

        protected override void Confirm(Info i)
        {
            DeptartmentModel.所选科室 = i.Tag.As<排班科室信息>();


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
                    medAmPm = ChoiceModel.Business == Business.预约 ? null : (RegDateModel.AmPm == AmPmSession.上午 ? "1" : "2")
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
    }
}
