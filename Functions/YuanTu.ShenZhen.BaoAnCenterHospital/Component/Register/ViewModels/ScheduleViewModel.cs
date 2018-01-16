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
using YuanTu.ShenZhenArea.Enums;
using YuanTu.ShenZhenArea.Models;
using YuanTu.ShenZhenArea.Services;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.Core.Gateway;
using System.Drawing;

namespace YuanTu.ShenZhen.BaoAnCenterHospital.Component.Register.ViewModels
{
    public class ScheduleViewModel :YuanTu.Default.Component.Register.ViewModels.ScheduleViewModel
    {

        protected static readonly BarCode.Code128 BarCode128 = new BarCode.Code128
        {
            Magnify = 1,
            Height = 80
        };


        [Dependency]
        public IYBModel YB { get; set; }


        [Dependency]
        public IYBService YB_Service { get; set; }


        [Dependency]
        public IAccountingService Account_Service { get; set; }

        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var scheduleInfo = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            //PaymentModel.Date = schedulInfo.medDate;
            //PaymentModel.Time = schedulInfo.medAmPm.SafeToAmPm();
            //PaymentModel.Department = schedulInfo.deptName ?? DeptartmentModel.所选科室?.deptName;
            //PaymentModel.Doctor = schedulInfo.doctName;

            PaymentModel.Self = decimal.Parse(scheduleInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(scheduleInfo.regAmount);
            //默认挂号、预约挂号、自费金额为0时不支付 
            switch (ChoiceModel.Business)
            {
                case Business.未定义:
                    break;
                case Business.建档:
                    break;
                case Business.挂号:
                    PaymentModel.NoPay = false;
                        #region 锁号操作

                        RegisterModel.Req挂号锁号 = new req挂号锁号
                        {
                            cardNo = CardModel.CardNo,
                            cardType = ((int)CardModel.CardType).ToString(),
                            patientId = patientInfo.patientId,
                            regType = ((int)RegType.普通门诊).ToString(),
                            medDate = scheduleInfo.medDate,
                            scheduleId = scheduleInfo.scheduleId,
                            deptCode = scheduleInfo.deptCode,
                            doctCode = scheduleInfo.doctCode,
                            medAmPm = scheduleInfo.medAmPm,
                        };

                        RegisterModel.Res挂号锁号 = DataHandlerEx.挂号锁号(RegisterModel.Req挂号锁号);
                        if (!RegisterModel.Res挂号锁号?.success ?? false)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}锁号", $"{ChoiceModel.Business}锁号失败\r\n原因：{RegisterModel.Res挂号锁号.msg}", debugInfo: $"{RegisterModel.Res挂号锁号.msg}");
                            return;
                        }

                        #endregion
                        var cm = (CardModel as ShenZhenCardModel);
                        if (CardModel.CardType == CardType.社保卡 && cm.RealCardType == CardType.门诊号)   //社保病人，但是通过扫描登记号进来的
                        {
                            ShowAlert(true, "温馨提示", "系统记录到您是社保病人\n但您本次就医未拿社保卡验证\n故本次就医只能自费缴费", 10);
                        }

                    if (CardModel.CardType == CardType.社保卡 && cm.CardType == CardType.社保卡)
                    {
                        var breakYB = false;
                        var 医保门诊挂号结果 = YB_Service.医保门诊挂号();
                        if (!医保门诊挂号结果.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊挂号失败", 医保门诊挂号结果.Message + "\n点击确定进行自费挂号");
                            breakYB = true;
                        }

                        YB_Service.获取挂号信息();


                        YB_Service.处理医保费用();
                        if (!breakYB)   //医保正常计算
                        {
                            PaymentModel.Self = (decimal)(100 * YB.自费);
                            PaymentModel.Insurance = (decimal)(100 * YB.账户支付额);
                            PaymentModel.Total = (decimal)(100 * (YB.账户支付额 + YB.自费));
                        }
                    }
                    break;
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
            if (PaymentModel.NoPay)
                PaymentModel.PayMethod = PayMethod.现金;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",scheduleInfo.medDate),
                new PayInfoItem("时间：",scheduleInfo.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",scheduleInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",scheduleInfo.doctName),
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
                    regType = ((int)RegType.普通门诊).ToString(),
                    medAmPm = scheduleInfo.medAmPm,
                    medDate = scheduleInfo.medDate,
                    deptCode = deptInfo.deptCode,
                    deptName = deptInfo.deptName,
                    scheduleId = scheduleInfo.scheduleId,
                    appoNo = scheduleAppNoInfo?.appoNo,
                    patientName = patientInfo.name,
                    doctCode = scheduleInfo?.doctCode,
                    doctName = scheduleInfo?.doctName,
                    phone = patientInfo.phone,
                };

