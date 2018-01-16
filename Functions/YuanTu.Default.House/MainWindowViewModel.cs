using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Default.House.Part.Views;

namespace YuanTu.Default.House
{
    public class MainWindowViewModel : Default.MainWindowViewModel
    {
        protected override void InitRegisterView()
        {
            var regionManager = GetInstance<IRegionManager>();
                regionManager.RegisterViewWithRegion(RegionNames.导航, typeof(NavigateBarView));
                regionManager.RegisterViewWithRegion(RegionNames.页尾, typeof(BottomBarView));
                regionManager.RegisterViewWithRegion(RegionNames.页首, typeof(TopBarView));
        }

        public override bool ShowBack => true;
    }
}