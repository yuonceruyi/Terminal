using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.PanYu.House.PanYuService;
using 排班信息 = YuanTu.PanYu.House.PanYuGateway.排班信息;

namespace YuanTu.PanYu.House.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.House.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }

        [Dependency]
        public new IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = HisService.排班信息列表.Select(p => new InfoMore
            {
                Title = p.doctName ?? DepartmentModel.所选科室?.deptName,
                SubTitle = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.medAmPm.SafeToAmPm()}",
                Type = "挂号费",
                Amount = decimal.Parse(p.regAmount),
                Extends = $"剩余号源 {p.restNum}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.restNum != "0"
            });
            Data = new ObservableCollection<InfoMore>(list);

            
            
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择预约医生 : SoundMapping.选择挂号医生);
        }

        protected override void Confirm(Info i)
        {
            HisService.排班信息 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            if (ChoiceModel.Business == Business.挂号)
            {
                var schedulInfo = HisService.排班信息;

                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.MidList = new List<PayInfoItem>
                {
                new PayInfoItem("就诊人",PatientModel.Name),
                new PayInfoItem("预约医生",schedulInfo.doctName),
                new PayInfoItem("预约科室",schedulInfo.deptName??HisService.排班科室信息?.deptName),
                new PayInfoItem("预约时间",$"{schedulInfo.medDate} {schedulInfo.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("预约序号",null),
                new PayInfoItem("诊查费",PaymentModel.Total.In元(),true)
                };
                Next();
            }
            else QuerySource(i);
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询号源信息，请稍候...");
                var result = HisService.Run号源明细查询();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", result.Message);
                    return;
                }
                ChangeNavigationContent(".");//导航栏状态改成已完成
                Next();
            });
        }
    }
}