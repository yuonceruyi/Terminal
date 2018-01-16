using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangHospital.Component.Appoint.ViewModels
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IAppointModel Appoint { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = Appoint.排班信息List.Select(p => new InfoMore
            {
                Title = $"{p.docname}{(p.title == p.docname ? string.Empty : $" {p.title}")}",
                SubTitle = $"{p.schdate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.ampm.SafeToAmPm()}",
                Type = "挂号费",
                Amount = decimal.Parse(p.fee) * 100m,
                Extends = $"号源 {p.numremain}/{p.numcount}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.numremain != "0"
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            Appoint.排班信息 = i.Tag.As<排班信息>();
            var req = new Req排班号源查询
            {
                schid = Appoint.排班信息.schid
            };

            var result = AppointService.Run<Res排班号源查询, Req排班号源查询>(req);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "预约排班号源", "没有获得预约号源信息", debugInfo: result.Message);
                return;
            }
            var res = result.Value;

            Appoint.号源信息List = res.list;
            ChangeNavigationContent(i.Title);
            Next();
        }
    }
}