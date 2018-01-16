using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.TakeNum.Models;

namespace YuanTu.XiaoShanZYY.Component.TakeNum.ViewModels
{
    public class QueryViewModel : Default.Component.TakeNum.ViewModels.QueryViewModel
    {
        [Dependency]
        public ITakeNumService TakeNumService { get; set; }

        [Dependency]
        public ITakeNumModel NumModel { get; set; }

        public override void Do()
        {
            if (RegNo.IsNullOrWhiteSpace())
            {
                ShowAlert(false, "取号密码", "请输入取号密码！");
                return;
            }
            NumModel.取号密码 = RegNo;
            QueryAppoInfo();
        }

        public override void QueryAppoInfo()
        {
            DoCommand(lp =>
            {
                var ret = TakeNumService.汇总取号(Confirm);
                if (!ret.IsSuccess)
                {
                    ShowAlert(false, "预约取号", ret.Message);
                    return;
                }
                Next();
            });
        }

        public Result Confirm()
        {
            var result = TakeNumService.取号();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "取号处理", $"取号处理失败:\n{result.Message}");
                return result;
            }
            TakeNumService.取号打印();
            Navigate(A.QH.Print);
            return Result.Success();
        }
    }
}