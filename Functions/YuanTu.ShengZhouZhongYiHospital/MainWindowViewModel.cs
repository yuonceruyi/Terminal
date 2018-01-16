using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Default.Component.Advertisement.Views;
using YuanTu.Default.Part.Views;

namespace YuanTu.ShengZhouZhongYiHospital
{
    public class MainWindowViewModel:YuanTu.Default.MainWindowViewModel
    {
        protected override void InitRegisterView()
        {
            var regionManager = GetInstance<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.导航, typeof(NavigateBarView));
            regionManager.RegisterViewWithRegion(RegionNames.页尾, typeof(BottomBarView));
            regionManager.RegisterViewWithRegion(RegionNames.页首, typeof(TopBarView));
            regionManager.RegisterViewWithRegion(RegionNames.广告, typeof(CarouselView));
        }
    }
}
