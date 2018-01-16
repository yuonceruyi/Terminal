using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.HuNanHangTianHospital.Common;

namespace YuanTu.HuNanHangTianHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = ScheduleModel.Res排班信息查询.data.Select(p => new InfoMore
            {
                Title = p.doctName.BackNotNullOrEmpty(DepartmentModel.所选科室?.deptName),
                SubTitle = $"{p.medDate.Substring(0,10)} {p.medAmPm.SafeToAmPm()}",
                Type = "挂号费",
                Amount = decimal.Parse(p.regAmount),
                Extends = $"剩余号源 {p.restnum}",
                ConfirmCommand = confirmCommand,
                Tag = p,
                IsEnabled = p.restnum != "0"
            });
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }
        protected override void Confirm(Info i)
        {
            Logger.Main.Info("进入到Confirm(Info i)!!!!!!!!");
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);
            var schedulInfo = ScheduleModel.所选排班;

            if (RegTypesModel.SelectRegType.RegType == RegType.免费挂号)
            {
                Logger.Main.Info("进入到判断!!!!!!!!");
                Task.Run(()=> { Confirm(); });
                return;
            }
            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.NoPay = PaymentModel.Self == 0;         
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
                Next();
            else QuerySource(i);
        }

        protected override Result Confirm()
        {
            Logger.Main.Info("进入到 Confirm2()!!!!!!!!");
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
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
                    regMode = ChoiceModel.Business == Business.挂号 ? "2" : "1",
                    regType = RegTypeConvert.GetRegType(RegTypesModel.SelectRegType.RegType),
                    medAmPm = scheduleInfo.medAmPm,
                    medDate = DateTime.Parse(scheduleInfo.medDate).ToString("yyyy-MM-dd"),
                    medTime = DateTimeCore.Now.ToString("HH:mm:ss"),
                    deptCode = deptInfo.deptCode,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = SourceModel.所选号源?.appoNo,
                    patientName = patientInfo.name,
                    doctCode = scheduleInfo.doctCode,
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
                    if (RegTypesModel.SelectRegType.RegType != RegType.免费挂号)
                    {
                        KY.MoveOutCard();
                    }
                    return Result.Success();
                }
                if (RegTypesModel.SelectRegType.RegType != RegType.免费挂号)
                {
                    KY.MoveOutCard();
                }
                ShowAlert(false, "预约挂号提示", "预约挂号失败");
                Logger.Main.Info("预约挂号失败"+ RegisterModel.Res预约挂号.msg);
                return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
            }).Result;
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在查询医生排班信息，请稍候...");
                SourceModel.Req号源明细查询 = new req号源明细查询
                {
                    operId = FrameworkConst.OperatorId,
                    regMode = "1",
                    regType = RegTypeConvert.GetRegType(RegTypesModel.SelectRegType.RegType),
                    medAmPm = ScheduleModel.所选排班.medAmPm,
                    medDate = DateTime.Parse(ScheduleModel.所选排班.medDate).ToString("yyyy-MM-dd"),
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

        protected override Queue<IPrintable> RegisterPrintables()
        {
#warning 就诊时间问题需要特殊处理！！！
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            //sb.Append($"排班类型：{paiban.doctTech}\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            sb.Append($"就诊时间：{ DateTime.Parse(schedule.medDate).ToString("yyy--MM-dd")}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"挂号序号：{register?.visitNo}\n");
            //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"仅限当日有效！\n");
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
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            //sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            //sb.Append($"挂号金额：{paiban.regAmount.In元()}\n");
            sb.Append($"就诊时间：{ DateTime.Parse(schedule.medDate).ToString("yyy--MM-dd")}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register.address}\n");
            sb.Append($"挂号序号：{register.visitNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
