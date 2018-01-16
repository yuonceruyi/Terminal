using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;

namespace YuanTu.JiaShanHospital.Component.Register.ViewModels
{
    public class SourceViewModel : Default.Component.Register.ViewModels.SourceViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = SourceModel.Res号源明细查询.data.Select(p => new InfoMore
            {
                Title = $"序号 {p.appoNo}",
                SubTitle = p.medBegtime,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoMore>(list);


            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }
    }
}
