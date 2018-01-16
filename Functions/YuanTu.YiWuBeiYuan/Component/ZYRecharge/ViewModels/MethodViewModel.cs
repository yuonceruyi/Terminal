using YuanTu.Consts;

namespace YuanTu.YiWuBeiYuan.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel: YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        protected override void OnCAClick()
        {
            Navigate(A.Third.Cash);
        }
    }
}
