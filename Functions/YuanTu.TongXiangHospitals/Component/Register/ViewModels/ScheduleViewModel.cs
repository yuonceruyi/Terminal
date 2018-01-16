using Microsoft.Practices.Unity;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Navigating;
using YuanTu.TongXiangHospitals.HealthInsurance;
using YuanTu.TongXiangHospitals.HealthInsurance.Model;
using YuanTu.TongXiangHospitals.HealthInsurance.Service;
using Prism.Regions;
using Prism.Commands;
using System.Collections.ObjectModel;
using YuanTu.Consts.Sounds;


namespace YuanTu.TongXiangHospitals.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }
       

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = ScheduleModel.Res排班信息查询.data.Select(p => new InfoMore
            {
                Title = p.doctName.BackNotNullOrEmpty(DepartmentModel.所选科室?.deptName),
                SubTitle = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.medAmPm.SafeToAmPm()}",
                Type = "挂号费",
                Amount = decimal.Parse(p.regAmount),
                Extends = $"剩余号源 {p.restnum}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.restnum != "0"
            });
            Data = new ObservableCollection<InfoMore>(list);

            if (RegTypesModel.SelectRegType.RegType == RegType.普通门诊)
                return;
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            if (ChoiceModel.Business == Business.挂号)
            {
                //挂号预结算
                SiModel.诊间结算 = false;
                DoCommand(cxt =>
                {
                    cxt.ChangeText("正在进行门诊挂号预结算，请稍候...");
                    var result = PreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    Next();
                });
            }
            else
            {
                var schedulInfo = ScheduleModel.所选排班;
                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = true;
                PaymentModel.ConfirmAction = Confirm;

                //PaymentModel.LeftList = new List<PayInfoItem>()
                //{
                //new PayInfoItem("日期：",schedulInfo.medDate),
                //new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                //new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                //new PayInfoItem("医生：",schedulInfo.doctName),
                //};

                //PaymentModel.RightList = new List<PayInfoItem>()
                //{
                //new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                //new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                //new PayInfoItem("总计金额：",PaymentModel.Amount.In元(),true),
                //};
                PaymentModel.MidList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：",schedulInfo.medDate),
                    new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：",schedulInfo.doctName),
                    new PayInfoItem("挂号费：", PaymentModel.Total.In元()),
                    new PayInfoItem("预约不收取任何费用，","取号时扣除挂号费",true)
                };
                QuerySource(i);
            }
        }

        public virtual Result PreSettle()
        {
            var schedulInfo = ScheduleModel.所选排班;

            if (CardModel.CardType == CardType.社保卡)
            {
                var result = SiPreSettle();
                if (!result.IsSuccess)
                {
                    return Result.Fail(result.Message);
                }

                var resOpRegPre = SiModel.门诊挂号预结算结果确认结果;
                PaymentModel.Self = decimal.Parse(resOpRegPre.selfFee);
                PaymentModel.Insurance = decimal.Parse(resOpRegPre.insurFee);
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = PaymentModel.Self == 0;
                PaymentModel.ConfirmAction = Confirm;
            }
            else
            {
                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = PaymentModel.Self == 0;
                PaymentModel.ConfirmAction = Confirm;
            }

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
                new PayInfoItem("总计金额：",PaymentModel.Total.In元(),true),
            };
            return Result.Success();
        }

        public virtual Result SiPreSettle()
        {
            //return DoCommand(ctx =>
            // {
            //ctx.ChangeText("正在进行门诊挂号预结算，请稍候...");
            //todo 请求HIS预结算
            var reqOpRegPre = new req门诊挂号预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medDate = ScheduleModel.所选排班.medDate,
                scheduleId = ScheduleModel.所选排班.scheduleId,
                deptCode = DepartmentModel.所选科室.deptCode,
                doctCode = ScheduleModel.所选排班.doctCode,
                medAmPm = ScheduleModel.所选排班.medAmPm,
                cash = ScheduleModel.所选排班.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPre = DataHandlerEx.门诊挂号预结算(reqOpRegPre);
            if (!resOpRegPre.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算失败", debugInfo: resOpRegPre.msg);
                return Result.Fail(resOpRegPre.msg);
            }
            SiModel.门诊挂号预结算结果 = resOpRegPre.data;
            //ctx.ChangeText("正在进行医保预结算，请稍候...");
            //todo 医保预结算

            var result = SiModel.诊间结算 ? SiService.OpRegClinicPreSettle(SiModel.门诊挂号预结算结果.insurFeeInfo)
                                        : SiService.OpRegPreSettle(SiModel.门诊挂号预结算结果.insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }
            //ctx.ChangeText("正在进行门诊挂号预结算结果确认，请稍候...");
            //todo HIS预结算确认

            var reqOpRegPreConfirm = new req门诊挂号预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medDate = ScheduleModel.所选排班.medDate,
                scheduleId = ScheduleModel.所选排班.scheduleId,
                deptCode = DepartmentModel.所选科室.deptCode,
                doctCode = ScheduleModel.所选排班.doctCode,
                medAmPm = ScheduleModel.所选排班.medAmPm,
                cash = ScheduleModel.所选排班.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPreConfirm = DataHandlerEx.门诊挂号预结算结果确认(reqOpRegPreConfirm);
            if (!resOpRegPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpRegPreConfirm.msg);
                return Result.Fail(resOpRegPreConfirm.msg);
            }
            SiModel.门诊挂号预结算结果确认结果 = resOpRegPreConfirm.data;
            return Result.Success();
            //});
        }

        protected virtual Result GetSiSettleReq()
        {
            var reqOpRegPre = new req门诊挂号预结算
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medDate = ScheduleModel.所选排班.medDate,
                scheduleId = ScheduleModel.所选排班.scheduleId,
                deptCode = DepartmentModel.所选科室.deptCode,
                doctCode = ScheduleModel.所选排班.doctCode,
                medAmPm = ScheduleModel.所选排班.medAmPm,
                cash = ScheduleModel.所选排班.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPre = DataHandlerEx.门诊挂号预结算(reqOpRegPre);
            if (!resOpRegPre.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算失败", debugInfo: resOpRegPre.msg);
                return Result.Fail(resOpRegPre.msg);
            }
            SiModel.门诊挂号预结算结果 = resOpRegPre.data;

            //todo 医保预结算

            var result = SiModel.诊间结算 ? SiService.OpRegClinicPreSettle(SiModel.门诊挂号预结算结果.insurFeeInfo)
                                        : SiService.OpRegPreSettle(SiModel.门诊挂号预结算结果.insurFeeInfo);
            if (!result.IsSuccess)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号医保预结算失败", debugInfo: result.Message);
                return Result.Fail(result.Message);
            }

            //todo HIS预结算确认

            var reqOpRegPreConfirm = new req门诊挂号预结算结果确认
            {
                patientId = PatientModel.当前病人信息.patientId,
                cardNo = CardModel.CardNo,
                cardType = ((int)CardModel.CardType).ToString(),
                regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medDate = ScheduleModel.所选排班.medDate,
                scheduleId = ScheduleModel.所选排班.scheduleId,
                deptCode = DepartmentModel.所选科室.deptCode,
                doctCode = ScheduleModel.所选排班.doctCode,
                medAmPm = ScheduleModel.所选排班.medAmPm,
                cash = ScheduleModel.所选排班.regAmount,
                ybCardNo = CardModel.CardNo,
                ybTradeType = SiModel.医保个人基本信息?.医保待遇类别,
                isZj = SiModel.诊间结算 ? "1" : "0",
                insurFeeInfo = SiModel.RetMessage,
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo
            };
            var resOpRegPreConfirm = DataHandlerEx.门诊挂号预结算结果确认(reqOpRegPreConfirm);
            if (!resOpRegPreConfirm.success)
            {
                ShowAlert(false, "门诊挂号预结算", "门诊挂号预结算结果确认失败", debugInfo: resOpRegPreConfirm.msg);
                return Result.Fail(resOpRegPreConfirm.msg);
            }
            SiModel.门诊挂号预结算结果确认结果 = resOpRegPreConfirm.data;
            return Result.Success();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                //todo 医保结算

                if (CardModel.CardType == CardType.社保卡 && ChoiceModel.Business == Business.挂号)
                {
                    lp.ChangeText("正在进行医保结算，请稍候...");
                    if (SiModel.诊间结算)
                    {
                        var result = GetSiSettleReq();
                        if (!result.IsSuccess)
                        {
                           return Result.Fail(result.Message);
                        }
                    }

                    var res = SiModel.诊间结算 ? SiService.OpRegClinicSettle(SiModel.门诊挂号预结算结果确认结果?.insurFeeInfo)
                                               : SiService.OpRegSettle(SiModel.门诊挂号预结算结果确认结果?.insurFeeInfo);
                    if (!res.IsSuccess)
                    {
                        ShowAlert(false, "门诊挂号医保结算", "门诊挂号医保结算失败", debugInfo: res.Message);
                        return Result.Fail(res.Message);
                    }
                }

                //todo HIS挂号
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

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
                    tradeMode = GetPayMethod(PaymentModel.PayMethod),
                    accountNo = patientInfo.patientId,
                    cash = (Startup.TestRefund ? PaymentModel.Self + 1 : PaymentModel.Self).ToString(CultureInfo.InvariantCulture),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = scheduleInfo.medAmPm,
                    medDate = scheduleInfo.medDate,
                    deptCode = deptInfo.deptCode,
                    doctCode = scheduleInfo.doctCode,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = SourceModel.所选号源?.appoNo,
                    patientName = patientInfo.name,
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                if (CardModel.CardType == CardType.社保卡 && ChoiceModel.Business == Business.挂号)
                    FillSiRequest(RegisterModel.Req预约挂号);

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);

                    return Result.Success();
                }

                if (CardModel.CardType == CardType.社保卡 && ChoiceModel.Business == Business.挂号 && SiModel.诊间结算)
                {
                    if (DataHandler.UnKnowErrorCode.Any(p => p == RegisterModel.Res预约挂号?.code))
                    {
                        //todo 打印医保单边账凭条
                        if (PaymentModel.Insurance > 0)
                        {
                            var errorMsg = $"医保消费成功，网关返回未知结果{RegisterModel.Res预约挂号?.code.ToString()}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                            医保单边账凭证(errorMsg);
                        }
                    }
                    else if (RegisterModel.Res预约挂号?.data?.extend != null)
                    {
                        //todo 医保退费
                        var result = SiService.OpRegClinicSettleRefund(RegisterModel.Res预约挂号?.data?.extend);
                        if (!result.IsSuccess)
                        {
                            //todo 医保退费失败处理
                            ShowAlert(false, "医保门诊挂号诊间结算退费", "医保门诊挂号诊间结算退费失败", debugInfo: result.Message);
                        }
                    }
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",
                        DebugInfo = RegisterModel.Res预约挂号?.msg
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                }
                ExtraPaymentModel.Complete = true;
                return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
            }).Result;
        }

        public virtual string GetPayMethod(PayMethod payMethod)
        {
            return payMethod == PayMethod.预缴金 ? "SMK" : payMethod.GetEnumDescription();
        }

        public virtual void FillSiRequest(req预约挂号 req)
        {
            req.extend = SiModel.RetMessage;
            req.ybInfo = new SiInfo
            {
                cardHardInfo = SiModel.CardHardInfo,
                siPatientInfo = SiModel.SiPatientInfo,
                transNo = SiModel.门诊挂号预结算结果确认结果?.transNo
            }.ToJsonString();
            //req.transNo = SiModel.门诊挂号预结算结果确认结果?.transNo;
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                var bus = GetInstance<IBusinessConfigManager>();
                lp.ChangeText("正在查询号源信息，请稍候...");
                SourceModel.Req号源明细查询 = new req号源明细查询
                {
                    operId = FrameworkConst.OperatorId,
                    regMode = "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = ScheduleModel.所选排班.medDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
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

        protected virtual void 医保单边账凭证(string errorMsg)
        {
            var queue = PrintManager.NewQueue($"医保{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo?.CardNo}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"排班编号：{ScheduleModel.所选排班.scheduleId}\n");
            sb.Append($"医保金额：{PaymentModel?.Insurance.In元()}\n");
            sb.Append($"异常描述：{errorMsg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导医处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            PrintModel.PrintInfo = new PrintInfo
            {
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = queue
            };
            PrintManager.Print();
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
         
            var preReg = SiModel.门诊挂号预结算结果确认结果;
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号码：{patientInfo.platformId}\n");
            sb.Append($"交易类型：现场挂号\n");
            //sb.Append($"科室名称：{department.parentDeptName}\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            if (preReg != null)
            {
                sb.Append($"个人支付：{preReg?.selfFee.In元()}\n");
                sb.Append($"医保报销：{preReg?.insurFee.In元()}\n");
            }
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"排队号：{register?.visitNo}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"流水号：{register?.appoNo}\n");
            sb.Append($"发票号：{register?.receiptNo}\n");

            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
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
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"卡号：{CardModel.CardNo}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"科室名称：{department.parentDeptName}\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            //sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            //sb.Append($"挂号金额：{paiban.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register.address}\n");
            sb.Append($"挂号序号：{register.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}