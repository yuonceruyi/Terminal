using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.TakeNum.Models;
using YuanTu.ZheJiangHospital.Component.ViewModels;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.TakeNum.ViewModels
{
    public class ApptRecordViewModel : Default.Component.TakeNum.ViewModels.ApptRecordViewModel
    {
        [Dependency]
        public ITakeNumModel TakeNum { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = TakeNum.Records.Select(p => new InfoAppt
            {
                Date = $"{p.MEDDATE:yyyy年MM月dd日}",
                Time = $"{p.MEDAMPM.SafeToAmPm()} {p.MEDTIME}",
                Department = $"{p.DEPTNAME.SafeSubstring(0, 7)}",
                Doctor = $"{p.DOCTNAME}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.STATUS == 0
            });
            Data = new ObservableCollection<InfoAppt>(list);

            PlaySound(SoundMapping.选择预约记录);
        }

        protected override void Confirm(Info i)
        {
            var record = i.Tag.As<YUYUE_JILU>();
            TakeNum.Record = record;
            ChangeNavigationContent(record.DOCTNAME);

            //TakeNum.List = new List<PayInfoItem>
            //{
            //    new PayInfoItem("就诊日期：", record.MEDDATE.ToString("yyyy年MM月dd日")),
            //    new PayInfoItem("就诊科室：", record.DEPTNAME),
            //    new PayInfoItem("就诊医生：", record.DOCTNAME),
            //    new PayInfoItem("就诊时段：", record.MEDAMPM.SafeToAmPm() + " " + record.MEDTIME),
            //    new PayInfoItem("就诊序号：", record.APPONO.ToString()),
            //    new PayInfoItem("挂号金额：", record.REGAMOUNT.In元(), true)
            //};

            //Next();

            PaymentModel.Self = record.REGAMOUNT * 100m;
            PaymentModel.Insurance = 0;
            PaymentModel.Total = record.REGAMOUNT * 100m;
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = () => ChoiceViewModel.Confirm(this);

            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", $"{record.MEDDATE:MM月dd日}"),
                new PayInfoItem("时间：", record.MEDAMPM.SafeToAmPm() + " " + record.MEDTIME),
                new PayInfoItem("科室：", record.DEPTNAME),
                new PayInfoItem("医生：", record.DOCTNAME)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };

            Navigate(A.QH.Confirm);
        }
    }
}