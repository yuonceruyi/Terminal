using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.XiaoShanZYY.Component.Register.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.Register.ViewModels
{
    internal class SourceViewModel : Default.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IRegisterModel Register { get; set; }

        [Dependency]
        public IRegisterService RegisterService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = Register.所选号源Item.HAOYUANMX.Select(p => new InfoMore
            {
                Title = $"序号 {p.GUAHAOXH}",
                SubTitle = p.JIUZHENSJ,
                ConfirmCommand = confirmCommand,
                Tag = p,
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            Register.所选号源 = i.Tag.As<HAOYUANXX>();

            var result = RegisterService.汇总预约(Confirm);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "汇总预约信息", "汇总预约信息失败:" + result.Message);
                return;
            }
            ChangeNavigationContent(i.Title);
            Next();
        }

        protected Result Confirm()
        {
            return DoCommand(lp =>
            {
                var result = RegisterService.预约();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "预约处理", $"预约处理失败:\n{result.Message}");
                    return result;
                }
                RegisterService.预约打印();
                Navigate(A.YY.Print);
                return Result.Success();
            }).Result;
        }
    }
}