                FillRechargeRequest(RegisterModel.Req预约挂号);

                #region 医保挂号。。。这里要去扣医保的钱

                if (ChoiceModel.Business == Business.挂号 && PaymentModel.Insurance > 0)
                {
                    YB_Service.处理医保挂号结算信息();
                    RegisterModel.Req预约挂号.extend = YB.HIS结算所需医保信息;
                }
                else
                {
                    RegisterModel.Req预约挂号.extend = "";
                }

                #endregion

                RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
                if (RegisterModel.Res预约挂号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

                    //todo  处理医保记账、消费记账
                    if (PaymentModel.Insurance > 0)
                    {
                        Account_Service.医保消费记账(true);
                    }
                    Account_Service.门诊挂号记账(true);

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
                else
                {
                    if (PaymentModel.Insurance > 0)  //医保退费
                    {
                        if (DataHandler.UnKnowErrorCode.Contains(RegisterModel.Res预约挂号.code))  //单边账。。
                        {
                            RegisterModel.Res预约挂号.msg = $"{RegisterModel.Res预约挂号.code} 服务受理异常,挂号失败!";
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "社保单边账",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分缴费失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = BillErrSheBaoPayPrintables(),
                                TipImage = "提示_凭条"
                            });

                            ShowAlert(false, "挂号结算结果未知", $"{RegisterModel.Res预约挂号.code} 服务受理异常,挂号失败!", 20);
                            //todo 处理医保记账、消费记账
                            Account_Service.门诊结算记账(false, true);
                            Account_Service.医保消费记账(false);
                            Navigate(A.JF.Print);
                            return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
                        }

                        //医保退费
                        var 医保门诊退费结果 = YB_Service.医保门诊退费();
                        if (!医保门诊退费结果.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊退费失败", 医保门诊退费结果.Message, 20);
                        }
                        var 医保门诊支付确认 = YB_Service.医保门诊支付确认(true);
                        if (!医保门诊支付确认.IsSuccess)
                        {
                            ShowAlert(false, "医保门诊退费确认失败", 医保门诊退费结果.Message, 20);
                        }
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
                }

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
            #region 登记号条码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion
            //sb.Append($"状态：挂号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            sb.Append($"就诊地点：{register.address}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 12, FontStyle.Bold) });
            sb.Clear();

            //sb.Append($"就诊地点：{register.address}\n");
            //sb.Append($"挂号费：{schedule.regfee.In元()}\n");
            //sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
            sb.Append($"诊疗费：{PaymentModel.Total.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();

            sb.Append($"您的排队号是{register.visitNo}。\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = new Font("微软雅黑", 12, FontStyle.Bold) });
            sb.Clear();

            sb.Append($"挂号时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
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
            #region 登记号条码
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
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"诊疗科室：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            //sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {register?.medDate?.Split('-')[0]}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊时间：{SourceModel.所选号源?.medBegtime}到{SourceModel.所选号源?.medEndtime}\n");            
            sb.Append($"预约时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText {Text = sb.ToString()});
            return queue;
        }
        protected virtual Queue<IPrintable> BillErrSheBaoPayPrintables()
        {
            var queue = PrintManager.NewQueue("社保单边账");
            var billPay = RegisterModel.Res预约挂号.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = ScheduleModel.所选排班;
            var sb = new StringBuilder();
            //sb.Append($"状态：缴费失败\n");   
            #region 登记号条码
            var image = BarCode128.GetCodeImage(patientInfo.patientId, BarCode.Code128.Encode.Code128A);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Center,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            #endregion
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"登记号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助挂号\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod.GetEnumDescription()}\n");
            sb.Append($"社保单据号：{YB.djh}\n");
            sb.Append($"社保金额：{PaymentModel.Insurance.In元()}\n");
            sb.Append($"失败原因：{RegisterModel.Res预约挂号.msg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请凭该凭证寻找工作人员处理\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }

    }
}