using System;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.TaiZhouCentralHospital.Component.Register.Models;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Model;
using YuanTu.TaiZhouCentralHospital.HealthInsurance.Service;

namespace YuanTu.TaiZhouCentralHospital.Component.Register
{
    
    public class SourceViewModel : Default.Component.Register.ViewModels.SourceViewModel
    {
        [Dependency]
        public ISiRegHelpModel SiRegHelpModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = SourceModel.Res号源明细查询.data.OrderBy(p => p.appoNo).Select(p => new InfoMore
            {
                Title = $"时间 {p.medBegtime}", //这里开始时间和结束时间相同
                Type = $"序号 {p.appoNo}",
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<InfoMore>(list);
            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号号源 : SoundMapping.选择预约号源);
        }

        protected override void Confirm(Info i)
        {
            SourceModel.所选号源 = i.Tag.As<号源明细>();

            var medTime = DateTime.Parse(SourceModel.所选号源.medBegtime);
            var nowHour = DateTimeCore.Now.Hour;
            if ((nowHour < 12 && medTime.Hour >= 12) || nowHour >= 12 && medTime.Hour < 12)
            {
                var nowStr = nowHour < 12 ? "上午" : "下午";
                var medStr = medTime.Hour < 12 ? "上午" : "下午";
                ShowAlert(false, "温馨提示", $"{nowStr}不能挂{medStr}的号");
                return;
            }


            ChangeNavigationContent(i.Title);

            DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡)
                {
                    //todo 医保预结算
                    lp.ChangeText("正在进行医保预结算,请稍候...");
                    var result = SiPreSettle();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                    var resOpRegPre = SiModel.医保预结算结果;
                    PaymentModel.Self = decimal.Parse(resOpRegPre.合计现金支付) * 100;
                    PaymentModel.Insurance = decimal.Parse(resOpRegPre.合计报销金额) * 100;
                    PaymentModel.Total = decimal.Parse(resOpRegPre.费用总额) * 100;
                    PaymentModel.NoPay = PaymentModel.Self == 0;
                    PaymentModel.ConfirmAction = Confirm;
                }
                else
                {
                    var schedulInfo = ScheduleModel.所选排班;
                    PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                    PaymentModel.Insurance = decimal.Parse("0");
                    PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                    PaymentModel.NoPay = PaymentModel.Self == 0 || ChoiceModel.Business == Business.预约;
                    PaymentModel.ConfirmAction = Confirm;
                }
                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", ScheduleModel.所选排班.medDate),
                    new PayInfoItem("时间：", ScheduleModel.所选排班.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", ScheduleModel.所选排班.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", ScheduleModel.所选排班.doctName)
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

