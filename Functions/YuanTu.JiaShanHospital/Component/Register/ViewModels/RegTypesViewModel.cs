using System.Collections.ObjectModel;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Dtos.Register;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;

namespace YuanTu.JiaShanHospital.Component.Register.ViewModels
{
    public class RegTypesViewModel : Default.Component.Register.ViewModels.RegTypesViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            var list = RegTypeDto.GetInfoTypes(
                GetInstance<IConfigurationManager>(),
                ResourceEngine,
                "RegType",
                new DelegateCommand<Info>(Confirm),
                p =>
                {
                    if (ChoiceModel.Business == Business.预约 && p.RegType == RegType.急诊门诊)
                        p.Visabled = false;
                }
            );
            Data = new ObservableCollection<InfoType>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号科室类别 : SoundMapping.选择预约科室类别);
        }
    }
}