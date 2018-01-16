using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Extension;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;
            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",schedulInfo.medDate),
                new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",schedulInfo.doctName),
            };
            if (ChoiceModel.Business == Business.挂号)
            {
                DoCommand(lp =>
                {
                    lp.ChangeText("开始挂号预处理,处理时间在1-2分钟请稍后...");
                    var reqOpRegPre = new req预约挂号预处理
                    {
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        regDate = ScheduleModel.所选排班.medDate,
                        regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                        medAmPm = ScheduleModel.所选排班.medAmPm,
                        deptCode = ScheduleModel.所选排班.deptCode,
                        doctCode = ScheduleModel.所选排班.doctCode,
                        patientId = PatientModel.当前病人信息.patientId,
                    };
                    var resOpRegPre = DataHandlerEx.预约挂号预处理(reqOpRegPre);
                    if (!resOpRegPre.success)
                    {
                        ShowAlert(false,"温馨提示", $"挂号预处理失败:{resOpRegPre.msg}");
                        return;
                    }
                    PaymentModel.Self = decimal.Parse(resOpRegPre.data.regAmount);
                    PaymentModel.Insurance = decimal.Parse(resOpRegPre.data.treatFee);
                    PaymentModel.Total = decimal.Parse(resOpRegPre.data.regAmount);
                    PaymentModel.NoPay =true; //默认预约或者自费金额为0时不支付            
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.RightList = new List<PayInfoItem>()
                    {
                        new PayInfoItem("总金额：",PaymentModel.Self.In元()),
                        //new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                        //new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                    };
                    Next();
                });
            }
            else
            {
                PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = true; //默认预约或者自费金额为0时不支付            
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.RightList = new List<PayInfoItem>()
                {
                    new PayInfoItem("总金额：",PaymentModel.Self.In元()),
                    //new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                    //new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                };
                QuerySource(i);
            }
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
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
                    tradeMode = "OC",
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm =ChoiceModel.Business==Business.挂号 ? (RegDateModel.AmPm == AmPmSession.上午 ? "1" : "2")
                                                                 : DateTime.Compare(DateTimeCore.Now,DateTime.Parse(SourceModel.所选号源.medBegtime))>0?"1": "2",
                    medDate = scheduleInfo.medDate,
                    deptCode = deptInfo.deptCode,
                    deptName = deptInfo.deptName,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = SourceModel.所选号源?.appoNo,
                    patientName = patientInfo.name,
                    doctCode = scheduleInfo?.doctCode,
                    doctName = scheduleInfo?.doctName,
                    extend = scheduleInfo?.extend,
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

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
    }
}