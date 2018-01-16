using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.JiaShanHospital.NativeServices;
using YuanTu.JiaShanHospital.NativeServices.Dto;

namespace YuanTu.JiaShanHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        [Dependency]
        public IDoctorModel DoctorModel { get; set; }
        private static string _registerNo = string.Empty;
        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            if (ChoiceModel.Business == Business.挂号)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("开始挂号预结算,处理时间在1-2分钟请稍后...");
                    var res = 挂号预结算();
                    if (!res.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", $"挂号预结算失败：{res.Message}");
                        return;
                    }
                    PaymentModel.Self = decimal.Parse(res.Value?.ActualPay ?? "0") * 100;
                    PaymentModel.Insurance = decimal.Parse(res.Value?.HealthCarePay ?? "0") * 100;
                    PaymentModel.Total = decimal.Parse(res.Value?.TotoalPay ?? "0") * 100;
                    PaymentModel.RightList = new List<PayInfoItem>()
                    {
                            new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                            new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                            new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true),
                    };
                    var schedulInfo = ScheduleModel.所选排班;
                    PaymentModel.LeftList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("时段：", schedulInfo.medAmPm.SafeToAmPm()),
                        new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                        new PayInfoItem("医生：", schedulInfo.doctName),
                    };
                    PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0 ||
                                         FrameworkConst.DoubleClick; //默认预约或者自费金额为0时不支付      
                    PaymentModel.ConfirmAction = Confirm;
                    Next();
                });
            }
            else
            {
                PaymentModel.Self = 0;
                PaymentModel.Insurance = 0;
                PaymentModel.Total = 0;
                var schedulInfo = ScheduleModel.所选排班;
                PaymentModel.MidList = new List<PayInfoItem>()
                {
                    new PayInfoItem("时段：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.doctName),
                };
                PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0 ||
                                     FrameworkConst.DoubleClick; //默认预约或者自费金额为0时不支付      
                PaymentModel.ConfirmAction = Confirm;
                QuerySource(i);
            }


        }
        private Result<PerRegisterPay> 挂号预结算()
        {
            Logger.Net.Info("嘉善:开始挂号预结算");
            var req = new PerRegisterPayRequest
            {
                CardNo = CardModel.CardNo,
                SelfPayTag = CardModel.CardType == CardType.就诊卡 ? 1 : 0,
                PaiBanId = ScheduleModel.所选排班.scheduleId,
                TimeFlag = (DayTimeFlag)(int.Parse(ScheduleModel.所选排班.medAmPm) - 1),
                PayFlag = PayMedhodFlag.银联,
                AlipayAmount = "",
                AlipayTradeNo = "",
            };
            Logger.Net.Info($"嘉善:挂号预结算入参{JsonConvert.SerializeObject(req)}");
            var res = LianZhongHisService.GetHospitalPerRegisterInfo(req);
            Logger.Net.Info($"嘉善:挂号预结算出参{JsonConvert.SerializeObject(res)}");
            return res;
        }
        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.预约)
                {
                    lp.ChangeText("正在进行预约，处理时间在1-2分钟请稍后...");

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
                        medDate = SourceModel.所选号源.medBegtime,
                        deptCode = deptInfo.deptCode,
                        deptName = deptInfo.deptName,
                        scheduleId = scheduleInfo.scheduleId,
                        appoNo = SourceModel.所选号源?.appoNo,
                        patientName = patientInfo.name,
                        doctCode = scheduleInfo?.doctCode,
                        doctName = scheduleInfo?.doctName,
                    };

                    RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                    if (RegisterModel.Res预约挂号?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "预约成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + "预约",
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
                else
                {
                    try
                    {
                        var req = new RegisterPayRequest
                        {
                            CardNo = CardModel.CardNo,
                            SelfPayTag = CardModel.CardType == CardType.就诊卡 ? 1 : 0,
                            PaiBanId = ScheduleModel.所选排班.scheduleId,
                            TimeFlag = (DayTimeFlag)(int.Parse(ScheduleModel.所选排班.medAmPm) - 1),
                        };
                        var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                        req.AlipayAmount = (PaymentModel.Self / 100).ToString();
                        switch (PaymentModel.PayMethod)
                        {
                            case PayMethod.未知:
                                break;
                            case PayMethod.现金:
                                break;
                            case PayMethod.银联:
                                Logger.Main.Info("挂号银联支付");
                                req.PayFlag = PayMedhodFlag.银联;
                                req.Account = (extraPaymentModel.PaymentResult as TransResDto).CardNo;
                                req.AlipayTradeNo = (extraPaymentModel.PaymentResult as TransResDto).Ref;
                                Logger.Main.Info("挂号银联model构造成功");
                                break;
                            case PayMethod.预缴金:
                                //req.PayFlag = PayMedhodFlag.院内账户;
                                break;
                            case PayMethod.社保:
                                break;
                            case PayMethod.支付宝:

                                req.PayFlag = PayMedhodFlag.支付宝;
                                req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                                break;
                            case PayMethod.微信支付:
                                req.PayFlag = PayMedhodFlag.微信;
                                req.AlipayTradeNo = (extraPaymentModel.PaymentResult as 订单状态).outTradeNo;
                                break;
                            case PayMethod.苹果支付:
                                break;
                            case PayMethod.智慧医疗:
                                break;
                            default:
                                break;
                        }
                        Logger.Net.Info($"嘉善:挂号结算入参{JsonConvert.SerializeObject(req)}");
                        var res = LianZhongHisService.ExcuteHospitalRegister(req);
                        Logger.Net.Info($"嘉善:挂号结算出参{JsonConvert.SerializeObject(res)}");
                        if (res.IsSuccess)
                        {
                            ExtraPaymentModel.Complete = true;
                            _registerNo = res.Value?.RegisterNo;
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "挂号成功",
                                TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + "挂号",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = RegisterPrintables(res.Value.PreRegisterId),
                                TipImage = "提示_凭条"
                            });
                            lp.ChangeText("正在挂号交易记录上传，请稍候...");
                            UpLoadTradeInfo(res.Value.PreRegisterId);
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
                        }
                        if (res.ResultCode == -100)
                        {
                            UpLoadTradeInfo("123456");
                        }
                        ShowAlert(false, "友情提示", $"挂号失败:{res.Message}");
                        return res.ResultCode == -100 ? Result.Fail(-100, res.Message) : Result.Fail(-1, res.Message);
                    }
                    catch (Exception e)
                    {
                        return Result.Fail(-1, $"支付异常{e}");
                    }

                }
            }).Result;
        }

        private void UpLoadTradeInfo(string settleId)
        {
            try
            {
                挂号记录同步到his系统();
                if (PaymentModel.Self != 0)
                {
                    自费交易记录同步到his系统(settleId);
                }
                if (PaymentModel.Insurance != 0)
                {
                    PaymentModel.PayMethod = PayMethod.社保;
                    医保交易记录同步到his系统(settleId);
                }
            }
            catch (Exception e)
            {
                Logger.Net.Error($"嘉善:挂号交易记录上传失败:{e.Message}");
            }
        }


        protected Queue<IPrintable> RegisterPrintables(string id)
        {
            Logger.Net.Info($"嘉善:挂号 凭条构造");
            var queue = PrintManager.NewQueue("挂号单");
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var payMethod = PaymentModel.Self == 0 ? "医保支付" : PaymentModel.PayMethod.ToString();
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊场次：{(schedule.medAmPm == "1" ? "上午" : "下午")}\n");
            sb.Append($"就诊地址：{schedule.extend}\n");
            sb.Append($"挂号序号：{_registerNo}\n");
            sb.Append($"挂号总费：{PaymentModel.Total.In元()}\n");
            sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
            sb.Append($"医保金额：{PaymentModel.Insurance.In元()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold) });
            sb.Clear();
            sb.Append($"支付方式：{payMethod}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"交易ID：{id}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"该凭条不作报销凭证！\n");
            sb.Append($"如需退号请到窗口进行处理！\n");
            sb.Append($"如需发票，请凭此票到人工窗口换取发票！\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            Logger.Net.Info($"嘉善:挂号 凭条构造结束");
            return queue;
        }
        protected override Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"就诊时间：{SourceModel.所选号源.medBegtime} \n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{schedule.extend}\n");
            sb.Append($"挂号序号：{register.appoNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 10 : 14), FontStyle.Bold) });
            sb.Clear();
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"该凭条不作报销凭证！\n");
            sb.Append($"如需退号请到窗口进行处理！\n");
            sb.Append($"如果需要发票,请到人工窗口进行打印,隔日无效！\n");
            sb.Append($"祝您早日康复！\n"); ;
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
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
                    medDate = RegDateModel.RegDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
                    doctCode = ScheduleModel.所选排班.doctCode,
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
        private void 自费交易记录同步到his系统(string settleId)
        {
            try
            {
                Logger.Net.Info($"开始交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Self.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    inHos = "1",
                    remarks = "挂号自费",
                    settleId = settleId
                };
                FillRechargeRequest(req);
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        private void 医保交易记录同步到his系统(string settleId)
        {
            try
            {
                Logger.Net.Info($"开始交易记录同步到his系统");
                var req = new req交易记录同步到his系统
                {
                    platformId = FrameworkConst.OperatorId,
                    hisPatientId = CardModel?.CardNo,
                    cardNo = CardModel?.CardNo,
                    idNo = PatientModel?.当前病人信息?.idNo,
                    patientName = PatientModel?.当前病人信息?.name,
                    tradeType = "2",
                    cash = PaymentModel?.Insurance.ToString(),
                    operId = FrameworkConst.OperatorId,
                    bankTime = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    tradeMode = GetEnumDescription(PaymentModel.PayMethod),
                    inHos = "1",
                    remarks = "挂号医保",
                    settleId = settleId
                };
                var res = DataHandlerEx.交易记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"交易记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"交易记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"交易记录同步到his系统失败异常:{ex.Message}");
            }
        }
        private void 挂号记录同步到his系统()
        {
            try
            {
                Logger.Net.Info($"开始预约挂号记录同步到his系统");
                var medAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";
                var req = new req预约挂号记录同步到his系统
                {
                    regMode = "2",
                    cardType = CardModel.CardType.ToString(),
                    idNo = PatientModel.当前病人信息.idNo,
                    patientName = PatientModel.当前病人信息.name,
                    phone = PatientModel.当前病人信息.phone,
                    medAmPm = medAmpm,
                    medDate = DateTimeCore.Now.Date.ToString("yyyy-MM-dd"),
                    medTime = DateTimeCore.Now.Date.ToString("HH:mm:ss"),
                    regType = RegTypesModel.SelectRegType.RegType.ToString(),
                    deptCode = DepartmentModel.所选科室.deptCode,
                    deptName = DepartmentModel.所选科室.deptName,
                    doctCode = "无",
                    cash = PaymentModel.Self.ToString(),
                };
                if (PaymentModel.Self != 0)
                {
                    FillRechargeRequest(req);
                }
                var res = DataHandlerEx.预约挂号记录同步到his系统(req);
                if (res.success)
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统成功");
                }
                else
                {
                    Logger.Net.Info($"预约挂号记录同步到his系统失败:{res.msg}");
                }
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"预约挂号记录同步到his系统失败异常:{ex.Message}");
            }
        }
        public static string GetEnumDescription(PayMethod value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }
        protected void FillRechargeRequest(req交易记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
        protected void FillRechargeRequest(req预约挂号记录同步到his系统 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.bankTime = posinfo.TransTime;
                    req.bankDate = posinfo.TransDate;
                    req.posTransNo = posinfo.Trace;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.MId;
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    //req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                    req.extend = thirdpayinfo.outTradeNo;
                }
            }
        }
    }
}
