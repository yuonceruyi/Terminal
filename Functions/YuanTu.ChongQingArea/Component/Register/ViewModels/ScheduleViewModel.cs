using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.ChongQingArea.Component.Auth.ViewModels;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;

namespace YuanTu.ChongQingArea.Component.Register.ViewModels
{
    class ScheduleViewModel : Default.Component.Register.ViewModels.ScheduleViewModel
    {
        [Dependency]
        public Models.Payment.IPaymentModels PaymentModels { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            var confirmCommand = new DelegateCommand<Info>(Confirm);

            var list = ScheduleModel.Res排班信息查询.data
            .Where(t => /*(t.medAmPm == "0") || (t.medAmPm == "3") ||*/
             ChoiceModel.Business == Business.挂号 ? (t.medAmPm == "1" && DateTimeCore.Now.Hour < 12 && t.doctName!="高明华") || (t.medAmPm == "2" && DateTimeCore.Now.Hour >= 12 && t.doctName != "高明华") || (t.medAmPm == "3" && t.doctName != "高明华")  : (t.medAmPm == "1" && t.doctName != "高明华") || (t.medAmPm == "2" && t.doctName != "高明华") || (t.medAmPm == "3" && t.doctName != "高明华")
            ).Select(p => new InfoMore
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
            })

            .OrderBy(t => t.Title).OrderBy(t => t.SubTitle);
            Data = new ObservableCollection<InfoMore>(list);

            PlaySound(ChoiceModel.Business == Business.挂号 ? SoundMapping.选择挂号医生 : SoundMapping.选择预约医生);

        }

        [Dependency]
        public ISiModel SiModel { get; set; }
        protected override void Confirm(Info i)
        {
            ScheduleModel.所选排班 = i.Tag.As<排班信息>();
            ChangeNavigationContent(i.Title);

            var schedulInfo = ScheduleModel.所选排班;

            //金额若是医保挂号会被改动
            PaymentModel.Self = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(schedulInfo.regAmount);
            PaymentModel.ConfirmAction = Confirm;

            if (ChoiceModel.Business == Business.挂号)
            {
                DoCommand(lp =>
                {
                    //if (CardModel.CardType == CardType.社保卡)
                    //{
                    //    var result = SiPrePay();

                    //    if (!result.IsSuccess)
                    //    {
                    //        ShowAlert(false, "医保预结算", "医保预结算失败:" + result.Message);
                    //        return;
                    //    }
                    //}
                    GatherConfirmInfo();

                    // 1103 挂号选号源
                    if (FrameworkConst.HospitalId == "1103")
                        QuerySource(i, lp);

                        lp.ChangeText("正在进行锁号,请稍后......");
                        var scheduleInfo = ScheduleModel.所选排班;
                        var lockreq = new req挂号锁号
                        {
                            operId = FrameworkConst.OperatorId,
                            medDate = scheduleInfo.medDate,

                            scheduleId = scheduleInfo.scheduleId,
                        };
                        RegisterModel.Res挂号锁号 = DataHandlerEx.挂号锁号(lockreq);
                        if (!RegisterModel.Res挂号锁号.success)
                        {
                            ShowAlert(false, $"{ChoiceModel.Business}锁号", $"{ChoiceModel.Business}锁号失败\r\n原因：{RegisterModel.Res挂号锁号.msg}", debugInfo: $"{RegisterModel.Res挂号锁号.msg}");
                            return;
                        }
                    else
                    {
                        SiModel.ConfirmSi = SiPrePay;
                        Next();
                    }
                });
            }
            else
            {
                GatherConfirmInfo();
                // 1101 预约不选号源
                if (FrameworkConst.HospitalId == "1101")
                {
                    QuerySource(i);
                    //StackNavigate(A.YuYue_Context, A.YY.Confirm);
                }
                else
                {
                    QuerySource(i);

                }
            }
        }

