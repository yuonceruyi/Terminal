using System;
using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Register.Models;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Register.ViewModels
{
    public class RegAmPmViewModel :Default.Component.Register.ViewModels.RegAmPmViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var temp= new ObservableCollection<InfoType>(
                AmPmConfig.GetInfoTypes(
                    ConfigurationManager,
                    ResourceEngine,
                    null,
                    new DelegateCommand<Info>(Confirm)
                )
            );
            temp[0].Remark = "当天07:00-11:00";
            temp[1].Remark = "当天11:00-17:00";
            Data = temp;
        }

    }
}