using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.ZheJiangHospital.Component.Auth.Models;

namespace YuanTu.ZheJiangHospital.Component.Appoint.ViewModels
{
    public class SourceViewModel : Default.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IAuthModel Auth { get; set; }

        [Dependency]
        public IAppointModel Appoint { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = Appoint.号源信息List.Select(p => new InfoMore
            {
                Title = $"{p.numtime}-{p.numendtime}",
                Type = $"序号 {p.numno}",
                Tag = p,
                ConfirmCommand = confirmCommand,
                IsEnabled = p.numstate == "0"
            });
            Data = new ObservableCollection<InfoMore>(list);
            PlaySound(SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            var source = i.Tag.As<号源信息>();
            Appoint.号源信息 = source;
            ChangeNavigationContent(i.Title);

            var schedule = Appoint.排班信息;

            PaymentModel.Self = decimal.Parse(schedule.regfee) * 100m;
            PaymentModel.Insurance = 0m;
            PaymentModel.Total = decimal.Parse(schedule.regfee) * 100m;
            PaymentModel.NoPay = true; //托收
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", schedule.schdate),
                new PayInfoItem("时间：", $"{schedule.ampm.SafeToAmPm()} {source.numtime}-{source.numendtime}"),
                new PayInfoItem("科室：", schedule.deptname),
                new PayInfoItem("医生：", schedule.docname)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            Next();
        }

        protected virtual Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行预约挂号，请稍候...");
                //随机6位密码
                Appoint.Pass = ((int)(new Random(DateTimeCore.Now.Millisecond).NextDouble() * 1000000)).ToString("D6");
                var req = new Req预约挂号
                {
                    numid = Appoint.号源信息.numid,
                    no = Appoint.号源信息.numno,
                    patname = Auth.Info.NAME,
                    patsex = Auth.Info.SEX == "男" ? "1" : "2",
                    mobileno = Auth.Info.PHONE,
                    idcardtype = "1",
                    idcard = Auth.Info.IDNO,
                    pass = Appoint.Pass,
                    oper = FrameworkConst.OperatorId,
                    appID = "10009-101",
                    time = DateTimeCore.Now.GetTimeStamp()
                };
                req.captcha = AppointService.GetCaptcha($"{req.appID}2C1C6A5056DC79A5{req.time}{req.funcode}");

                var result = AppointService.Run<Res预约挂号, Req预约挂号>(req);
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "预约挂号失败", $"预约挂号失败:{result.Message}");
                    return Result.Fail($"预约挂号失败:{result.Message}");
                }
                var res = result.Value;

                Appoint.Res预约挂号 = res;

                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "挂号预约",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分预约",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    Printables = AppointPrintables(),
                    TipImage = "提示_凭条"
                });

                Navigate(A.YY.Print);
                return Result.Success();
            }).Result;
        }

        private Queue<IPrintable> AppointPrintables()
        {
            var dept = Appoint.科室信息;
            var schedule = Appoint.排班信息;
            var source = Appoint.号源信息;
            var queue = PrintManager.NewQueue("挂号单");
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{Auth.Info.NAME}\n");
            sb.Append($"门诊号：{Auth.Info.PATIENTID}\n");
            sb.Append($"交易类型：预约挂号\n");
            //sb.Append($"排班类型：{paiban.doctTech}\n");
            sb.Append($"科室名称：{schedule.deptname}\n");
            sb.Append($"就诊医生：{schedule.docname}\n");
            sb.Append($"挂号费：{(decimal.Parse(schedule.regfee) * 100m).In元()}\n");
            sb.Append($"挂号金额：{(decimal.Parse(schedule.fee) * 100m).In元()}\n");
            sb.Append($"就诊时间：{schedule.schdate}\n");
            sb.Append($"就诊场次：{schedule.ampm.SafeToAmPm()} {source.numtime}-{source.numendtime}\n");
            sb.Append($"就诊地址：{dept.deptaddress}\n");
            sb.Append($"挂号序号：{source.numno}\n");
            sb.Append($"取号密码：{Appoint.Pass}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}