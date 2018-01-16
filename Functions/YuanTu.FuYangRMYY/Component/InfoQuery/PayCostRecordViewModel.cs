using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;

namespace YuanTu.FuYangRMYY.Component.InfoQuery.ViewModels
{
    public class PayCostRecordViewModel : YuanTu.Default.Component.InfoQuery.ViewModels.PayCostRecordViewModel
    {
        public override string Title => "查询缴费记录";
        

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            HideNavigating = true;
        }
    }
}