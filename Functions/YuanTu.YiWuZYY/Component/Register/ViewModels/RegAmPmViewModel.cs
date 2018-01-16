using System;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Register.Models;

namespace YuanTu.YiWuZYY.Component.Register.ViewModels
{
    public class RegAmPmViewModel: YuanTu.Default.Component.Register.ViewModels.RegAmPmViewModel
    {
        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IDeptartmentModel DeptartmentModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        protected override void Confirm(Info i)
        {
            var cfg = i.Tag as AmPmConfig;
            var date = DateTimeCore.Today.ToString("yyyy-MM-dd");
            var start = DateTime.ParseExact($"{date} {cfg?.StartTime.BackNotNullOrEmpty("00:01")}", "yyyy-MM-dd HH:mm", null);
            var end = DateTime.ParseExact($"{date} {cfg?.EndTime.BackNotNullOrEmpty("23:59")}", "yyyy-MM-dd HH:mm", null);
            if (ChoiceModel.Business==Business.挂号&& (DateTimeCore.Now < start || DateTimeCore.Now > end) )
            {
                ShowAlert(false, "挂号限制", $"该场次仅能在{cfg.StartTime}-{cfg.EndTime}时间范围内操作");
                return;
            }
            RegDateModel.AmPm = i.Title.SafeToAmPmEnum();
            ChangeNavigationContent(i.Title);
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班科室，请稍候...");
                DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    
                    startDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
                    endDate =
                        ChoiceModel.Business == Business.挂号
                            ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                            : RegDateModel.RegDate,
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
