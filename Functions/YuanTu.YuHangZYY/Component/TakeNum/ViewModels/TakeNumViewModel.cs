using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.YuHangZYY.Component.TakeNum.Models;
using YuanTu.YuHangZYY.NativeService;
using YuanTu.YuHangZYY.NativeService.Dto;

namespace YuanTu.YuHangZYY.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public IPreTakeNumModel PreTakeNumModel { get; set; }

        protected override void ConfirmAction()
        {
            var record = RecordModel.所选记录;
            ChangeNavigationContent(record.doctName);
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号预处理,请稍候...");
                var req = new PerGetTicketCheckoutRequest
                {
                    AppointmentId = RecordModel.所选记录.regNo,
                    AppointmentTime = DateTime.Parse(RecordModel.所选记录.medDate),
                    CardNo = CardModel.CardNo,
                    DayTimeFlag = (DayTimeFlag) int.Parse(RecordModel.所选记录.medAmPm),
                    PayFlag = PayMedhodFlag.院内账户,
                    RegisterOrder = RecordModel.所选记录.appoNo,
                    AlipayTradeNo = "",
                    AlipayAmount = ""
                };

                var result = LianZhongHisService.GetHospitalPerGetTicketCheckoutInfo(req);

                if (!result.IsSuccess)
                    return Result.Fail(result.Message);
                PreTakeNumModel.ResPreTakeNum = result.Value;
                return Result.Success();
            }).ContinueWith(ctx =>
            {
                if (!ctx.Result.IsSuccess)
                {
                    ShowAlert(false, "温馨提示", $"取号预处理失败:{ctx.Result.Message}");
                    return;
                }
                var preTakeNumInfo = PreTakeNumModel.ResPreTakeNum;
                var pmodel = PaymentModel as YuanTu.YuHangZYY.Component.Models.PaymentModel;
                PaymentModel.Self = decimal.Parse(preTakeNumInfo.ActualPay ?? "0") * 100;
                PaymentModel.Insurance = decimal.Parse(preTakeNumInfo.HealthCarePay ?? "0") * 100;
                PaymentModel.Total = decimal.Parse(preTakeNumInfo.TotalPay ?? "0") * 100;
                pmodel.CitizenBlance = decimal.Parse(preTakeNumInfo.CitizenCardBalance ?? "0") * 100;
                PaymentModel.Self = PaymentModel.Self < 0m ? 0m : PaymentModel.Self;
                PaymentModel.NoPay = PaymentModel.Self <= 0;
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", record.medDate),
                    new PayInfoItem("时间：", record.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", record.deptName),
                    new PayInfoItem("医生：", record.doctName)
                };
                if (pmodel.CitizenBlance > 0)
                {
                    PaymentModel.LeftList.Add(new PayInfoItem("智慧医疗余额：", pmodel.CitizenBlance.In元()));
                }
                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };
                Next();
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行取号，请稍候...");
                var req = new GetTicketCheckoutRequest
                {
                    AppointmentId = RecordModel.所选记录.regNo,
                    AppointmentTime = DateTime.Parse(RecordModel.所选记录.medDate),
                    CardNo = CardModel.CardNo,
                    DayTimeFlag = (DayTimeFlag) int.Parse(RecordModel.所选记录.medAmPm),
                    RegisterOrder = RecordModel.所选记录.appoNo,
                    PayFlag = PayMedhodFlag.院内账户,
                };
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                switch (PaymentModel.PayMethod)
                {
                    case PayMethod.未知:
                        break;

                    case PayMethod.现金:
                        break;

                    case PayMethod.银联:
                        req.PayFlag = PayMedhodFlag.银联;
                        break;

                    case PayMethod.预缴金:
                        req.PayFlag = PayMedhodFlag.院内账户;
                        break;

                    case PayMethod.社保:
                        break;

                    case PayMethod.支付宝:
                        req.PayFlag = PayMedhodFlag.支付宝扫码;
                        req.AlipayAmount = (PaymentModel.Self/100).ToString();
                        req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态)?.outTradeNo;
                        break;

                    case PayMethod.微信支付:
                        break;

                    case PayMethod.苹果支付:
                        break;

                    case PayMethod.智慧医疗:
                        req.PayFlag = PayMedhodFlag.市民卡;
                        break;

                    default:
                        req.PayFlag = PayMedhodFlag.院内账户;
                        break;
                }

                var res = LianZhongHisService.ExcuteHospitalGetTicketCheckout(req);

                if (res.IsSuccess)
                {
                    PreTakeNumModel.ResTakeNum = res.Value;
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "取号失败",
                        DebugInfo = res.Message
                    });
                    Navigate(A.QH.Print);
                }
                ExtraPaymentModel.Complete = true;
                return Result.Fail(res.Message);
            }).Result;
        }
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.当前病人信息;
        
            var record = RecordModel.所选记录;
            var sb = new StringBuilder();
           // sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
          //  sb.Append($"交易类型：预约取号\n");
            sb.Append($"科室名称：{record.deptName}\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            if (PaymentModel.Total == PaymentModel.Insurance && PaymentModel.Self == 0)
            {
                sb.Append($"支付方式：医保报销\n");
            }
            else
            {
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            }
            sb.Append($"挂号总费：{PaymentModel.Total.In元()}\n");
            sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保金额：{PaymentModel.Insurance.In元()}\n");
            var balance = decimal.Parse(patientInfo.accBalance);
            if (PaymentModel.PayMethod == PayMethod.预缴金)
            {
                sb.AppendLine($"账户余额：{(balance - PaymentModel.Self).In元()}");
            }
            else
            {
                sb.AppendLine($"账户余额：{balance.In元()}");
            }
            if (CardModel.CardType == CardType.社保卡)
            {
                var citizenBlance = decimal.Parse(PreTakeNumModel.ResPreTakeNum?.CitizenCardBalance ?? "0") * 100;
                if (PaymentModel.PayMethod == PayMethod.智慧医疗)
                {
                    sb.Append($"智慧医疗余额：{(citizenBlance - PaymentModel.Self).In元()}");
                }
                else
                {
                    sb.Append($"智慧医疗余额：{citizenBlance.In元()}");
                }
            }
            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{PreTakeNumModel.ResTakeNum?.VisitingLocation}\n");
            sb.Append($"挂号序号：{record?.appoNo}\n");
     
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            sb.Append($"如需退号请到窗口进行处理！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}