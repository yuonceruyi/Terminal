using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;

namespace YuanTu.BJJingDuETYY.Component.ViewModels
{
    public class PrintViewModel: Default.Component.ViewModels.PrintViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (PrintModel.NeedPrint)
            {
                LightBarService?.PowerOn(LightItem.热敏打印机);
            }

            if(ChoiceModel.Business == Business.建档)
            {
                LightBarService?.PowerOn(LightItem.发卡机);
            }
        }
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (PrintModel.NeedPrint || ChoiceModel.Business == Business.建档)
            {
                LightBarService?.PowerOff();
            }

            return base.OnLeaving(navigationContext);
        }

    }
}
