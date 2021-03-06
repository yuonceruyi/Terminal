﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Consts.Models.Register;
using Microsoft.Practices.Unity;
using YuanTu.BJArea;
using YuanTu.BJArea.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.BJJingDuETYY.Component.Register.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.BJArea.Models.TakeNum;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Consts.Models.Payment;
using YuanTu.BJArea.Services.BankPosBOC;
using YuanTu.Core.Services.PrintService;
using YuanTu.BJArea.BeiJingSiPay;

namespace YuanTu.BJJingDuETYY.Component.Register.ViewModels
{
    public class ScheduleViewModel: YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IDoctorModel DoctorModel { get; set; }

        [Dependency]
        public IRegLockExtendModel RegLockExtendModel { get; set; }

        [Dependency]
        public IRegUnLockExtendModel RegUnLockExtendModel { get; set; }

        [Dependency]
        public IRegisterExtendModel RegisterExtendModel { get; set; }

        [Dependency]
        public IAppoRecordModel RecordModel { get; set; }

        [Dependency]
        public IRegDateModel RegDateModel { get; set; }
        [Dependency]
        public ISiModel SiModel { get; set; }

        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var curAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";
            var innerModel = (ScheduleModel as ScheduleModel);
            var k = ScheduleModel.Res排班信息查询.data
                .Where(p => (innerModel.IsDoctor ? p.doctCode == innerModel.DoctorCode : p.doctCode.IsNullOrWhiteSpace()) 
                        && p.medAmPm.SafeToAmPmEnum() == RegDateModel.AmPm)//当前选择的午别
                .Select(p => new InfoMore
                {
                    Title = p.doctName ?? DepartmentModel.所选科室?.deptName,
                    SubTitle = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.medAmPm.SafeToAmPm()}",
                    Type = "挂号费",
                    Amount = decimal.Parse(p.regAmount),
                    Extends = p.restnum == "-1" ? "停诊" : $"剩余号源 {p.restnum}",
                    ConfirmCommand = confirmCommand,
                    Tag = p,
                    IsEnabled = GetDisable(p, curAmpm),
                    DisableText = GetDisableText(p, curAmpm)
                });

            Data = new ObservableCollection<InfoMore>(k);

            UnlockSource();

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }
        protected virtual void UnlockSource()
        {
            #region 解除锁号
            if (ChoiceModel.Business == Business.挂号 && ConfigBJ.ScheduleVersion == "1")
            {
                //查询预约记录
                var patientModel = GetInstance<IPatientModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumExtendModel = GetInstance<ITakeNumExtendModel>();

                takeNumExtendModel.version = ConfigBJ.ScheduleVersion;

                DoCommand(lp =>
                {
                    lp.ChangeText("正在查询锁号记录，请稍候...");
                    RecordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                    {
                        patientId = patientModel.当前病人信息?.patientId,
                        patientName = patientModel.当前病人信息?.name,
                        startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                        endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                        //searchType = "2",//1、预约 2、挂号 3 加号4 诊间加号//北京修改
                        cardNo = cardModel.CardNo,
                        cardType = ((int)cardModel.CardType).ToString(),
                        status = "101",//锁号状态的
                        appoNo = "",//传空
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        extend = takeNumExtendModel.ToJsonString(),
                    };
                    RecordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(RecordModel.Req挂号预约记录查询);
                    if (RecordModel.Res挂号预约记录查询?.success ?? false)
                    {
                        if (RecordModel.Res挂号预约记录查询?.data.Where(p => p.status == "101").ToList().Count > 0)
                        {
                            lp.ChangeText("正在解除锁号记录，请稍候...");

                            RegUnLockExtendModel.version = ConfigBJ.ScheduleVersion;
                            foreach (var obj in RecordModel.Res挂号预约记录查询.data.Where(obj => !obj.extend.IsNullOrWhiteSpace() && obj.status == "101"))
                            {
                                RegisterModel.Req挂号解锁 = new req挂号解锁
                                {
                                    lockId = obj.extend,
                                    extend = RegUnLockExtendModel.ToJsonString(),
                                };
                                RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(RegisterModel.Req挂号解锁);
                                //不处理返回值
                            }
                        }

                    }
                });
            }
            #endregion

        }
        protected string GetDisableText(排班信息 p, string curAmpm)
        {
            string disableText = string.Empty;
            if (p.restnum == "-1")
            {
                disableText = "停诊";
            }
            if (p.restnum == "0")
            {
                disableText = "已满";
            }

            return disableText;
        }
        protected bool GetDisable(排班信息 p, string curAmpm)
        {
            if (p.restnum == "0" || p.restnum == "-1")
            {
                return false;
            }
            return true;
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var reMain = 0m;
            
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Left,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"状态：挂号成功\n");

            #region 个人主要信息
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"出生日期：{patientInfo.birthday.Split(' ')[0]}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            #region  大粗字体,诊疗信息
            sb = new StringBuilder();
            sb.Append($"挂号科室：{department.deptName}\n");
            sb.Append($"挂号序号：{register?.appoNo}号\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"就诊时间：{schedule.medDate} {schedule.medAmPm.SafeToAmPm()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = PrintConfig.HeaderFont2 });
            #endregion

            #region 交易信息
            sb = new StringBuilder();
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"交易金额：{schedule.regAmount.In元()}\n");
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(schedule.regAmount ?? "0");
                    sb.Append($"缴费方式：预缴金\n");
                    sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                    sb.Append($"交易流水号：{register.transNo}\n");
                    sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                    break;
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as BankPosStr;

                    sb.Append($"缴费方式：银行卡\n");
                    sb.Append($"银行卡号：{posinfo.BankNo}\n");
                    sb.Append($"凭证号：{posinfo.POSSeq}\n");
                    sb.Append($"终端编号：{posinfo.TerminalNo}\n");
                    sb.Append($"终端流水：{posinfo.TransSeq}\n");
                    sb.Append($"批次号：{posinfo.ClearDate}\n");
                    sb.Append($"检索参考号：{posinfo.CenterSeq}\n");
                    //sb.Append($"交易日期：{posinfo.TransDateTime}\n");
                    sb.Append($"交易时间：{posinfo.TransDateTime}\n");
                    break;
                case PayMethod.社保:
                    var ybinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    reMain = Convert.ToDecimal(ExtraPaymentModel.ThridRemain); //余额

                    sb.Append($"缴费方式：医保卡\n");
                    sb.Append($"缴费后余额：{reMain.In元()}\n");
                    sb.Append($"卡号：{ybinfo.CardNo}\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();

                    var str1 = string.Empty;
                    var str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Ref))
                    {
                        if (ybinfo.Ref.Length > 12)
                        {
                            str1 = ybinfo.Ref.Substring(0, 12);
                            str2 = ybinfo.Ref.Substring(12, ybinfo.Ref.Length - 12);
                        }
                        else
                        {
                            str1 = ybinfo.Ref;
                        }
                    }

                    sb.Append($"医院支付流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Trace))
                    {
                        if (ybinfo.Trace.Length > 15)
                        {
                            str1 = ybinfo.Trace.Substring(0, 15);
                            str2 = ybinfo.Trace.Substring(15, ybinfo.Trace.Length - 15);
                        }
                        else
                        {
                            str1 = ybinfo.Trace;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    sb.Append($"批次号：{ybinfo.TId}\n");
                    sb.Append($"交易时间：{ybinfo.TransDate + ybinfo.TransTime}\n");
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;

                    sb.Append($"缴费方式：{PaymentModel.PayMethod.ToString()}\n");
                    sb.Append($"支付账号：{thirdpayinfo.buyerAccount}\n");
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n");
                    }
                    sb.Append($"交易时间：{thirdpayinfo.paymentTime}\n");
                    break;
                default:
                    break;
            }
            #endregion

            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            #region 温馨提示
            guahaoSpicalPrint(queue);
            #endregion
            
            return queue;
        }

        protected override Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var sourceModel = SourceModel.所选号源;

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"状态：预约成功\n");

            #region 个人主要信息
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"出生日期：{patientInfo.birthday.Split(' ')[0]}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"就诊卡号：{CardModel.CardNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            #region  大粗字体,诊疗信息
            sb = new StringBuilder();
            sb.Append($"预约科室：{department.deptName}\n");
            sb.Append($"预约医生：{schedule.doctName}\n");
            sb.Append($"就诊地址：{register.address}\n");
            sb.Append($"就诊时间：{schedule.medDate} {schedule.medAmPm.SafeToAmPm()} {sourceModel?.medBegtime}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = PrintConfig.HeaderFont2 });
            #endregion

            #region 交易信息
            sb = new StringBuilder();
            sb.Append($"交易类型：预约挂号\n");

            if (RegisterExtendModel.isPayNow == "1")//已支付
            {
                sb.Append($"支付状态：预约时支付\n");
                sb.Append($"交易金额：{schedule.regAmount.In元()}\n");

                //todo
                reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(schedule.regAmount ?? "0");
                sb.Append($"缴费方式：预缴金\n");
                sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                sb.Append($"交易流水号：{register.transNo}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            }
            else
            {
                sb.Append($"支付状态：取号时支付\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            sb = new StringBuilder();
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"温馨提示：\n");
            sb.Append($"1、请于就诊当天提前半小时取号。\n");
            sb.Append($"2、如需取消，请于就诊时间2小时前取消预约。\n");
            sb.Append($"------------------------------------\n");

            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        protected virtual void guahaoSpicalPrint(Queue<IPrintable> queue)
        {
            var sb = new StringBuilder();
            var schedule = ScheduleModel.所选排班;
            sb.Append($"------------------------------------\n");
            sb.Append($"温馨提示：\n");
            sb.Append($"1、请妥善保管好您的凭条。\n");
            sb.Append($"2、此号当日有效。\n");
            sb.Append($"3、如需打印发票请到挂号收费处，凭此条打印。\n");
            sb.Append($"4、就诊卡存有患者重要信息，如有丢失请立即挂失、补办。\n");
            sb.Append($"------------------------------------\n");
            sb.Append($"祝您早日康复\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
        }
        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;
            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);


            if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡)
            {
                var SiDivide = SiPerPay(schedulInfo);
                if (!SiDivide.IsSuccess)
                {
                    return;
                }
                else
                {
                    PaymentModel.Self = decimal.Parse(SiDivide.Value.output.sumpay.cash)*100m;
                    PaymentModel.Insurance = decimal.Parse(SiDivide.Value.output.sumpay.fund) * 100m;
                }
            }
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",schedulInfo.medDate),
                new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",schedulInfo.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };
            if (ChoiceModel.Business == Business.挂号)
            {
                #region 锁号
                if (ConfigBJ.ScheduleVersion == "1")
                {
                    DoCommand(lp =>
                    {
                        lp.ChangeText("正在锁号，请稍候...");
                        var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                        var scheduleInfo = ScheduleModel.所选排班;
                        var deptInfo = DepartmentModel.所选科室;
                        var doctInfo = DoctorModel.所选医生;

                        RegLockExtendModel.appoNo = SourceModel?.所选号源?.appoNo;
                        RegLockExtendModel.version = ConfigBJ.ScheduleVersion;
                        RegLockExtendModel.regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1";
                        RegLockExtendModel.phone = patientInfo.phone;

                        RegisterModel.Req挂号锁号 = new req挂号锁号
                        {
                            cardNo = CardModel.CardNo,
                            cardType = ((int)CardModel.CardType).ToString(),
                            patientId = patientInfo.patientId,
                            regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                            medDate = scheduleInfo.medDate,
                            scheduleId = scheduleInfo.scheduleId,
                            deptCode = deptInfo.deptCode,
                            doctCode = doctInfo?.doctCode,
                            medAmPm = scheduleInfo.medAmPm,
                            extend = RegLockExtendModel.ToJsonString(),
                        };
                        RegisterModel.Res挂号锁号 = DataHandlerEx.挂号锁号(RegisterModel.Req挂号锁号);
                        if (!RegisterModel.Res挂号锁号?.success ?? false)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}锁号", $"{ChoiceModel.Business}锁号失败\r\n原因：{RegisterModel.Res挂号锁号.msg}", debugInfo: $"{RegisterModel.Res挂号锁号.msg}");
                            return;
                        }
                        Next();
                    });
                }
                else
                {
                    Next();
                }

                #endregion
            }
            else
            {
                QuerySource(i);
            }
        }
        protected override Result Confirm()
        {
            try
            {
                return DoCommand(lp =>
                {
                    lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                    var scheduleInfo = ScheduleModel.所选排班;
                    var deptInfo = DepartmentModel.所选科室;
                    RegisterExtendModel.version = ConfigBJ.ScheduleVersion;
                    RegisterExtendModel.isPayNow = RegLockExtendModel.isCharge ? "1" : "0";
                    RegisterExtendModel.regTypeYT = RegisterModel.Req挂号锁号.regType;

                    RegisterModel.Req预约挂号 = new req预约挂号
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        tradeMode =
                            PaymentModel.NoPay
                                ? PayMethod.预缴金.GetEnumDescription()
                                : PaymentModel.PayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientId,
                        cash = PaymentModel.Total.ToString(),
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = scheduleInfo.hosRegType,
                        medAmPm = scheduleInfo.medAmPm,
                        medDate = scheduleInfo.medDate,
                        deptCode = deptInfo.deptCode,
                        scheduleId = scheduleInfo.scheduleId,
                        appoNo = SourceModel.所选号源?.appoNo,
                        doctCode = scheduleInfo.doctCode,
                        lockId = RegisterModel.Res挂号锁号?.data?.lockId,
                        extend = RegisterExtendModel.ToJsonString(),
                    };

                    FillRechargeRequest(RegisterModel.Req预约挂号);
                    LocalRequest(RegisterModel.Req预约挂号);

                    RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                    if (RegisterModel.Res预约挂号?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约") +
                                (RegLockExtendModel.isCharge ? "\r\n就诊当天，您可直接取号，不需另行支付" : ""),
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables =
                                ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : AppointPrintables(),
                            TipImage = "提示_凭条"
                        });
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);

                        return Result.Success();
                    }
                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    var state = NavigationEngine.State;
                    if (state != A.Third.PosUnion && state != A.Third.SiPay)
                    {
                        //PrintModel.SetPrintInfo(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",errorMsg: RegisterModel.Res预约挂号?.msg);
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",
                            TipMsg = RegisterModel.Res预约挂号?.msg,
                            DebugInfo = RegisterModel.Res预约挂号?.msg
                        });
                        Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    }

                    ExtraPaymentModel.Complete = true;
                    return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
                }).Result;
            }
            catch (Exception ex)
            {
                Logger.Main.Error($"异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                ShowAlert(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败", "发生系统异常 ");
                return Result.Fail("系统异常");
            }
        }

        protected override void FillRechargeRequest(req预约挂号 req)
        {
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as BankPosStr;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.BankNo;
                    req.posTransNo = posinfo.TransSeq;
                    req.bankTransNo = posinfo.CenterSeq;
                    req.deviceInfo = posinfo.TerminalNo;
                    req.sellerAccountNo = posinfo.ComNo;

                    req.accountNo = posinfo?.CenterSeq;
                    req.transNo = posinfo?.TransSeq;

                    req.bankTime = posinfo?.TransDateTime;// DateTime.ParseExact(posinfo?.TransTime, "HHmmss", null).ToString("HHmmss");
                    req.bankDate = posinfo?.ClearDate;// DateTime.ParseExact(posinfo?.TransDate, "MMdd", null).ToString("yyyyMMdd");
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.社保)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    req.bankCardNo = posinfo.CardNo;
                    req.posTransNo = posinfo.MId;
                    req.bankTransNo = posinfo.Ref;
                    req.deviceInfo = posinfo.TId;
                    req.sellerAccountNo = posinfo.Memo;//医保支付方式：银联交易流水号

                    req.accountNo = posinfo?.Trace;
                    req.transNo = posinfo?.Ref;

                    req.bankTime = DateTime.ParseExact(posinfo?.TransTime, "HHmmss", null).ToString("HHmmss");
                    req.bankDate = DateTime.ParseExact(posinfo?.TransDate, "yyyyMMdd", null).ToString("yyyyMMdd");
                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 || extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    req.payAccountNo = thirdpayinfo.buyerAccount;
                    req.transNo = thirdpayinfo.outPayNo;
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }
        }

        protected virtual void LocalRequest(req预约挂号 req)
        {

        }

        public Result<Res费用分解> SiPerPay(排班信息 scheduleInfo)
        {
            #region 准备数据
            var tradeinfo = new Req费用分解.Input.Tradeinfo()
            {
                curetype = scheduleInfo.regType == RegType.急诊门诊.ToString() ? "19" : "17",//就诊类型，门诊挂号17急诊挂号19
                illtype = "0",
                feeno = "",
                operator2 = FrameworkConst.OperatorId
            };
            var recipearray = new Req费用分解.Input.Recipe[]
            {
                new Req费用分解.Input.Recipe(){
                    diagnoseno="1",
                    recipeno="1",
                    recipedate=scheduleInfo.medDate,
                    recipetype="1",
                    helpmedicineflag="0",
                    registertradeno="",
                    billstype="1",
                }
            };
            string amt元 = (decimal.Parse(scheduleInfo.regAmount) / 100m).ToString("F2");
            var feeitemarray = new Req费用分解.Input.Feeitem[]
            {
                new Req费用分解.Input.Feeitem()
                {
                    itemno="1",
                    recipeno="1",
                    hiscode=scheduleInfo.hosRegType,
                    itemname=scheduleInfo.hosRegType,
                    itemtype="1",
                    unitprice=amt元,
                    count="1",
                    fee=amt元,
                    babyflag="0",
                }
            };
            var 费用信息 = new Req费用分解()
            {
                input = new Req费用分解.Input()
                {
                    tradeinfo = tradeinfo,
                    recipearray = recipearray,
                    feeitemarray = feeitemarray,
                }
            };
            #endregion

            SiModel.req费用分解 = 费用信息;
            var 费用分解结果 = SiInterface.Handle<Req费用分解, Res费用分解>(费用信息);
            SiModel.res费用分解 = 费用分解结果.Value;
            if (!费用分解结果.IsSuccess)
            {
                ShowAlert(false,"医保交易", "费用分解失败",debugInfo: 费用分解结果.Value.ErrorMsg ?? "");
            }
            return 费用分解结果;
        }

        public Res交易确认 SiCommit()
        {
            return null;
        }

    }
}