        protected Result SiPrePay()
        {

            Logger.Main.Info("开始SiPrePay");
            if (SiModel.SiContext != null)
            {
                Logger.Main.Info("医保存在之前的SiContext");
                if (SiModel.SiContext.待冲正)
                {
                    var result2 = SiModel.SiContext.冲正交易();
                    Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                }
            }
            var now = DateTimeCore.Now;
            var s = Convert.ToUInt32((now - new DateTime(2017, 1, 1)).TotalSeconds);
            var prefix = BitConverter.ToString(BitConverter.GetBytes(s)).Replace("-", "");
            var 处方号 = $"1{prefix}{CardModel.CardNo}"; //HIS生成字母开头
            var 开方日期 = $"{now:yyyy-MM-dd HH:mm:ss}";

            var itemList = ScheduleModel.所选排班.extend
                .ToJsonObject<List<获取挂号号源Extend>>()
                .Select(i =>
                {
                    i.RECEIPTNO = 处方号;
                    i.RECEIPTTIME = 开方日期;
                    if (i.MARKDESC.IsNullOrWhiteSpace() && i.DOCTOR.IsNullOrWhiteSpace())//没有医生姓名
                    {
                        if (ScheduleModel.所选排班.doctName.IsNullOrWhiteSpace())
                        {
                            switch (RegTypesModel.SelectRegType.RegType)
                            {
                                case RegType.普通门诊:
                                    i.MARKDESC = "普通";
                                    break;
                                case RegType.急诊门诊:
                                    i.MARKDESC = "急诊";
                                    break;
                            }
                        }
                        else
                        {
                            i.MARKDESC = ScheduleModel.所选排班.doctName;
                        }

                    }
                    return (ISiItem)i;
                })
                .ToList();

            Logger.Main.Info("基础信息赋值完成");
            var context = new SiContext()
            {
                经办人 = FrameworkConst.OperatorId,
                住院号_门诊号 = $"0{prefix}{SiModel.社保卡卡号}",
                社保卡卡号 = SiModel.社保卡卡号,
                入院日期 = $"{now:yyyy-MM-dd}",
                职工医疗类别 = "11",
                居民医疗类别 = "12",

                科室编码 = DepartmentModel.所选科室.deptCode,
                医生编码 = ScheduleModel.所选排班.doctCode,

                itemList = itemList,
                本次结算总金额 = $"{itemList.Sum(i => Convert.ToDecimal(i.NUM) * Convert.ToDecimal(i.PRICE))}",
                本次结算明细总条数 = $"{itemList.Count}",

            };
            SiModel.SiContext = context;
            Logger.Main.Info("医保信息赋值完成");
            var result = context.RunFirstHalf();
            Logger.Main.Info("医保预结算处理:" + (!result.IsSuccess ? $"失败:{result.Message}" : "成功"));

            if (!result.IsSuccess)
            {
                var result2 = context.冲正交易();
                UnlockSource();
                Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                return result;
            }

            var res = context.res预结算;

            PaymentModel.Self = decimal.Parse(res.现金支付) * 100m;
            PaymentModel.Insurance = res.SumNonSelf() * 100m;
            PaymentModels.FundPayment = decimal.Parse(res.统筹支付) * 100m;
            PaymentModels.AccountPay = decimal.Parse(res.帐户支付) * 100m;
            PaymentModels.CivilServant = decimal.Parse(res.公务员补助) * 100m;

            return result;
        }

