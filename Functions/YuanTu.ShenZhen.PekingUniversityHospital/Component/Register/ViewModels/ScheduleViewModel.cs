using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.ShenZhen.PekingUniversityHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {
        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };


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
                IsEnabled = p.restnum != "0",
                DisableText = p.restnum == "0" ? "已满" : ""
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;

            //PaymentModel.Date = schedulInfo.medDate;
            //PaymentModel.Time = schedulInfo.medAmPm.SafeToAmPm();
            //PaymentModel.Department = schedulInfo.deptName ?? DeptartmentModel.所选科室?.deptName;
            //PaymentModel.Doctor = schedulInfo.doctName;

            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付            
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
            QuerySource(i);

            //if (ChoiceModel.Business == Business.挂号)
            //    Next();
            //else QuerySource(i);
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var scheduleInfo = ScheduleModel.所选排班;
                var deptInfo = DepartmentModel.所选科室;
                var sourceInfo = SourceModel.所选号源;

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("beginTime", sourceInfo.medBegtime);
                dictionary.Add("endTime", sourceInfo.medEndtime);
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
                    //bankCardNo = pos?.CardNo,
                    //bankTime = pos?.TransTime,
                    //bankDate = pos?.TransDate,
                    //posTransNo = pos?.Trace,
                    //bankTransNo = pos?.Ref,
                    //deviceInfo = pos?.TId,
                    //sellerAccountNo = pos?.MId,
                    //lockId = GuaHao.Res挂号锁号?.data?.lockId
                    extend = dictionary.ToJsonString(),
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
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = ScheduleModel.所选排班.medDate,
                    deptCode = DepartmentModel.所选科室.deptCode,
                    scheduleId = ScheduleModel.所选排班.scheduleId,
                    doctCode = ScheduleModel.所选排班.doctCode
                };

                SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
                if (SourceModel.Res号源明细查询?.success ?? false)
                {
                    if (SourceModel.Res号源明细查询?.data?.Count > 0)
                    {
                        ChangeNavigationContent(i.Title);

                        if (ChoiceModel.Business == Business.挂号)
                            Navigate(A.XC.Time);
                        else
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

        protected override Queue<IPrintable> RegisterPrintables()
        {
#warning 就诊时间问题需要特殊处理！！！
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            //sb.Append($"状态：挂号成功\n");

            var patientInfoimage = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = patientInfoimage,
                Height = patientInfoimage.Height / 1.5f,
                Width = patientInfoimage.Width / 1.5f
            });

            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：现场挂号\n");
            //sb.Append($"排班类型：{paiban.doctTech}\n");
            //sb.Append($"科室名称：{department.parentDeptName}\n");
            sb.Append($"就诊科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> AppointPrintables()
        {
#warning 就诊时间问题需要特殊处理！！！
            var queue = PrintManager.NewQueue("预约挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            var patientInfoimage = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = patientInfoimage,
                Height = patientInfoimage.Height / 1.5f,
                Width = patientInfoimage.Width / 1.5f
            });
            //sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：预约挂号\n");
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