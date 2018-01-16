using Microsoft.Practices.Unity;
using YuanTu.XiaoShanZYY.Component.InfoQuery.Models;

namespace YuanTu.XiaoShanZYY.Component.InfoQuery.ViewModels
{
    internal class DateTimeViewModel : Default.Component.InfoQuery.ViewModels.DateTimeViewModel
    {
        [Dependency]
        public IInfoQueryModel InfoQuery { get; set; }

        [Dependency]
        public IInfoQueryService InfoQueryService { get; set; }

        protected override void PayCostQuery()
        {
            if (DateTimeStart > DateTimeEnd)
            {
                ShowAlert(false, "已缴费信息查询", "开始时间不能晚于结束时间！");
                return;
            }
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询已缴费信息，请稍候...");
                InfoQuery.DateTimeStart = DateTimeStart;
                InfoQuery.DateTimeEnd = DateTimeEnd;
                var result = InfoQueryService.缴费结算查询();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "缴费结算查询", "缴费结算查询失败:\n" + result.Message);
                    return;
                }
                ChangeNavigationContent($"{DateTimeStart:yyyy-MM-dd}至\n{DateTimeEnd:yyyy-MM-dd}");
                Next();
            });
        }
    }
}