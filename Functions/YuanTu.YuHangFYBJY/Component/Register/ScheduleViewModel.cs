﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.YuHangFYBJY.Component.Register.Models;
using YuanTu.YuHangFYBJY.NativeService;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.Component.Register
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
                    var pmodel = PaymentModel as YuanTu.YuHangFYBJY.Component.Models.PaymentModel;
                    PaymentModel.Self = decimal.Parse(preRegInfo.ActualPay ?? "0") * 100;
                    PaymentModel.Insurance = decimal.Parse(preRegInfo.HealthCarePay ?? "0") * 100;
                    PaymentModel.Total = decimal.Parse(preRegInfo.TotoalPay ?? "0") * 100;
                    pmodel.CitizenBlance = decimal.Parse(preRegInfo.CitizenCardBalance ?? "0") * 100;

                    PaymentModel.Self = PaymentModel.Self < 0m ? 0m : PaymentModel.Self;
                    PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self <= 0; //默认预约或者自费金额为0时不支付
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.LeftList = new List<PayInfoItem>
                    {
                    new PayInfoItem("日期：", schedulInfo.medDate),
                    new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.doctName)
                    };
                    if (pmodel.CitizenBlance > 0)
                    {
                        PaymentModel.LeftList.Add(new PayInfoItem("智慧医疗余额：", pmodel.CitizenBlance.In元()));
                    }
                    PaymentModel.RightList = new List<PayInfoItem>
                    {
                    new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付总额：", PaymentModel.Total.In元(), true)
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
                    new PayInfoItem("支付总额：", PaymentModel.Total.In元(), true)
                };

                QuerySource(i);
            }
        }
        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生信息，请稍候...");
                SourceModel.Req号源明细查询 = new req号源明细查询
                {
                    operId = FrameworkConst.OperatorId,
                    regMode = "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = ScheduleModel.所选排班.medDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
                    doctCode = ScheduleModel.所选排班.doctCode.BackNotNullOrEmpty("*"),
                    scheduleId = ScheduleModel.所选排班.scheduleId
                };
                SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
                if (SourceModel.Res号源明细查询?.success ?? false)
                {
                    if (SourceModel.Res号源明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        Navigate(A.YY.Time);
                    }
                    else
                    {
                        ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
                    }
                }
                else
                {
                    ShowAlert(false, "号源明细查询", "没有获得号源信息", debugInfo: SourceModel.Res号源明细查询?.msg);
                }
            });
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
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        idNo = patientInfo.idNo,
                        operId = FrameworkConst.OperatorId,
                        tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientId,
                        cash = PaymentModel.Total.ToString(),
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
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
                    TimeFlag = (DayTimeFlag)(int.Parse(ScheduleModel.所选排班.medAmPm) - 1),
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
                        req.AlipayAmount = (PaymentModel.Self/100).ToString();
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
           // sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号费：{PaymentModel.Total.In元()}\n");
            sb.Append($"医保部分：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"自费部分：{PaymentModel.Self.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{PreRegModel.Res挂号结果?.VisitingLocation}\n");
            sb.Append($"挂号序号：{PreRegModel.Res挂号结果?.RegisterNo}\n");
           
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"就诊时请出示此凭条\n");
            sb.Append($"请妥善保管好您的凭条，以做就诊或退号凭证。\n");
            sb.Append($"如需打印发票请到门诊大厅发票打印窗口办理。\n");
            sb.Append($"祝您早日康复！\n");
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
            var mingxi = SourceModel.所选号源;
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"就诊日期：{schedule.medDate}\n");
            sb.Append($"就诊时间: {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            var time = string.IsNullOrWhiteSpace(mingxi.medBegtime) ? "" : DateTime.ParseExact(mingxi.medBegtime, "HH:mm", null, DateTimeStyles.None).AddMinutes(-10).ToString("HH:mm");
            sb.Append($"请于就诊当日{time}取号候诊\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}