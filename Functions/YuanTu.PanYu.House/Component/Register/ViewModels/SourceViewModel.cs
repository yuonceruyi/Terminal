using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.PanYu.House.PanYuService;
using 号源明细 = YuanTu.PanYu.House.PanYuGateway.号源明细;

namespace YuanTu.PanYu.House.Component.Register.ViewModels
{
    public class SourceViewModel : Default.House.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IHisService HisService { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = HisService.Res号源明细查询.data.Select(p => new InfoTime
            {
                Time = $"{p.medBegTime}-{p.medEndTime}",
                Title = $"序号 {p.appoNo}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoTime>(list);
            
            
            PlaySound(SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            HisService.号源明细 = i.Tag.As<号源明细>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = HisService.排班信息;
            var sourceInfo = HisService.号源明细;
            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.MidList = new List<PayInfoItem>
                {
                new PayInfoItem("就诊人：",PatientModel.Name),
                new PayInfoItem("预约医生：",schedulInfo.doctName),
                new PayInfoItem("预约科室：",schedulInfo.deptName??HisService.排班科室信息?.deptName),
                new PayInfoItem("预约时间：",$"{schedulInfo.medDate} {schedulInfo.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("预约序号：",$"{sourceInfo.appoNo} {sourceInfo.medBegTime}-{sourceInfo.medEndTime}"),
                new PayInfoItem("诊查费：",PaymentModel.Total.In元(),true)
                };
            ChangeNavigationContent(".");//导航栏状态改成已完成
            Next();
        }

        protected virtual Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行预约挂号，请稍候...");
                var result = HisService.Run预约挂号();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", result.Message);
                    return Result.Fail(result.Message);
                }

                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "预约成功",
                    //TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分预约",
                    TipMsg=$"请在就诊当天去医院自助服务终端取号就诊",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = AppointPrintables(),
                    TipImage = "凭条出口_House"
                });
                ChangeNavigationContent(".");//导航栏状态改成已完成
                Navigate(A.YY.Print);

                return Result.Success();
            }).Result;
        }

        protected virtual Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约挂号单");
            var dept = HisService.排班科室信息;
            var doct = HisService.排班医生信息;
            var sech = HisService.排班信息;
            var detail = HisService.号源明细;
            var guahao = HisService.Res预约挂号?.data;
            var sb = new StringBuilder();
            string hospitalName=null;
            if (PanYuGateway.DataHandler.HospitalId == "549")
                hospitalName = "广州市番禺区中心医院";
            if (PanYuGateway.DataHandler.HospitalId == "550")
                hospitalName = "广州市番禺区中医院";
            if (PanYuGateway.DataHandler.HospitalId == "551")
                hospitalName = "广州市番禺区何贤纪念医院";
            sb.Append($"预约医院：{hospitalName}\n");
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{PatientModel.Name}\n");
            sb.Append($"民生卡号：{PatientModel.CardNo}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"预约科室：{dept.deptName}\n");
            sb.Append($"预约医生：{doct.doctName}\n");
            sb.Append($"就诊日期：{sech.medDate}\n");
            sb.Append($"就诊场次：{sech.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊时段：{detail.medBegTime + "~" + detail.medEndTime}\n");
            //sb.Append($"就诊地址：{guahao?.address}\n");
            sb.Append($"挂号序号：{guahao?.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜机编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于就诊当天{detail.medBegTime}前自助取号,过时作废。\n如需取消预约，请于{Convert.ToDateTime(sech.medDate).AddDays(-1).ToString("yyyy-MM-dd")}前到自助机自助办理。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            var qrCodePath =ResourceEngine.GetResourceFullPath("打印二维码_House");
            var qrCode = Image.FromFile(qrCodePath);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = qrCode,
                //Height = qrCode.Height / 1.5f,
                //Width = qrCode.Width / 1.5f
                Height = qrCode.Height,
                Width = qrCode.Width
            });
            return queue;
        }
    }
}