using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.ChongQingArea.Component.Register.ViewModels
{
    public class SourceViewModel : YuanTu.Default.Component.Register.ViewModels.SourceViewModel
    {

        public override string Title => "选择预约号源";

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = SourceModel.Res号源明细查询.data.Select(p => new InfoMore
            {
                Title = $"{p.medBegtime}-{p.medEndtime}",              
                Type = $"序号 {p.appoNo}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            SourceModel.所选号源 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);

            Next();
        }
    }
}