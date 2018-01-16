using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;

namespace YuanTu.NanYangFirstPeopleHospital.Component.Register.ViewModels
{
    public class RegDateViewModel : Default.Component.Register.ViewModels.RegDateViewModel
    {
        protected override int AppointingDays { get; set; } = 2;
        protected override void Confirm(Info i)
        {
            RegDateModel.RegDate = i.Title;
            ChangeNavigationContent(i.Title);
            var deptartmentModel = GetInstance<IDeptartmentModel>();
            var choiceModel = GetInstance<IChoiceModel>();
            //var regDateModel = GetInstance<IRegDateModel>();
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                deptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = choiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = null,//不选挂号类型
                    startDate =
                        choiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        choiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate
                };
                deptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(deptartmentModel.排班科室信息查询);
                if (deptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (deptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        Next();
                        return;
                    }

                    ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    return;
                }

                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: deptartmentModel.Res排班科室信息查询?.msg);
            });
        }
    }
}