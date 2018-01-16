using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Gateway;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.FuYangRMYY.HisNative;
using YuanTu.FuYangRMYY.HisNative.Models;
using YuanTu.FuYangRMYY.Managers;
using YuanTu.FuYangRMYY.Services;

namespace YuanTu.FuYangRMYY.Component.Register
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            SiOperatorManager.StopMonitor();
            return base.OnLeaving(navigationContext);
        }

        private void PasswordNotify()
        {
            var sm = GetInstance<IShellViewModel>();
            if (!sm.Busy.IsBusy)
            {
                sm.Busy.IsBusy = true;
            }
            sm.Busy.BusyContent = "请输入6位社保卡密码，并按“确认”键";
            PlaySound("请输入社保卡密码并按确认键结束");
        }

        protected override void Confirm(Info i)
        {
            DoCommand(lp =>
            {
               
                ScheduleModel.所选排班 = i.Tag.As<排班信息>();
                ChangeNavigationContent(i.Title);
                var schedulInfo = ScheduleModel.所选排班;
                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                ////社保
                //if (CardModel.CardType == CardType.社保卡)
                //{
                //    var ret = PerPayWithSi(lp);
                //    if (!ret.IsSuccess)
                //    {
                //        ShowAlert(false, "挂号交易", "社保交易失败", debugInfo: ret.Message);
                //        return;
                //    }
                //    //1^0  代表的是自费支付0元，6^0 代表的是医保基金支付0元, 31^8 代表的是医保账户支付8元
                //    var register = RegisterModel as Models.RegisterModel;
                //    var selfPay =decimal.Parse(register.社保挂号结算.自费金额.Split('^')[1])*100;
                //    var siFund =decimal.Parse(register.社保挂号结算.医保基金.Split('^')[1])*100;
                //    var siSelf =decimal.Parse(register.社保挂号结算.医保账户.Split('^')[1])*100;
                //    PaymentModel.Self = selfPay;
                //    PaymentModel.Insurance = siFund + siSelf;
                //}

                PaymentModel.ConfirmAction = Confirm;
                PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", schedulInfo.medDate),
                    new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.doctName)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    //new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                    //new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                };

                Next();
            });
           

        }

        private Result<社保挂号结算> PayWithSi(LoadingProcesser lp)
        {
            SiOperatorManager.StartMonitor(PasswordNotify);
            lp.ChangeText("正在从HIS系统获取社保基础参数，请稍后...");
            var insurancepara = HisService.GetRegisterInsuranceParams(PatientModel.当前病人信息, ScheduleModel.所选排班.scheduleId);
            if (!insurancepara.IsSuccess)
            {
                return Result<社保挂号结算>.Fail($"从HIS系统中获取社保参数失败!\r\n{insurancepara.Message}");
                // ShowAlert(false, "排班信息", "从HIS系统中获取社保参数失败！", debugInfo: insurancepara.Message);
            }
            lp.ChangeText("正在社保交易，请稍后...");
            var resp = HisInsuranceService.InsuOPReg(0, "1", ScheduleModel.所选排班.scheduleId, "1",
                "26", insurancepara.Value.ExtStr);
            var register = RegisterModel as Models.RegisterModel;
            register.社保挂号结算 = resp.Value;
            register.ExpString = insurancepara.Value.ExtStr;
            SiOperatorManager.StopMonitor();
            return resp;
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                object extendinfo = null;
                if (ChoiceModel.Business == Business.挂号&&CardModel.CardType == CardType.社保卡 && extraPaymentModel.CurrentPayMethod == PayMethod.社保)
                {
                    var rest = PayWithSi(lp);
                    if (!rest)
                    {
                        ShowAlert(false, "社保缴费", rest.Message);
                        return rest.Convert();
                    }
                    //1^0  代表的是自费支付0元，6^0 代表的是医保基金支付0元, 31^8 代表的是医保账户支付8元
                    var register = RegisterModel as Models.RegisterModel;
                    var selfPay = decimal.Parse(register.社保挂号结算.自费金额.Split('^')[1]) * 100;
                    var siFund = decimal.Parse(register.社保挂号结算.医保基金.Split('^')[1]) * 100;
                    var siSelf = decimal.Parse(register.社保挂号结算.医保账户.Split('^')[1]) * 100;
                    PaymentModel.Self = selfPay;
                    PaymentModel.Insurance = siFund + siSelf;
                    extendinfo = new
                    {
                        PayFee=PaymentModel.Total.ToString("0"),
                        PayInsuFeeStr= rest.Value.OriginStr.Replace(rest.Value.DataSplit, '#')
                    };
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
                    regType = null,
                    medAmPm = scheduleInfo.medAmPm,
                    medDate = scheduleInfo.medDate,
                    deptCode = deptInfo.deptCode,
                    deptName = deptInfo.deptName,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = SourceModel.所选号源?.appoNo,
                    patientName = patientInfo.name,
                    doctCode = scheduleInfo?.doctCode,
                    doctName = scheduleInfo?.doctName,
                    extend = extendinfo?.ToJsonString(),
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分" + (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    return Result.Success();
                }
                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡 && extraPaymentModel.CurrentPayMethod == PayMethod.社保)
                {
                    if (DataHandler.UnKnowErrorCode.Contains(RegisterModel.Res预约挂号.code))
                    {
                        打印网关未知异常凭证_社保($"社保扣费结算成功，网关返回未知结果");
                    }
                    else
                    {
                        SiOperatorManager.StartMonitor(PasswordNotify);
                        lp.ChangeText("正在社保退费，请稍后...");
                        var register = RegisterModel as Models.RegisterModel;
                        var rowId = register.社保挂号结算.OriginStr.Split('!')[0].Split('^')[1];
                        var resp = HisInsuranceService.OPRegDestroy(0, "1", rowId, "1", "26", register.ExpString);
                        if (!resp)
                        {
                            ShowAlert(false, "提示", "社保卡挂号撤销失败！", debugInfo: resp.Message);

                        }
                        SiOperatorManager.StopMonitor();
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
        protected override void FillRechargeRequest(req预约挂号 req)
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
                    req.outTradeNo = thirdpayinfo.outTradeNo;
                    req.tradeTime = thirdpayinfo.paymentTime;
                }
            }else if (extraPaymentModel.CurrentPayMethod == PayMethod.社保)
            {
                
            }
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.当前病人信息;
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            //院方要求上午显示序号，下午不需要
            if (schedule.medAmPm.SafeToAmPmEnum() == AmPmSession.上午)
            {
                sb.Append($"挂号序号：{register?.appoNo}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"机器编号：{FrameworkConst.OperatorId}\n");
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
            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"就诊时间：{schedule.medDate}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register.address}\n");
            sb.Append($"挂号序号：{register.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"机器编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected virtual void 打印网关未知异常凭证_社保(string 异常描述)
        {
            var register = RegisterModel as Models.RegisterModel;
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"登记号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"社保串：{register.社保挂号结算?.OriginStr}\n");
            sb.Append($"异常描述：{异常描述}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"机器编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}