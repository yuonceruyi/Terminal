using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;

namespace YuanTu.NanYangFirstPeopleHospital.Component.TakeNum.ViewModels
{
    internal class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        protected override void CancelAction()
        {
            var record = RecordModel.所选记录;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock
            {
                Text = $"{ record.medDate } { record.medAmPm.SafeToAmPm() } { record.deptName }",
                Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0))
            });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                    CancelAppoModel.Req取消预约 = new req取消预约
                    {
                        appoNo = record.appoNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        regNo = record.regNo
#pragma warning restore 612
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        RecordModel.Res挂号预约记录查询.data.Remove(RecordModel.所选记录);
                        Navigate(RecordModel.Res挂号预约记录查询.data.Count > 0 ? A.QH.Record : A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);
                    }
                });
            }, 60, ConfirmExModel.Build("是", "否", true));
        }
        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
                var record = RecordModel.所选记录;

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,

                    appoNo = record.appoNo,
                    extend=$"{record.deptCode}|{record.doctCode}",

                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm=="上午"?"1":"2"
#pragma warning restore 612
                };
                base.FillRechargeRequest(TakeNumModel.Req预约取号);
                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    //PrintModel.SetPrintInfo(true, "取号成功", $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                    //         ConfigurationManager.GetValue("Printer:Receipt"), TakeNumPrintables());
                    //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条"));
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = base.TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });

                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                else
                {
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        //PrintModel.SetPrintInfo(false, "取号失败", errorMsg: TakeNumModel.Res预约取号?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "取号失败",
                            DebugInfo = TakeNumModel.Res预约取号?.msg
                        });
                        Navigate(A.QH.Print);
                    }

                    ExtraPaymentModel.Complete = true;

                    return Result.Fail(TakeNumModel.Res预约取号?.code??-100, TakeNumModel.Res预约取号?.msg);
                }
            }).Result;
        }

        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"挂号费：{record.regFee.In元()}\n");
            sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            sb.Append($"挂号金额：{record.regAmount.In元()}\n");
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"挂号序号：{takeNum?.visitNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}