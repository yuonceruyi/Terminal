using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.FuYangRMYY.Component.InfoQuery.ViewModels
{
    public class DiagReportViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.DiagReportViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HideNavigating = true;
        }
        
    }
}