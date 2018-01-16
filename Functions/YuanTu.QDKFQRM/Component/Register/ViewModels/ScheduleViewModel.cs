using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.QDKouQiangYY.Component.Register.Services;
using YuanTu.Consts.Models.Payment;

namespace YuanTu.QDKFQRM.Component.Register.ViewModels
{
    public class ScheduleViewModel: YuanTu.QDKouQiangYY.Component.Register.ViewModels.ScheduleViewModel
    {
        public bool isFree;

        public override void OnEntered(NavigationContext navigationContext)
        {
            isFree = PatientModel.当前病人信息.birthday.Age() >= 60 &&
                     !PatientModel.当前病人信息.idNo.IsNullOrWhiteSpace();
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var curAmpm = DateTimeCore.Now.Hour >= 12 ? "下午" : "上午";             
            var innerModel = (ScheduleModel as QDKouQiangYY.Component.Register.Models.ScheduleModel);
            var k = ScheduleModel.Res排班信息查询.data
                .Where(p => (innerModel.IsDoctor ? p.doctCode == innerModel.DoctorCode : p.doctCode.IsNullOrWhiteSpace()))
                .Select(p => new InfoMore
                {
                    Title = p.doctName ?? DepartmentModel.所选科室?.deptName,
                    SubTitle = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.medAmPm.SafeToAmPm()}",
                    Type = "挂号费",
                    Amount = isFree ? 0:decimal.Parse(p.regAmount),
                    Extends = $"剩余号源 {p.restnum}",
                    ConfirmCommand = confirmCommand,
                    Tag = p,
                    IsEnabled = GetDisable(p, curAmpm),
                    DisableText = GetDisableText(p, curAmpm)
                });

            Data = new ObservableCollection<InfoMore>(k);

            
            
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

            PaymentModel.Self = isFree ? 0 : decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = isFree?0:decimal.Parse(schedulInfo.regAmount);
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

            if (ChoiceModel.Business == Business.挂号)
                Next();
            else QuerySource(i);
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var reMain = 0m;

            var sb = new StringBuilder();
            var docProfe = schedule.doctCode.IsNullOrWhiteSpace() ? "" : DocExtendDic.DocDictionary.ContainsKey(schedule.doctCode)
                ? DocExtendDic.DocDictionary[schedule.doctCode].doctProfe
                : "";
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            //sb.Append($"科室名称：{department.parentDeptName}\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName} {docProfe}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register?.address}\n");
            sb.Append($"挂号序号：{register?.appoNo}\n");
            sb.Append($"交易金额：{PaymentModel.Total.In元()}\n");
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(PaymentModel.Total);
                    sb.Append($"缴费方式：预缴金\n");
                    sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                    sb.Append($"交易流水号：{register.transNo}\n");
                    sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                    break;
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    sb.Append($"缴费方式：银行卡\n");
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        sb.Append($"{posinfo?.Receipt}\n");
                    }
                    else
                    {
                        sb.Append($"银行卡号：{posinfo.CardNo}\n");
                        sb.Append($"授权号：{posinfo.Auth}\n");
                        sb.Append($"终端编号：{posinfo.TId}\n");
                        sb.Append($"终端流水：{posinfo.Trace}\n");
                        sb.Append($"批次号：{posinfo.Batch}\n");
                        sb.Append($"检索参考号：{posinfo.Ref}\n");
                        sb.Append($"交易日期：{posinfo.TransDate}\n");
                        sb.Append($"交易时间：{posinfo.TransTime}\n");
                    }
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
            var docProfe = schedule.doctCode.IsNullOrWhiteSpace()?"":DocExtendDic.DocDictionary.ContainsKey(schedule.doctCode)
                ? DocExtendDic.DocDictionary[schedule.doctCode].doctProfe
                : "";
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：预约挂号\n");

            if (RegisterExtendModel.isPayNow == "1")//已支付
            {
                sb.Append($"支付类型：预约时支付\n");
                switch (PaymentModel.PayMethod)
                {
                    case PayMethod.预缴金:
                        reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(schedule.regAmount ?? "0");
                        sb.Append($"缴费方式：预缴金\n");
                        sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                        sb.Append($"交易流水号：{register.transNo}\n");
                        sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                sb.Append($"支付类型：取号时支付\n");
            }

            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            //sb.Append($"取号密码：{register.orderNo}\n");
            //sb.Append($"科室名称：{department.parentDeptName}\n");
            sb.Append($"预约科室：{department.deptName}\n");
            sb.Append($"预约医生：{schedule.doctName} {docProfe}\n");
            sb.Append($"预约场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊地址：{register.address}\n");

            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于就诊当天提前半小时取号\n");
            sb.Append($"如需取消，请于就诊时间2小时前取消预约。\n");

            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
