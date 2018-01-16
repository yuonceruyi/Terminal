using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Core.Extension;

namespace YuanTu.TaiZhouCentralHospital.Component.Register
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);
            QuerySource(i);
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询号源信息，请稍候...");
                SourceModel.Req号源明细查询 = new req号源明细查询
                {
                    operId = FrameworkConst.OperatorId,
                    regMode = "1",
                    regType = ((int) RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = ScheduleModel.所选排班.medDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
                    scheduleId = ScheduleModel.所选排班.scheduleId,
                    doctCode = ScheduleModel.所选排班.doctCode,
                    extend = ScheduleModel.所选排班.extend
                };
                SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
                if (SourceModel.Res号源明细查询?.success ?? false)
                    if (SourceModel.Res号源明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        Navigate(ChoiceModel.Business == Business.挂号 ? InnerA.XC.Time : A.YY.Time);
                    }
                    else
                    {
                        ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
                    }
                else
                    ShowAlert(false, "号源明细查询", "没有获得号源信息", debugInfo: SourceModel.Res号源明细查询?.msg);
            });
        }
    }
}