        public virtual Result SiPreSettle()
        {
            //todo HIS预约挂号
            var patientInfo = PatientModel.当前病人信息;
            var scheduleInfo = ScheduleModel.所选排班;
            var deptInfo = DepartmentModel.所选科室;
            var reqAppoint = new req预约挂号
            {
                patientId = patientInfo.patientId,
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                idNo = patientInfo.idNo,
                tradeMode = null,
                accountNo = patientInfo.patientId,
                cash = scheduleInfo.regAmount,
                regMode = "1",
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medAmPm = scheduleInfo.medAmPm,
                medDate = scheduleInfo.medDate,
                deptCode = deptInfo.deptCode,
                deptName = deptInfo.deptName,
                scheduleId = scheduleInfo.scheduleId,
                appoNo = SourceModel.所选号源?.appoNo,
                patientName = patientInfo.name,
                doctCode = scheduleInfo?.doctCode,
                doctName = scheduleInfo?.doctName,
                medTime = SourceModel.所选号源?.medBegtime,
                extend = patientInfo.patientType
            };

            SiRegHelpModel.ResAppoint = DataHandlerEx.预约挂号(reqAppoint);
            if (!SiRegHelpModel.ResAppoint.success)
            {
                return Result.Fail($"门诊挂号预处理失败:{SiRegHelpModel.ResAppoint.msg}");
            }
            SiRegHelpModel.CancelAppoint =CancelAppoint;
            //todo HIS提供医保预结算入参
            var reqOpRegPre = new req预约挂号预处理
            {
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                regDate = ScheduleModel.所选排班.medDate,
                regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                medAmPm = ScheduleModel.所选排班.medAmPm,
                deptCode = ScheduleModel.所选排班.deptCode,
                doctCode = ScheduleModel.所选排班.doctCode,
                appoNo = SiRegHelpModel.ResAppoint.data?.orderNo,
                patientId = PatientModel.当前病人信息.patientId,
                extend = $"{SiModel.SiPatientInfo}"
            };
            var resOpRegPre = DataHandlerEx.预约挂号预处理(reqOpRegPre);
            if (!resOpRegPre.success)
            {
                SiRegHelpModel.CancelAppoint?.Invoke();
                return Result.Fail($"门诊挂号预处理失败:{resOpRegPre.msg}");
            }
           
            SiModel.门诊挂号预处理结果 = resOpRegPre.data;

            //todo 医保预结算

            var result = SiService.OpPreSettle(SiModel.门诊挂号预处理结果.regFee);
            if (!result.IsSuccess)
            {
                SiRegHelpModel.CancelAppoint?.Invoke();
                return Result.Fail($"门诊挂号医保预结算失败:{result.Message}");
            }
           
            return Result.Success();
        }

        public virtual Result Confirm()
        {
            return DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡)
                {
                    lp.ChangeText("正在进行医保结算,请稍候...");
                    //todo 医保结算
                    var result = SiService.OpSettle(SiModel.门诊挂号预处理结果.treatFee);
                    if (!result.IsSuccess)
                    {
                        SiRegHelpModel.CancelAppoint?.Invoke();
                        return Result.Fail($"门诊挂号医保结算失败:{result.Message}");
                    }
                    
                }

                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
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
                    doctName = scheduleInfo?.doctName,
                    medTime = SourceModel.所选号源?.medBegtime,
                    extend = patientInfo.patientType
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡)
                {
                    FillSiRechargeRequest(RegisterModel.Req预约挂号);
                }

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg =
                            $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分" +
                            (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);

                    return Result.Success();
                }
                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡)
                {
                    SiRegHelpModel.CancelAppoint?.Invoke();
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

        protected virtual void FillRechargeRequest(req预约挂号 req)
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
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
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

        protected virtual void FillSiRechargeRequest(req预约挂号 req)
        {
            req.ybInfo = SiModel.医保结算结果字符串;
        }


        protected virtual Result CancelAppoint()
        {

            Task.Run(()=>DataHandlerEx.取消预约(new req取消预约
            {
                appoNo = SiRegHelpModel.ResAppoint.data?.appoNo,
                orderNo = SiRegHelpModel.ResAppoint.data?.orderNo,
                patientId = PatientModel.当前病人信息?.patientId,
                operId = FrameworkConst.OperatorId,
                regMode = "1",
                cardNo = CardModel.CardNo,
                cardType = ((int) CardModel.CardType).ToString(),
#pragma warning disable 612
                medDate = ScheduleModel.所选排班?.medDate,
                scheduleId = ScheduleModel.所选排班?.scheduleId,
                medAmPm = ScheduleModel.所选排班?.medAmPm,
                regNo = SiRegHelpModel.ResAppoint.data?.regFlowId,
#pragma warning restore 612
                extend = $"{ScheduleModel.所选排班?.deptCode}|{ScheduleModel.所选排班?.doctCode}"
            }));
            return Result.Success();
        }

        protected virtual Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            //sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{department?.extend}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            sb.Append($"发票号：{register?.receiptNo}\n");
            //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约挂号单");
            var register = RegisterModel.Res预约挂号?.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约挂号\n");

            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");

            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{department.extend}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        #region Dependency

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }

        [Dependency]
        public IRegTypesModel RegTypesModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public ISiService SiService { get; set; }

        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        #endregion Dependency
    }
}