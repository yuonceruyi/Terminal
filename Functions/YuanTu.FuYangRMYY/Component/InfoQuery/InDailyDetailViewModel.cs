using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.FuYangRMYY.Component.InfoQuery.ViewModels
{
    public class InDailyDetailViewModel : YuanTu.Default.Component.InfoQuery.ViewModels. InDailyDetailViewModel
    {
        
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HideNavigating = true;
        }
    }
}