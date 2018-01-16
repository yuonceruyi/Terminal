using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.ZheJiangHospital.Component.Appoint;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.TakeNum.Models;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public IAuthModel Auth { get; set; }
        [Dependency]
        public ITakeNumModel TakeNum { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            List = TakeNum.List;
            ConfirmCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        protected override bool CanCancel()
        {
            return false;
            //var record = TakeNum.Record;
            //return DateTimeCore.Now < record.MEDDATE;
        }

        protected override bool CanConfirm()
        {
            var record = TakeNum.Record;
            return DateTimeCore.Today == record.MEDDATE;
        }

        protected override void CancelAction()
        {
            var record = TakeNum.Record;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock
            {
                Text =
                    $"{record.MEDDATE:yyyy-MM-dd} {record.MEDAMPM.SafeToAmPm()} {record.MEDTIME} {record.DEPTNAME} {record.DOCTNAME}",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
            });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    // TODO 参数来源
                    var req = new Req取消预约
                    {
                        orderid = "",
                        pass = "",
                        appID = "10009-101",
                        time = DateTimeCore.Now.GetTimeStamp()
                    };
                    req.captcha = AppointService.GetCaptcha($"{req.appID}2C1C6A5056DC79A5{req.time}{req.funcode}");
                    var result = AppointService.Run<Res取消预约, Req取消预约>(req);
                    if (result.IsSuccess)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        TakeNum.Records.Remove(record);
                        Switch(A.QuHao_Context, TakeNum.Records.Count > 0 ? A.QH.Record : A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: result.Message);
                    }
                });
            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected override void ConfirmAction()
        {
            var record = TakeNum.Record;
            ChangeNavigationContent(record.DOCTNAME);

            PaymentModel.Self = record.REGAMOUNT * 100m;
            PaymentModel.Insurance = 0;
            PaymentModel.Total = record.REGAMOUNT * 100m;
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = Confirm;

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

            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取号，请稍候...");

                    // TODO 参数来源
                    var req = new Req取预约号()
                    {
                        身份证号 = Auth.Info.IDNO,
                    };

                    switch (CardModel.CardType)
                    {
                        case CardType.身份证:
                            req.医保类别 = "1";
                            break;
                        case CardType.市医保卡:
                            req.医保类别 = "2";
                            break;
                        case CardType.省医保卡:
                            req.医保类别 = "3";
                            break;
                    }

                    var res = DataHandler.RunExe<Res取预约号>(req);
                    if (res.Success)
                    {
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "取号成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分取号",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = TakeNumPrintables(res),
                            TipImage = "提示_凭条"
                        });

                        Navigate(A.QH.Print);
                        return Result.Success();
                    }
                    ShowAlert(false, "预约取号失败", $"预约取号失败:{res.Message}");

                    return Result.Fail(res.Message);
                })
                .Result;
        }

        private Queue<IPrintable> TakeNumPrintables(Res取预约号 res)
        {
            var queue = PrintManager.NewQueue("取号单");
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{res.病人姓名}\n");
            sb.Append($"就诊卡号：{res.就诊卡号}\n");
            sb.Append($"门诊号码：{res.门诊号码}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"科室名称：{res.挂号科室}\n");
            sb.Append($"就诊医生：{res.挂号医生}\n");
            sb.Append($"就诊时间：{DateTimeCore.Today:yyyy-MM-dd}\n");
            sb.Append($"就诊场次：{res.挂号班别.SafeToAmPm()}\n");
            //sb.Append($"就诊地址：{res.就诊地址}\n");
            sb.Append($"就诊序号：{res.就诊序号}\n");
            sb.Append($"交易时间：{res.挂号日期}\n");
            sb.Append($"挂号费总额：{res.挂号费总额}\n");
            sb.Append($"账户支付：{res.病人账户支付}\n");
            sb.Append($"医保支付：{res.医保支付}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            return queue;
        }
    }
}