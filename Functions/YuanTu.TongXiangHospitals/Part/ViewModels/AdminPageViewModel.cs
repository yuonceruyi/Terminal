using Microsoft.Practices.Unity;
using YuanTu.Core.Log;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;

namespace YuanTu.TongXiangHospitals.Part.ViewModels
{
    public class AdminPageViewModel : Default.Part.ViewModels.AdminPageViewModel
    {
        [Dependency]
        public ISiService SiService { get; set; }

        protected override void OnExitCommand()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("程序退出前，正在关闭医保服务，请稍候...");
                var ret = SiService.Close();
                if (!ret.IsSuccess)
                    Logger.Main.Warn($"程序退出前，医保接口关闭失败");
                Logger.Main.Info($"程序退出前，医保接口关闭成功");
            });

            base.OnExitCommand();
        }
    }
}