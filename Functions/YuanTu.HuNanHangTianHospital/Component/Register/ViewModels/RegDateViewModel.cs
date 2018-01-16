using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.HuNanHangTianHospital.Component.Register.ViewModels
{
    public class RegDateViewModel : Default.Component.Register.ViewModels.RegDateViewModel
    {
        protected override void Confirm(Info i)
        {
            RegDateModel.RegDate = i.Title;
            ChangeNavigationContent(i.Title);
            var deptartmentModel = GetInstance<IDeptartmentModel>();
            var choiceModel = GetInstance<IChoiceModel>();
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                deptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = "1",
                    startDate = RegDateModel.RegDate,
                    endDate = RegDateModel.RegDate
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
