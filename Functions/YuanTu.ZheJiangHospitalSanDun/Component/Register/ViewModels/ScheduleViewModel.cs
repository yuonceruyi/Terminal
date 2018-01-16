using System.Collections.Generic;
using System.Text;
using Microsoft.Practices.Unity;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Register;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;

namespace YuanTu.ZheJiangHospitalSanDun.Component.Register.ViewModels
{
    class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public IRegDateModel RegDateModel { get; set; }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;

            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.NoPay = true; // 固定用账户余额支付
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",RegDateModel.RegDate),
                //new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",schedulInfo.doctName),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };

            DoCommand(lp =>
            {
                var req = new req挂号锁号()
                {
                    cardNo = CardModel.CardNo,
                    cardType = ((int)CardModel.CardType).ToString(),
                    scheduleId = schedulInfo.scheduleId,
                };
                RegisterModel.Req挂号锁号 = req;
                var res = DataHandlerEx.挂号锁号(req);
                RegisterModel.Res挂号锁号 = res;
                if (res == null || !res.success)
                {
                    ShowAlert(false, "挂号锁号", "挂号锁号失败\n" + res?.msg);
                    return;
                }
                Next();
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(Act).Result;
        }

        private Result Act(LoadingProcesser lp)
        {
            lp.ChangeText("正在进行现场挂号，请稍候...");

            var patientInfo = PatientModel.当前病人信息;
            var scheduleInfo = ScheduleModel.所选排班;
            var deptInfo = DepartmentModel.所选科室;
            var req = new req预约挂号
            {
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,

                patientId = patientInfo.patientId,
                //idNo = patientInfo.idNo,
                //patientName = patientInfo.name,

                //tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                //accountNo = patientInfo.patientId,
                //cash = PaymentModel.Total.ToString(),

                regMode = "2",
                //regType = ((int)RegTypesModel.SelectRegType.RegType).ToString(),
                //medAmPm = scheduleInfo.medAmPm,
                medDate = RegDateModel.RegDate,
                deptCode = deptInfo.deptCode,
                deptName = deptInfo.deptName,

                doctCode = scheduleInfo.doctCode,
                doctName = scheduleInfo.doctName,

                scheduleId = scheduleInfo.scheduleId,
                //appoNo = SourceModel.所选号源?.appoNo,
                lockId = RegisterModel.Res挂号锁号.data.lockId,
            };
            
            RegisterModel.Req预约挂号 = req;
            var res = DataHandlerEx.预约挂号(req);
            RegisterModel.Res预约挂号 = res;
            if (res == null || !res.success)
                return Result.Fail(res?.code ?? -100, res?.msg);

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
            sb.Append($"卡号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"挂号日期：{RegDateModel.RegDate}\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");
            sb.Append($"当前余额：{register.extend.In元()}\n");
            //sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            //sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            //sb.Append($"就诊地址：{register?.address}\n");
            //sb.Append($"挂号序号：{register?.appoNo}\n");
            //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");

            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
