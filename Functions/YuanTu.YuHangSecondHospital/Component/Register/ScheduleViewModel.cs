using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.YuHangSecondHospital.Component.Register.Models;
using YuanTu.YuHangSecondHospital.NativeService;
using YuanTu.YuHangSecondHospital.NativeService.Dto;

namespace YuanTu.YuHangSecondHospital.Component.Register
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IPreRegModel PreRegModel { get; set; }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;

            //TODO 挂号预算
            if (ChoiceModel.Business == Business.挂号)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行挂号预处理,请稍候...");
                    var req = new PerRegisterPayRequest
                    {
                        CardNo = CardModel.CardNo,
                        PaiBanId = ScheduleModel.所选排班.scheduleId,
                        TimeFlag = (DayTimeFlag)(int.Parse(ScheduleModel.所选排班.medAmPm) - 1),
                        PayFlag = PayMedhodFlag.院内账户,
                        AlipayAmount = "",
                        AlipayTradeNo = ""
                    };

                    var result = LianZhongHisService.GetHospitalPerRegisterInfo(req);

                    if (!result.IsSuccess)
                    {
                        return Result.Fail(result.Message);
                    }
                    PreRegModel.Res挂号预处理 = result.Value;
                    return Result.Success();
                }).ContinueWith(ctx =>
                {
                    if (!ctx.Result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", $"挂号预处理失败:{ctx.Result.Message}");
                        return;
                    }
                    var preRegInfo = PreRegModel.Res挂号预处理;
                    PaymentModel.Self = decimal.Parse(preRegInfo.ActualPay ?? "0") * 100;
                    PaymentModel.Insurance = decimal.Parse(preRegInfo.HealthCarePay ?? "0") * 100;
                    PaymentModel.Total = decimal.Parse(preRegInfo.TotoalPay ?? "0") * 100;
                    PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.LeftList = new List<PayInfoItem>
                    {
                    new PayInfoItem("日期：", schedulInfo.medDate),
                    new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.doctName)
                    };

                    PaymentModel.RightList = new List<PayInfoItem>
                    {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                    };
                    Next();
                });
            }
            else
            {
                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", schedulInfo.medDate),
                    new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.doctName)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };

                QuerySource(i);
            }
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.预约)
                {
                    lp.ChangeText("正在进行预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    var scheduleInfo = ScheduleModel.所选排班;
                    var deptInfo = DepartmentModel.所选科室;
                    RegisterModel.Req预约挂号 = new req预约挂号
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int) CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        idNo = patientInfo.idNo,
                        operId = FrameworkConst.OperatorId,
                        tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientId,
                        cash = PaymentModel.Total.ToString(),
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int) RegTypesModel.SelectRegType.RegType).ToString(),
                        medAmPm = scheduleInfo.medAmPm,
                        medDate = scheduleInfo.medDate,
                        deptCode = deptInfo.deptCode,
                        deptName = deptInfo.deptName,
                        scheduleId = scheduleInfo.scheduleId,
                        appoNo = SourceModel.所选号源?.appoNo,
                        patientName = patientInfo.name,
                        doctCode = scheduleInfo?.doctCode,
                        doctName = scheduleInfo?.doctName
                    };

                    RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                    if (RegisterModel.Res预约挂号?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "预约成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分预约",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = AppointPrintables(),
                            TipImage = "提示_凭条"
                        });
                        Navigate(A.YY.Print);
                        return Result.Success();
                    }
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "预约失败",
                            DebugInfo = RegisterModel.Res预约挂号?.msg
                        });
                        Navigate(A.YY.Print);
                    }
                    return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
                }

                lp.ChangeText("正在进行挂号，请稍候...");
                var req = new RegisterPayRequest
                {
                    CardNo = CardModel.CardNo,
                    PaiBanId = ScheduleModel.所选排班.scheduleId,
                    TimeFlag = (DayTimeFlag) (int.Parse(ScheduleModel.所选排班.medAmPm) - 1),
                    PayFlag = PayMedhodFlag.院内账户,
                };
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
                        req.AlipayAmount = PaymentModel.Self.ToString();
                        req.AlipayTradeNo = (ExtraPaymentModel.PaymentResult as 订单状态)?.outTradeNo;
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

                var res = LianZhongHisService.ExcuteHospitalRegister(req);

                if (res.IsSuccess)
                {
                    PreRegModel.Res挂号结果 = res.Value;
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "挂号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分挂号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = RegisterPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.XC.Print);
                    return Result.Success();
                }
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "挂号失败",
                        DebugInfo = res.Message
                    });
                    Navigate(A.XC.Print);
                }
                return Result.Fail(res.Message);
            }).Result;
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
           
            var queue = PrintManager.NewQueue("挂号单");
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊地址：{PreRegModel.Res挂号结果?.VisitingLocation}\n");
            if (PaymentModel.Total == PaymentModel.Insurance && PaymentModel.Self == 0){
                sb.Append($"支付方式：医保报销\n");
            }
            else {
                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            }
            
            sb.Append($"挂号总费：{PaymentModel.Total.In元()}\n");
            sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保金额：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            sb.Append($"如需退号请到窗口进行处理！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
           
            return queue;
        }

        protected override Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}