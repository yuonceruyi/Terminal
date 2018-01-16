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
using System.Drawing;
using YuanTu.Core.Services.PrintService;
using YuanTu.ShenZhenArea.Services;
using YuanTu.ShenZhenArea.Models;
using YuanTu.Core.Log;
using YuanTu.ShenZhenArea.Enums;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel : YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {

        [Dependency]
        public IYBModel YB { get; set; }


        [Dependency]
        public IYBService YB_Service { get; set; }


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
                Title = p.doctName.BackNotNullOrEmpty(DepartmentModel.所选科室?.deptName) + (string.IsNullOrEmpty(p.doctTech) ? "" : $"[{p.doctTech}]"),
                SubTitle = $"{p.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} {p.medAmPm.SafeToAmPm()}",
                Type = "诊查费",
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
            //默认挂号、预约挂号、自费金额为0时不支付 
            switch (ChoiceModel.Business)
            {
                case Business.未定义:
                    break;
                case Business.建档:
                    break;
                case Business.挂号:
                case Business.预约:
                    PaymentModel.NoPay = true;
                    PaymentModel.PayMethod = PayMethod.预缴金;
                    break;
                case Business.取号:
                    break;
                case Business.缴费:
                    break;
                case Business.充值:
                    break;
                case Business.查询:
                    break;
                case Business.住院押金:
                    break;
                case Business.检验结果:
                    break;
                case Business.出院结算:
                    break;
                case Business.健康服务:
                    break;
                case Business.体测查询:
                    break;
                case Business.外院卡注册:
                    break;
                case Business.补打:
                    break;
                case Business.实名认证:
                    break;
                case Business.生物信息录入:
                    break;
                default:
                    break;
            }
            PaymentModel.NoPay = PaymentModel.NoPay || PaymentModel.Self == 0;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",schedulInfo.medDate),
                new PayInfoItem("上下午：",$"{schedulInfo.medAmPm.SafeToAmPm()}"),
                new PayInfoItem("时间段：",$"{SourceModel.所选号源?.medBegtime}到{SourceModel.所选号源?.medEndtime}"),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                //new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                //new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                //new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),

                //new PayInfoItem("个人支付：","0元"),
                //new PayInfoItem("医保报销：","0元"),
                //new PayInfoItem("支付金额：","0元",true),
                new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",schedulInfo.doctName),
                new PayInfoItem("诊查费：",ScheduleModel.所选排班.regAmount.In元()),
            };

            if (ChoiceModel.Business == Business.挂号)
                Next();
            else QuerySource(i);
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
                var scheduleInfo = ScheduleModel.所选排班;
                var deptInfo = DepartmentModel.所选科室;
                var scheduleAppNoInfo = SourceModel.所选号源;
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
                    appoNo = scheduleAppNoInfo?.appoNo,
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
                    phone = patientInfo.phone,
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                #region 去医保挂号，然后得到的门诊流水号传给HIS


                if (ChoiceModel.Business == Business.挂号 && CardModel.CardType == CardType.社保卡 && YB.参保类型 == Cblx.基本医疗保险一档)
                {
                    var 医保门诊挂号结果 = YB_Service.医保门诊挂号();
                    if (医保门诊挂号结果.IsSuccess)
                    {
                        Logger.Main.Info($"{patientInfo.name}挂号(科室：{deptInfo.deptCode}{deptInfo.deptName})医保登记成功！");
                        YB_Service.处理医保挂号信息();
                        RegisterModel.Req预约挂号.extend = YB.HIS挂号所需医保信息;
                    }
                    else
                    {
                        Logger.Main.Info($"{patientInfo.name}挂号(科室：{deptInfo.deptCode}{deptInfo.deptName})医保登记失败！病人挂号后将无法进行医保线上结算。");
                    }
                }
                

                #endregion

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
            }
        }


        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();

            #region 登记号二维码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion

            sb.Clear();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{register.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            //sb.Append($"就诊时间：{register.}\n");   //就诊时间需要处理
            sb.Append($"挂号时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            sb.Append($"您的排队号是{register.visitNo}。前方还有{register.extend}人等待就医，请耐心等候！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 12, FontStyle.Bold) });
            sb.Clear();

            sb.Append($"自助机编号：{FrameworkConst.OperatorId}\n");
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
            
            #region 登记号二维码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion

            //sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            //sb.Append($"交易类型：预约挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            //sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            sb.Append($"就诊时间：{SourceModel.所选号源?.medBegtime}到{SourceModel.所选号源?.medEndtime}\n");
            sb.Append($"订单号：{register.orderNo}\n");
            sb.Append($"取号密码：{register.visitNo}（凭此密码取号）\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 12, FontStyle.Bold) });
            sb.Clear();
            sb.Append($"预约时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            //sb.Append($"订单号：{register.orderNo}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}