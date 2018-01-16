using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.UserCenter.Auth;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserCenter;
using YuanTu.Consts.UserCenter.Entities;
using YuanTu.Core.Extension;
using PayInfoItem = YuanTu.Consts.Models.Payment.PayInfoItem;

namespace YuanTu.QingDao.House.Component.Register.ViewModels
{
    public class ScheduleViewModel : Default.House.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public new IRegisterModel RegisterModel { get; set; }

        [Dependency]
        public IAuthModel AuthModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            var confirmCommand = new DelegateCommand<Info>(Confirm);
            var scheduleTypeVoList = RegisterModel.Res按医生排班列表.data.scheduleTypeVOList;
            var list = new List<InfoMore>();
            foreach (var schelist in scheduleTypeVoList)
                foreach (var docInfo in schelist.doctorVOList)
                {
                    if (docInfo.medAmNum >= 0)
                    {
                        var doct = docInfo;

                        doct.medAmPm = 1;
                        list.Add(new InfoMore
                        {
                            Title = doct.name.BackNotNullOrEmpty(doct.deptName),
                            SubTitle = $"{doct.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} 上午",
                            Type = doct.doctTech.BackNotNullOrEmpty(doct.regTypeName),
                            Amount = decimal.Parse(doct.regAmount),
                            Extends = $"剩余号源 {doct.medAmNum}",
                            ConfirmCommand = confirmCommand,
                            Tag = doct,
                            IsEnabled = doct.medAmNum != 0
                        });
                    }

                    if (docInfo.medPmNum >= 0)
                    {
                        var doct = docInfo;

                        doct.medAmPm = 2;
                        list.Add(new InfoMore
                        {
                            Title = doct.name.BackNotNullOrEmpty(doct.deptName),
                            SubTitle = $"{doct.medDate.SafeConvertToDate("yyyy-MM-dd", "MM月dd日")} 下午",
                            Type = doct.doctTech.BackNotNullOrEmpty(doct.regTypeName),
                            Amount = decimal.Parse(doct.regAmount),
                            Extends = $"剩余号源 {doct.medPmNum}",
                            ConfirmCommand = confirmCommand,
                            Tag = doct,
                            IsEnabled = doct.medPmNum != 0
                        });
                    }
                }

            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);
        }

        protected override void Confirm(Info i)
        {
            RegisterModel.当前选择医生排班 = i.Tag.As<DoctorVO>();

            var schedulInfo = RegisterModel.当前选择医生排班;

            DoCommand(lp =>
            {
                if (ChoiceModel.Business == Business.挂号)
                {
                    lp.ChangeText("正在获取挂号应付金额,请稍候...");
                    var result = GetRegAmount();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                }
                else
                {
                    lp.ChangeText("正在查询排班号量,请稍候...");
                    var result = QueryNumSource();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "温馨提示", result.Message);
                        return;
                    }
                }

                PaymentModel.Self = ChoiceModel.Business == Business.挂号
                    ? RegisterModel.Res获取挂号应付金额.data.discountFee
                    : decimal.Parse(schedulInfo.regAmount);
                PaymentModel.Insurance = decimal.Parse("0");
                PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
                PaymentModel.NoPay = ChoiceModel.Business == Business.预约 || PaymentModel.Self == 0; //默认预约或者自费金额为0时不支付
                PaymentModel.ConfirmAction = Confirm;

                PaymentModel.LeftList = new List<PayInfoItem>
                {
                    new PayInfoItem("日期：", schedulInfo.medDate),
                    new PayInfoItem("时间：", schedulInfo.medAmPm.SafeToAmPm()),
                    new PayInfoItem("科室：", schedulInfo.deptName ?? RegisterModel.当前选择科室?.deptName),
                    new PayInfoItem("医生：", schedulInfo.name)
                };

                PaymentModel.RightList = new List<PayInfoItem>
                {
                    new PayInfoItem("应付金额：", PaymentModel.Total.In元()),
                    new PayInfoItem("实付金额：", PaymentModel.Self.In元(), true)
                };

                ChangeNavigationContent(".");
                if (ChoiceModel.Business == Business.挂号)
                    Next();
                else Navigate(A.YY.Time);
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                ChangeNavigationContent(".");
                lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

                var patientInfo = AuthModel.当前就诊人信息;
                var scheduleInfo = RegisterModel.当前选择医生排班;
                var deptInfo = RegisterModel.当前选择科室;
                var hospital = RegisterModel.当前选择医院;
                var numSource = RegisterModel.当前选择号源;

                if (ChoiceModel.Business == Business.挂号)
                {
                    var req = new req确认挂号
                    {
                        corpId = hospital.corpId.ToString(),
                        deptCode = deptInfo.deptCode,
                        medAmPm = scheduleInfo.medAmPm.ToString(),
                        medDate = scheduleInfo.medDate,
                        patientId = patientInfo.patientId.ToString(),
                        regMode = "2",
                        regType = scheduleInfo.regType.ToString(),
                        scheduleId = scheduleInfo.scheduleId
                    };
                    RegisterModel.Res确认挂号 = DataHandlerEx.确认挂号(req);
                    if (RegisterModel.Res确认挂号?.success ?? false)
                    {
                        var reqPay = new req挂号支付
                        {
                            corpId = hospital.corpId.ToString(),
                            feeChannel = "3",
                            optType = "3",
                            outId = RegisterModel.Res确认挂号?.data.id.ToString(),
                            patientId = patientInfo.patientId.ToString()
                        };
                        RegisterModel.Res挂号支付 = DataHandlerEx.挂号支付(reqPay);
                        if (RegisterModel.Res挂号支付?.success ?? false)
                        {
                            ExtraPaymentModel.Complete = true;

                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "挂号成功",
                                TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分挂号",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                PrintablesList = new List<Queue<IPrintable>> { RegisterPrintables() },
                                TipImage = "凭条出口_House"
                            });
                            
                            Navigate(A.XC.Print);
                            return Result.Success();
                        }
                    }
                }
                else
                {
                    var req = new req确认预约
                    {
                        appoNo = numSource.appoNo,
                        corpId = hospital.corpId.ToString(),
                        deptCode = deptInfo.deptCode,
                        doctCode = scheduleInfo.doctCode,
                        feeChannel = "4", //到院支付

                        medAmPm = scheduleInfo.medAmPm.ToString(),
                        medDate = scheduleInfo.medDate,
                        medBegTime = numSource.medBegTime,
                        medEndTime = numSource.medEndTime,
                        optType = "6",
                        patientId = patientInfo.patientId.ToString(),
                        regMode = "1",
                        regType = scheduleInfo.regType.ToString(),
                        scheduleId = scheduleInfo.scheduleId
                    };
                    RegisterModel.Res确认预约 = DataHandlerEx.确认预约(req);
                    if (RegisterModel.Res确认预约?.success ?? false)
                    {
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "预约成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分预约",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            PrintablesList = new List<Queue<IPrintable>> { AppointPrintables() },
                            TipImage = "凭条出口_House"
                        });
                        
                        Navigate(A.YY.Print);
                        return Result.Success();
                    }
                }

                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = ChoiceModel.Business == Business.挂号 ? "挂号失败" : "预约失败",
                        TipMsg = ChoiceModel.Business == Business.挂号
                            ? RegisterModel.Res挂号支付?.msg ?? RegisterModel.Res确认挂号?.msg
                            : RegisterModel.Res确认预约?.msg
                    });
                    Navigate(ChoiceModel.Business == Business.挂号 ? A.XC.Print : A.YY.Print);
                }
                ExtraPaymentModel.Complete = true;
                return
                    Result.Fail(ChoiceModel.Business == Business.挂号
                        ? RegisterModel.Res挂号支付?.msg ?? RegisterModel.Res确认挂号?.msg
                        : RegisterModel.Res确认预约?.msg);
            }).Result;
        }

        protected virtual Result GetRegAmount()
        {
            var req = new req获取挂号应付金额
            {
                corpId = RegisterModel.当前选择医院.corpId.ToString(),
                deptCode = RegisterModel.当前选择科室.deptCode,
                doctCode = RegisterModel.当前选择医生排班.doctCode,
                medAmPm = RegisterModel.当前选择医生排班.medAmPm.ToString(),
                medDate = RegisterModel.当前选择医生排班.medDate,
                patientId = AuthModel.当前就诊人信息.patientId.ToString(),
                regAmount = RegisterModel.当前选择医生排班.regAmount,
                regMode = ChoiceModel.Business == Business.预约 ? "1" : "2",
                regType = RegisterModel.当前选择医生排班.regType.ToString(),
                scheduleId = RegisterModel.当前选择医生排班.scheduleId
            };
            var res = DataHandlerEx.获取挂号应付金额(req);
            if (res?.success ?? false)
            {
                RegisterModel.Res获取挂号应付金额 = res;
                return Result.Success();
            }

            return Result.Fail($"获取挂号应付金额失败:{res?.msg}");
        }

        protected virtual Result QueryNumSource()
        {
            var req = new req查询排班号量
            {
                corpId = RegisterModel.当前选择医院.corpId.ToString(),
                deptCode = RegisterModel.当前选择科室.deptCode,
                doctCode = RegisterModel.当前选择医生排班.doctCode,
                medAmPm = RegisterModel.当前选择医生排班.medAmPm.ToString(),
                medDate = RegisterModel.当前选择医生排班.medDate,
                regMode = ChoiceModel.Business == Business.预约 ? "1" : "2",
                regType = RegisterModel.当前选择医生排班.regType.ToString(),
                scheduleId = RegisterModel.当前选择医生排班.scheduleId
            };
            var res = DataHandlerEx.查询排班号量(req);
            if (res?.success ?? false)
            {
                if (res.data?.sourceList?.Count > 0)
                {
                    RegisterModel.Res查询排班号量 = res;
                    return Result.Success();
                }

                return Result.Fail("没有获得号源信息(列表为空)");
            }
            return Result.Fail($"没有获得号源信息:{res?.msg}");
        }

        protected override Queue<IPrintable> RegisterPrintables()
        {
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res确认挂号.data;

            var sb = new StringBuilder();
            sb.Append($"状态：挂号成功\n");
            sb.Append($"医院：{register.corpName}\n");
            sb.Append($"姓名：{register.patientName}\n");
            sb.Append($"就诊人Id：{register.patientId}\n");
            sb.Append($"交易类型：现场挂号\n");
            sb.Append($"就诊科室：{register.deptName}\n");
            sb.Append($"就诊医生：{register.doctName}\n");
            sb.Append($"挂号金额：{Convert.ToDecimal(register.regAmount).In元()}\n");
            sb.Append($"就诊日期：{RegisterModel.当前选择医生排班?.medDate}\n");
            sb.Append(
                $"就诊时间：{register.medBegTime.MillisecondsToDateTime("HH:mm")}~{register.medEndTime.MillisecondsToDateTime("HH:mm")}\n");
            sb.Append($"就诊场次：{register.medAmPm.SafeToAmPm()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }

        protected override Queue<IPrintable> AppointPrintables()
        {
            var queue = PrintManager.NewQueue("预约挂号单");
            var appoint = RegisterModel.Res确认预约.data;

            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"医院：{appoint.corpName}\n");
            sb.Append($"姓名：{appoint.patientName}\n");
            sb.Append($"就诊人Id：{appoint.patientId}\n");
            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"就诊科室：{appoint.deptName}\n");
            sb.Append($"就诊医生：{appoint.doctName}\n");
            sb.Append($"就诊日期：{RegisterModel.当前选择医生排班?.medDate}\n");
            sb.Append($"就诊场次：{appoint.medAmPm.SafeToAmPm()}\n");
            sb.Append($"就诊时间：{RegisterModel.当前选择号源.medBegTime}~{RegisterModel.当前选择号源.medEndTime}\n");
            sb.Append($"预约序号：{appoint.appoNo}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于当日取号就诊。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}