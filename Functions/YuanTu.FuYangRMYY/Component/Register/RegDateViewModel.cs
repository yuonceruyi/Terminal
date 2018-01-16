using Microsoft.Practices.Unity;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;

namespace YuanTu.FuYangRMYY.Component.Register
{
    public class RegDateViewModel : Default.Component.Register.ViewModels.RegDateViewModel
    {
        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        protected override void Confirm(Info i)
        {
            RegDateModel.RegDate = i.Title;
            ChangeNavigationContent(i.Title);
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = "1",
                    regType = null,
                    startDate = RegDateModel.RegDate,
                    endDate = RegDateModel.RegDate
                };
                DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
                if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
                {
                    if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
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