        private void GatherConfirmInfo()
        {
            PaymentModel.NoPay = ChoiceModel.Business == Business.预约; //默认预约或者自费金额为0时不支付  || PaymentModel.Self == 0

            var schedulInfo = ScheduleModel.所选排班;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {

                new PayInfoItem("日期：",schedulInfo.medDate),
                new PayInfoItem("时间：",schedulInfo.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",schedulInfo.deptName?? DepartmentModel.所选科室?.deptName),
                new PayInfoItem("医生：",schedulInfo.doctName),
            };

            if (ChoiceModel.Business == Business.挂号) //预约时不支付
            {
                PaymentModel.RightList = new List<PayInfoItem>()
               {
                 new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                 //new PayInfoItem("医保支付：",PaymentModel.Insurance.In元()),
                 //new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                };
            }
        }

        protected override Result Confirm()
        {
            return DoCommand(Confirm).Result;
        }

        void OnSiPassword(bool firstTime)
        {
            Logger.Main.Info("开始OnSiPassword");
            var vm = GetInstance<IShellViewModel>();
            var mre = new ManualResetEvent(false);
            Logger.Main.Info("开始输入");
            Invoke(DispatcherPriority.Normal, () =>
            {
                vm.Busy.IsBusy = false;
                StopTimer();
                var viewModel = new SiPasswordViewModel()
                {
                    SiContext = SiModel.SiContext,
                    ManualResetEvent = mre,
                    ErrorMessage = firstTime ? "" : "密码错误 请重新输入",
                };
                ShowMask(true, new SiPasswordView()
                {
                    DataContext = viewModel
                });
            });
            Logger.Main.Info("输入结束");
            mre.WaitOne();
            vm.Busy.IsBusy = true;
            ShowMask(false);
        }

        private Result Confirm(LoadingProcesser lp)
        {
            Logger.Main.Info("开始挂号Confirm");
            lp.ChangeText(ChoiceModel.Business == Business.挂号 ? "正在进行现场挂号，请稍候..." : "正在进行预约挂号，请稍候...");

            Logger.Main.Info("获取患者信息");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            Logger.Main.Info("获取排班信息");
            var scheduleInfo = ScheduleModel.所选排班;
            Logger.Main.Info("获取科室信息");
            var deptInfo = DepartmentModel.所选科室;
            if (ScheduleModel.所选排班.medAmPm == "1" && DateTimeCore.Now.Hour > 11 && ChoiceModel.Business == Business.挂号)
            {
                Logger.Main.Info("12点以后不允许挂上午号" + ScheduleModel.所选排班.medAmPm + "|" + DateTimeCore.Now.Hour.ToString());
                return Result.Fail("12点以后不允许挂上午号");
            }
            if (ScheduleModel.所选排班.medAmPm == "2" && DateTimeCore.Now.Hour <= 11 && ChoiceModel.Business == Business.挂号)
            {
                Logger.Main.Info("12点以前不允许挂下午号" + ScheduleModel.所选排班.medAmPm + "|" + DateTimeCore.Now.Hour.ToString());
                return Result.Fail("12点以前不允许挂下午号");
            }
            Logger.Main.Info("卡类型转换");

            //是否使用社保影响卡类型
            //ByWCL20170724
            string t_cardType = "0";
            string t_cardNo = CardModel.CardNo;
            if (SiModel.UseSi)//使用社保，一律社保卡
            {
                t_cardType = ((int)CardType.社保卡).ToString();
                t_cardNo = SiModel.社保卡卡号;
            }
            else if (CardModel.CardType == CardType.社保卡)//社保卡不使用社保，身份证号做标识
            {
                t_cardType = ((int)CardType.身份证).ToString();
                t_cardNo = patientInfo.idNo;
            }
            else//其他使用原始卡类型
            {
                t_cardType = ((int)CardModel.CardType).ToString();
            }

            Logger.Main.Info($"开始准备挂号信息 实际卡类型|卡号={t_cardType}|{t_cardNo}");
            var req = new req预约挂号
            {
                patientId = patientInfo.patientId,
                cardType = t_cardType,
                cardNo = t_cardNo,
                idNo = patientInfo.idNo,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                accountNo = patientInfo.patientId,
                phone = patientInfo.phone,
                cash = PaymentModel.Self.ToString("0"), // 这里用Self

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

                medTime = SourceModel.所选号源?.extend,

                lockId = RegisterModel.Res挂号锁号?.data?.lockId
            };

            Logger.Main.Info("准备挂号信息完成");
            var context = SiModel.SiContext;
            if (SiModel.UseSi && ChoiceModel.Business == Business.挂号)
            {
                context.OnSiPassword = OnSiPassword;
                var result = context.RunSecondHalf();
                Logger.Main.Info("医保结算处理:" + (!result.IsSuccess ? $"失败:{result.Message}" : "成功"));

                if (!result.IsSuccess)
                {
                    var result2 = context.冲正交易();

                    UnlockSource();

                    Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                    ShowAlert(false, "医保结算", "医保结算失败:" + result.Message);

                    Logger.Main.Info($"即将判断:{context.InputPWD.ToString()}==1 || !{result2.IsSuccess.ToString()} && {result2.Message}== 用户取消操作");
                    if (context.InputPWD == -1 || (!result2.IsSuccess && result2.Message == "用户取消操作"))
                    {//输入中返回，表示取消
                        Logger.Main.Info("取消密码输入，返回上一界面 ");
                        ShowMask(false);
                        context.InputPWD = 0;
                        try
                        {
                            var sm = GetInstance<IShellViewModel>();
                            Logger.Main.Info($"阻断状态={sm.Busy.IsBusy.ToString()}|{sm.Busy.BusyContent}|{sm.Busy.BusyMutiContent}");
                        }
                        catch { }
                        StackNavigate(A.XianChang_Context, B.XC.SelectSi);
                        return Result.Fail("用户取消交易");
                    }
                    else
                    {
                        context.InputPWD = 0;
                        return Result.Fail("医保支付失败");
                    }
                }
                context.InputPWD = 0;
                req.ybInfo = result.Value.ToJsonString();
            }

            FillRechargeRequest(req);
            Logger.Main.Info("准备支付信息完成");

            RegisterModel.Req预约挂号 = req;
            RegisterModel.Res预约挂号 = DataHandlerEx.预约挂号(RegisterModel.Req预约挂号);
            Logger.Main.Info("挂号接口完成");
            if (RegisterModel.Res预约挂号?.success ?? false)
            {
                Logger.Main.Info("挂号接口成功");
                ExtraPaymentModel.Complete = true;

                if (ChoiceModel.Business == Business.挂号)
                {
                    //支付完成
                    if (SiModel.UseSi)
                        context.待冲正 = false;

                    //预缴金，则修改余额

                    //
                    if (ExtraPaymentModel.CurrentPayMethod == PayMethod.预缴金)
                    {
                        var accLeft = decimal.Parse(patientInfo.accBalance) - PaymentModel.Self;
                        ChangeNavigationContent(A.CK.Info, A.ChaKa_Context, $"{patientInfo.name}\r\n余额{accLeft.In元()}");
                    }

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "挂号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分挂号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = RegisterPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.XC.Print);
                }
                else
                {

                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "预约成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分预约",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = AppointPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.YY.Print);
                }
                return Result.Success();
            }

            Logger.Main.Info("挂号接口失败");

            if (SiModel.UseSi && ChoiceModel.Business == Business.挂号)//CardModel.CardType == CardType.社保卡
            {
                var result2 = context.冲正交易();
                UnlockSource();
                Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
            }


            //第三方支付失败时去支付流程里面处理，不在业务里面处理
            if (NavigationEngine.State != A.Third.PosUnion)
            {
                if (ChoiceModel.Business == Business.挂号)
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "挂号失败",
                        DebugInfo = RegisterModel.Res预约挂号?.msg,
                    });
                    Navigate(A.XC.Print);
                }
                else
                {
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "预约失败",
                        DebugInfo = RegisterModel.Res预约挂号?.msg
                    });
                    Navigate(A.YY.Print);
                }
            }

            ExtraPaymentModel.Complete = true;
            return Result.Fail(RegisterModel.Res预约挂号?.code ?? -100, RegisterModel.Res预约挂号?.msg);
        }

        /// <summary>
        /// 号源解锁
        /// </summary>
        private void UnlockSource()
        {
            var scheduleInfo = ScheduleModel.所选排班;
            DoCommand(lp =>
            {
                lp.ChangeText("正在解除锁号记录,请稍后......");
                var lock01 = new req挂号解锁
                {
                    operId = FrameworkConst.OperatorId,
                    medDate = scheduleInfo?.medDate,
                    scheduleId = scheduleInfo?.scheduleId,
                    lockId = RegisterModel.Res挂号锁号?.data?.lockId,
                };
                //不处理返回值
                RegisterModel.Res挂号解锁 = DataHandlerEx.挂号解锁(lock01);
                Logger.Main.Info("挂号解锁:" + (!RegisterModel.Res挂号解锁.success ? $"失败" : "成功"));

            });
        }

        /// <summary>
        ///根据患者信息获取用户实足年龄 
        /// </summary>
        /// <returns></returns>
        //private int PatientAge(string birthday)
        //{
        //    int age;

        //    string today = DateTime.Now.ToString("yyyy-MM-dd");

        //    int y = Convert.ToInt32(birthday.Split('-')[0]);
        //    int m = Convert.ToInt32(birthday.Split('-')[1]);
        //    int d = Convert.ToInt32(birthday.Split('-')[2]);

        //    int y1 = Convert.ToInt32(today.Split('-')[0]);
        //    int m1 = Convert.ToInt32(today.Split('-')[1]);
        //    int d1 = Convert.ToInt32(today.Split('-')[2]);

        //    age = y1 - y;

        //    if (y1 > y)
        //    {
        //        if (m1 < m || (m1 == m && d1 < d))
        //        {
        //            age = age - 1;
        //        }
        //    }

        //    return age;
        //}


        protected override void FillRechargeRequest(req预约挂号 req)
        {
            base.FillRechargeRequest(req);
            var tradeModeList = new List<支付信息>();

            var item = new 支付信息
            {
                tradeMode = req.tradeMode,
                accountNo = req.accountNo,
                cash = req.cash,
                posTransNo = req.posTransNo,
                bankTransNo = req.bankTransNo,
                bankDate = req.bankDate,
                bankTime = req.bankTime,
                bankSettlementTime = req.bankSettlementTime,
                bankCardNo = req.bankCardNo,
                posIndexNo = req.posIndexNo,
                sellerAccountNo = req.sellerAccountNo,
                transNo = req.transNo,
                payAccountNo = req.payAccountNo,
                outTradeNo = req.outTradeNo,
            };

            if (SiModel.UseSi && ChoiceModel.Business == Business.挂号)
            {
                var context = SiModel.SiContext;
                tradeModeList.Add(new 支付信息()
                {
                    tradeMode = "MIC",
                    cash = PaymentModel.Insurance.ToString("0"),
                    transNo = context.res结算.交易流水号,
                    bankCardNo = context.社保卡卡号,
                    bankDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                    bankTime = DateTimeCore.Now.ToString("HH:mm:ss"),
                });
                if (item.cash != "0")
                    tradeModeList.Add(item);
            }
            else
            {
                tradeModeList.Add(item);
            }

            req.tradeModeList = tradeModeList.ToJsonString();
            req.tradeMode = "MIX";

            if (FrameworkConst.HospitalId == "1101")
                req.extend = ScheduleModel.所选排班.hosRegType;
        }

        protected override void QuerySource(Info i)
        {
            DoCommand(lp => QuerySource(i, lp));
        }

        protected void QuerySource(Info i, LoadingProcesser lp)
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
                scheduleId = ScheduleModel.所选排班.scheduleId
            };
            SourceModel.Res号源明细查询 = DataHandlerEx.号源明细查询(SourceModel.Req号源明细查询);
            if (!SourceModel.Res号源明细查询.success)
            {
                ShowAlert(false, "号源明细查询", "没有获得号源信息", debugInfo: SourceModel.Res号源明细查询.msg);
                return;
            }
            if (SourceModel.Res号源明细查询.data?.Count > 0)
            {
                ChangeNavigationContent(i.Title);
                Next();
            }
            else
            {
                ShowAlert(false, "号源明细查询", "没有获得号源信息(列表为空)");
            }
        }


        protected override Queue<IPrintable> RegisterPrintables()
        {
#warning 就诊时间问题需要特殊处理！！！
            Logger.Main.Info("开始拼写凭条");
            var queue = PrintManager.NewQueue("挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            Logger.Main.Info("开始拼写内容");
            try
            {

                var sb = new StringBuilder();
                sb.Append($"状态：挂号成功\n");
                sb.Append($"姓名：{patientInfo.name}\n");
                if (string.IsNullOrEmpty(patientInfo.extend))
                {
                    sb.Append($"门诊号：{patientInfo.patientId}\n");
                }
                else
                {
                    sb.Append($"门诊号：{patientInfo.extend.Split('|')[0]}\n");
                }
                sb.Append($"交易类型：现场挂号\n");
                //sb.Append($"排班类型：{paiban.doctTech}\n");
                sb.Append($"科室名称：{department.deptName}\n");

                //患者选择便民门诊，就诊医生显示便民门诊
                if (string.IsNullOrWhiteSpace(schedule.doctName))
                {
                    sb.Append($"就诊医生：{schedule.doctTech}\n");
                }
                else
                {
                    sb.Append($"就诊医生：{schedule.doctName}\n");
                }

                sb.Append($"挂号费：{schedule.regfee.In元()}\n");
                sb.Append($"诊疗费：{schedule.treatfee.In元()}\n");
                sb.Append($"挂号金额：{schedule.regAmount.In元()}\n");

                sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
                switch (PaymentModel.PayMethod)
                {
                    case PayMethod.预缴金:
                        sb.Append($"预缴金支付：{PaymentModel.Self.In元()}\n");
                        break;
                    case PayMethod.社保:
                        sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
                        break;
                    case PayMethod.银联:
                        sb.Append($"银行卡支付：{PaymentModel.Self.In元()}\n");
                        break;
                    case PayMethod.支付宝:
                        sb.Append($"支付宝支付：{PaymentModel.Self.In元()}\n");
                        break;
                    case PayMethod.微信支付:
                        sb.Append($"微信支付：{PaymentModel.Self.In元()}\n");
                        break;

                    default:
                        sb.Append($"自费金额：{PaymentModel.Self.In元()}\n");
                        break;
                }

                sb.Append($"单据号：{register.receiptNo}\n");

                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb = new StringBuilder();

                if (PaymentModel.PayMethod == PayMethod.预缴金)
                {
                    decimal bb = 0;
                    if (decimal.TryParse(patientInfo.accBalance, out bb))
                    {
                        var b = bb - PaymentModel.Self;
                        sb.Append($"健康卡余额：{b.In元()}\n");
                    }
                }
                if (SiModel.UseSi)
                {
                    /*{"交易流水号":"050000007047455","统筹支付":"0","帐户支付":"5","公务员补助":"0","现金支付":"0","大额理赔金额":"0","历史起付线公务员返还":"0","帐户余额":"551.83","单病种定点医疗机构垫支":"0","民政救助金额":"0","民政救助门诊余额":"0","耐多药项目支付金额":"0","一般诊疗支付数":"0","神华救助基金支付数":"0","本年统筹支付累计":"0","本年大额支付累计":"0","特病起付线支付累计":"0","耐多药项目累计":"0","本年民政救助住院支付累计":"0","中心结算时间":"2017-07-26 09:12:56","本次起付线支付金额":"0","本次进入医保范围费用":"0","药事服务支付数":"0","医院超标扣款金额":"0","生育基金支付":"0","生育现金支付":"0","工伤基金支付":"0","工伤现金支付":"0","工伤单病种机构垫支":"0","工伤全自费原因":"","执行代码":"1","错误信息":null}*/

                    sb.Append($"\n");
                    var context = SiModel.SiContext;
                    sb.Append($"社保统筹支付：{context.res结算.统筹支付}元\n");
                    sb.Append($"社保帐户支付：{context.res结算.帐户支付}元\n");
                    sb.Append($"社保帐户余额：{context.res结算.帐户余额}元\n");
                    if (context.res结算.公务员补助 != "0")
                    {
                        sb.Append($"公务员补助：    {context.res结算.公务员补助}元\n");
                    }
                }
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb = new StringBuilder();
                sb.Append($"就诊时间：{schedule.medDate}\n");
                sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");
                if (!string.IsNullOrEmpty(register?.address))
                {
                    sb.Append($"就诊地址：{register?.address}\n");
                }

                sb.Append($"挂号序号：{register?.appoNo}\n");
                //sb.Append($"个人支付：{guahao.selfFee.In元()}\n");
                //sb.Append($"医保支付：{Convert.ToDouble(guahao.insurFee).In元()}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
                sb.Append($"如需退号请到人工窗口处理！\n");
                sb.Append($"祝您早日康复！\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                if (CurrentStrategyType() == DeviceType.Clinic)
                {
                    for (int i = 1; i < Startup.PrintSpaceLine; i++)
                    {
                        queue.Enqueue(new PrintItemText { Text = "　　 \r\n" });
                    }
                    queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
                }
                else
                    queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            }
            catch (Exception e)
            { Logger.Main.Error(e.ToString()); }
            Logger.Main.Info("内容拼写完毕");
            return queue;
        }


        protected override Queue<IPrintable> AppointPrintables()
        {
#warning 就诊时间问题需要特殊处理！！！
            var queue = PrintManager.NewQueue("预约挂号单");
            var register = RegisterModel.Res预约挂号.data;
            var source = SourceModel.所选号源;
            var department = DepartmentModel.所选科室;
            var schedule = ScheduleModel.所选排班;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

            var sb = new StringBuilder();
            sb.Append($"状态：预约成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");

            if (string.IsNullOrEmpty(patientInfo.extend))
            {
                sb.Append($"门诊号：{patientInfo.patientId}\n");
            }
            else
            {
                sb.Append($"门诊号：{patientInfo.extend.Split('|')[0]}\n");
            }

            sb.Append($"交易类型：预约挂号\n");
            sb.Append($"科室名称：{department.deptName}\n");
            sb.Append($"就诊医生：{schedule.doctName}\n");
            //sb.Append($"挂号费：{paiban.regfee.In元()}\n");
            //sb.Append($"诊疗费：{paiban.treatfee.In元()}\n");
            //sb.Append($"挂号金额：{paiban.regAmount.In元()}\n");
            sb.Append($"就诊时间：{schedule.medDate} {source.extend.SafeConvertToDate("HH:mm:ss", "HH:mm")}\n");
            sb.Append($"就诊场次：{schedule.medAmPm.SafeToAmPm()}\n");

            if (!string.IsNullOrEmpty(register?.address))
            {
                sb.Append($"就诊地址：{register?.address}\n");
            }

            sb.Append($"挂号序号：{source.appoNo}\n");

            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请于预约时间提前半小时取号就诊 \n");
            sb.Append($"过号请重新预约 \n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                ;
                for (int i = 1; i < Startup.PrintSpaceLine; i++)
                {
                    queue.Enqueue(new PrintItemText { Text = "　　 \r\n" });
                }
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            }
            else
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            return queue;
        }
    }
}
