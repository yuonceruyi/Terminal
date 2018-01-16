using Prism.Regions;

namespace YuanTu.YiWuZYY.Component.Register.ViewModels
{
    public class RegDateViewModel: YuanTu.Default.Component.Register.ViewModels.RegDateViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            AppointingDays = 7;
            AppointingStartOffset = 1;
            base.OnEntered(navigationContext);
        }
    }
}
