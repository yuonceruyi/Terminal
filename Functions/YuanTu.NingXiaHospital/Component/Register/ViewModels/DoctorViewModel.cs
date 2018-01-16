using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
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
using YuanTu.NingXiaHospital.HisService;

namespace YuanTu.NingXiaHospital.Component.Register.ViewModels
{
    public class DoctorViewModel : Default.Component.Register.ViewModels.DoctorViewModel
    {
        [Dependency]
        public ISourceModel SourceModel { get; set; }

        [Dependency]
        public IDeptartmentModel DepartmentModel { get; set; }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        [Dependency]
        public IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var res = ResourceEngine;
            var list = DoctorModel.Res查询所有医生信息.data.Select(p => new Info
            {
                Title = p.doctName,
                Tag = p,
                ConfirmCommand = confirmCommand
            });
            Data = new ObservableCollection<Info>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            DoctorModel.医生介绍 = i.Tag.As<医生介绍>();

            DoCommand(lp =>
            {
                lp.ChangeText("正在查询排班信息，请稍候...");
                //宁夏特殊处理：出现了特殊号  ||doctCode作为排班标志  dectspec作为金额 doctIntro作为特殊描述 doctProfe为日期 doctlevel作为medampm
                if (DoctorModel.医生介绍.doctIntro == "#特殊#")
                {
                    ScheduleModel.Res排班信息查询 = new res排班信息查询();
                    ScheduleModel.Res排班信息查询.success = true;
                    ScheduleModel.Res排班信息查询.data = new List<排班信息>();
                    ScheduleModel.Res排班信息查询.data.Add(new 排班信息
                    {
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        deptName = DeptartmentModel.所选科室.deptName,
                        regAmount = DoctorModel.医生介绍.doctSpec,
                        medDate = DoctorModel.医生介绍.doctProfe,
                        medAmPm = DoctorModel.医生介绍.doctLevel,
                        scheduleId = DoctorModel.医生介绍.doctCode
                    });
                }
                else
                {
                    ScheduleModel.排班信息查询 = new req排班信息查询
                    {
                        regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                        deptCode = DeptartmentModel.所选科室.deptCode,
                        parentDeptCode = DeptartmentModel.所选科室.parentDeptCode,
                        doctCode = DoctorModel.医生介绍.doctCode,
                        startDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate,
                        endDate =
                            ChoiceModel.Business == Business.挂号
                                ? DateTimeCore.Today.ToString("yyyy-MM-dd")
                                : RegDateModel.RegDate
                    };
                    ScheduleModel.Res排班信息查询 = DataHandlerEx.排班信息查询(ScheduleModel.排班信息查询);
                }

                if (ScheduleModel.Res排班信息查询?.success ?? false)
                    if (ScheduleModel.Res排班信息查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);
                        var schedulInfo = ScheduleModel.Res排班信息查询.data[0];
                        ScheduleModel.所选排班 = schedulInfo;
                        PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                        PaymentModel.Insurance = decimal.Parse("0");
                        PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                        PaymentModel.NoPay =
                            ChoiceModel.Business == Business.预约 ||
                            PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付            
                        PaymentModel.ConfirmAction = PayConfirm;

                        PaymentModel.LeftList = new List<PayInfoItem>
                        {
                            new PayInfoItem("日期：", schedulInfo.medDate),
                            new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                            new PayInfoItem("科室：", schedulInfo.deptName ?? DepartmentModel.所选科室?.deptName),
                            new PayInfoItem("医生：", DoctorModel.医生介绍.doctName)
                        };

                        PaymentModel.RightList = new List<PayInfoItem>
                        {
                            new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                            new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                            new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
                        };
                        Next();
                    }
                    else
                    {
                        ShowAlert(false, "排班列表查询", "没有获得排班信息(列表为空)");
                    }
                else
                    ShowAlert(false, "排班列表查询", "没有获得排班信息", debugInfo: ScheduleModel.Res排班信息查询?.msg);
            });
        }


        protected Result PayConfirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var scheduleInfo = ScheduleModel.所选排班;
                var deptInfo = DepartmentModel.所选科室;
                RegisterModel.Req预约挂号 = new req预约挂号
                {
                    patientId = patientInfo.patientType == "2" ? "" : patientInfo.patientId,
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
                    extend = DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号成功" : "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分" +
                                 (ChoiceModel.Business == Business.挂号 ? "挂号" : "预约"),
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = ChoiceModel.Business == Business.挂号 ? RegisterPrintables() : null,
                        TipImage = "提示_凭条"
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                    return Result.Success();
                }
                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    //PrintModel.SetPrintInfo(false, ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败", errorMsg: RegisterModel.Res预约挂号?.msg);
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

        protected Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var id = patientInfo.patientType == "2" ? Common.GetNewPatientId("1", CardModel.CardNo) : patientInfo.patientId;
            var sb = new StringBuilder();
            sb.Append($"{RegTypesModel.SelectRegTypeName}   {schedule.medAmPm}坐诊\n");
            sb.Append($"{department.deptName}   {DoctorModel.医生介绍.doctName}\n");
            sb.Append($"{schedule.regAmount.In元()}   {EcanRmb.CmycurD(schedule.regAmount.InRMB())}\n");
            sb.Append($"医保：0  现金{schedule.regAmount.In元()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center });
            sb.Clear();
            sb.Append($"您目前在第{register.appoNo}位\n");
            var temp = string.IsNullOrEmpty(DoctorModel?.医生介绍?.deptCode)
                ? department.deptName
                : DoctorModel?.医生介绍?.deptCode+"诊室";
            sb.Append($"请到{temp}就诊\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center, Font = new Font("微软雅黑", 16, FontStyle.Bold) });
            sb.Clear();
            sb.Append($"病人ID：{id}\n");
            sb.Append($"{patientInfo.name} {patientInfo.sex ?? "男"}  {patientInfo.birthday}\n");
            sb.Append($"{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"祝你早日康复！\n");
            sb.Append($"操作员{FrameworkConst.OperatorId}\n");
            sb.Append($"此号{schedule.medAmPm}有效\n");
            sb.Append($"门诊处方三日内有效\n");
            sb.Append($"如需发票就诊结束后到结算处打印\n");
            sb.Append($"\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), StringFormat = PrintConfig.Center });
            sb.Clear();
            sb.Append($"——————————————\n");
            sb.Append($"  就诊医嘱提示:请您按此核对：\n");
            sb.Append($"  交费，取药，检查，治疗\n");
            sb.Append($"  西药口  中成药口  中草药口\n");
            sb.Append($"  检验口  放  射口   B  超口\n");
            sb.Append($"  心电口  输  液口  治  疗口\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            sb.Append($"  其他：\n");
            sb.Append($"\n");
            sb.Append($"——————————————");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }


    }
}