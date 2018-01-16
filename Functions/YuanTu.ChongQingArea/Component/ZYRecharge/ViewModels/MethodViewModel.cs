using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.ChongQingArea.Component.Auth.ViewModels;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using SerialPrinter = YuanTu.Devices.Printer;
using YuanTu.ChongQingArea.Models.Payment;

namespace YuanTu.ChongQingArea.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        public override void OnSet()
        {
            var list = PayMethodDto.GetInfoPays(
                GetInstance<IConfigurationManager>(),
                ResourceEngine,
                "RechargeZYJS",
                new DelegateCommand<Info>(OnPayButtonClick));

            Data = new ObservableCollection<InfoIcon>(list);
            if (ChoiceModel.Business == Business.出院结算)
            { list.Remove(list.Find(t => t.Title == "现金支付")); }
            PlaySound(SoundMapping.选择支付方式);

            //if (ChoiceModel.Business == Business.出院结算)
            //{ list.Remove(list.Find(t => t.Title == "现金充值")); }
            //PlaySound(SoundMapping.选择充值方式);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
          
        }
        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        [Dependency]
        public ISiModel SiModel { get; set; }
        [Dependency]
        public IPaymentModels PaymentModel { get; set; }

        [Dependency]
        public ICardModel CardModel { get; set; }
        protected override void OnPayButtonClick(Info i)
        {
            if (ChoiceModel.Business == Business.出院结算)
            {
                if (decimal.Parse(PatientModel.住院患者信息.balance) > 0)
                {
                    ShowAlert(false, "结算失败", "您的账户中尚有余额，请到窗口进行结算 ");
                    return;
                }
                var payMethod = (PayMethod)i.Tag;
                IpRechargeModel.RechargeMethod = payMethod;
                ExtraPaymentModel.Complete = false;
                ExtraPaymentModel.CurrentBusiness = Business.住院押金;
                ExtraPaymentModel.CurrentPayMethod = payMethod;
                ExtraPaymentModel.FinishFunc = OnRechargeCallback;
                //准备住院充值所需病人信息
                ExtraPaymentModel.PatientInfo = new PatientInfo
                {
                    Name = PatientModel.住院患者信息.name,
                    PatientId = PatientModel.住院患者信息.patientHosId,
                    IdNo = PatientModel.住院患者信息.idNo,
                    GuardianNo = PatientModel.住院患者信息.guardianNo,
                    CardNo = PatientModel.住院患者信息.patientHosId,
                    Remain = decimal.Parse(PatientModel.住院患者信息.accBalance ?? "0")
                };
                ExtraPaymentModel.TotalMoney =0- decimal.Parse(PatientModel.住院患者信息.balance);
                ChangeNavigationContent(IpRechargeModel.RechargeMethod.ToString());
                switch (payMethod)
                {
                    case PayMethod.未知:
                    case PayMethod.预缴金:
                    case PayMethod.社保:                    
                    case PayMethod.现金:
                        ShowAlert(false, "充值失败", "结算时不允许使用现金 ");                    
                        break;
                    case PayMethod.银联:
                        Navigate(A.Third.PosUnion);
                        break;
                    case PayMethod.支付宝:
                        Navigate(A.Third.ScanQrCode);
                        break;
                    case PayMethod.微信支付:
                        Navigate(A.Third.ScanQrCode);
                        break;
                    case PayMethod.苹果支付:
                        break;

                    default:
                        break;
                }
            }
            else
            {
                base.OnPayButtonClick(i);
            }
        }
        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    if (ChoiceModel.Business == Business.出院结算)
                    {
                        return ZYJS();
                        //return Result.Success();
                    }
                    else
                    {
                        p.ChangeText("正在进行充值，请稍候...");
                        IpRechargeModel.Res住院预缴金充值 = null;
                        var patientInfo = PatientModel.住院患者信息;
                        IpRechargeModel.Req住院预缴金充值 = new req住院预缴金充值
                        {
                            patientId = PatientModel.住院患者信息.patientId,
                            patientHosId = PatientModel.住院患者信息.patientHosId,
                            cardNo = PatientModel.住院患者信息.cardNo,
                            operId = FrameworkConst.OperatorId,
                            cash = ExtraPaymentModel.TotalMoney.ToString(),
                            tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                            accountNo = patientInfo.patientHosId
                        };

                        //填充各种支付方式附加数据
                        FillRechargeRequest(IpRechargeModel.Req住院预缴金充值);

                        IpRechargeModel.Res住院预缴金充值 = DataHandlerEx.住院预缴金充值(IpRechargeModel.Req住院预缴金充值);
                        ExtraPaymentModel.Complete = true;
                        if (IpRechargeModel.Res住院预缴金充值.success)
                        {
                            ExtraPaymentModel.Complete = true;
                            //p.ChangeText("正在更新余额信息，请稍后...");
                            //var old = PatientModel.Res住院患者信息查询;
                            //PatientModel.Res住院患者信息查询 = DataHandlerEx.住院患者信息查询(PatientModel.Req住院患者信息查询);
                            //if (!PatientModel.Res住院患者信息查询.success)
                            //{
                            //    PatientModel.Res住院患者信息查询 = old;
                            //    PatientModel.Res住院患者信息查询.data.balance =
                            //    (decimal.Parse(PatientModel.Res住院患者信息查询.data.balance) +
                            //     ExtraPaymentModel.TotalMoney).ToString("0");

                            //    PatientModel.Res住院患者信息查询.data.accBalance =
                            //    (decimal.Parse(PatientModel.Res住院患者信息查询.data.accBalance) +
                            //     ExtraPaymentModel.TotalMoney).ToString("0");

                            //}
                            if (FrameworkConst.DeviceType == "YT-740")
                            {
                                SerialPrint(true);
                                PrintModel.SetPrintInfo(true, new PrintInfo
                                {
                                    TypeMsg = "缴住院押金成功",
                                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                    TipImage = "提示_凭条_740"
                                });
                            }
                            else if (FrameworkConst.DeviceType == "YT-BG350")
                            {
                                PrintModel.SetPrintInfo(true, new PrintInfo
                                {
                                    TypeMsg = "缴住院押金成功",
                                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                    Printables = IpRechargePrintables(true),
                                    TipImage = "提示_凭条"
                                });
                            }
                            else
                            {
                                PrintModel.SetPrintInfo(true, new PrintInfo
                                {
                                    TypeMsg = "缴住院押金成功",
                                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                    Printables = IpRechargePrintables(true),
                                    TipImage = "提示_凭条"
                                });
                            }
                            patientInfo.accBalance = IpRechargeModel.Res住院预缴金充值.data.cash;
                            ChangeNavigationContent(A.ZY.InPatientInfo, A.ZhuYuan_Context,
                                $"{patientInfo.name}\r\n余额{(decimal.Parse(patientInfo.balance)+ ExtraPaymentModel.TotalMoney).In元()}");
                            Navigate(A.ZYCZ.Print);
                            return Result.Success();
                        }
                        else
                        {
                            //PrintModel.SetPrintInfo(false, "充值失败",
                            //    $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(false),
                            //    IpRechargeModel.Res住院预缴金充值?.msg);
                            if (FrameworkConst.DeviceType == "YT-740")
                            {
                                SerialPrint(false);
                                PrintModel.SetPrintInfo(true, new PrintInfo
                                {
                                    TypeMsg = "缴住院押金失败",
                                    TipMsg = $"您已于{DateTimeCore.Now:HH:mm}分缴押金{ExtraPaymentModel.TotalMoney.In元()}失败",
                                    TipImage = "提示_凭条"
                                });
                            }
                            else
                            {
                                PrintModel.SetPrintInfo(false, new PrintInfo
                                {
                                    TypeMsg = "充值失败",
                                    TipMsg = $"您于{DateTimeCore.Now:HH:mm}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                    Printables = IpRechargePrintables(false),
                                    TipImage = "提示_凭条",
                                    DebugInfo = IpRechargeModel.Res住院预缴金充值?.msg
                                });
                            }
                            Navigate(A.ZYCZ.Print);
                            ExtraPaymentModel.Complete = true;
                            return Result.Fail(IpRechargeModel.Res住院预缴金充值?.code ?? -100, IpRechargeModel.Res住院预缴金充值.msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new ZYRechargeInfo
                    {
                        PatientId = PatientModel?.住院患者信息?.patientHosId,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = IpRechargeModel.Res住院预缴金充值?.success ?? false,
                        ErrorMsg = IpRechargeModel.Res住院预缴金充值?.msg
                    });
                }
            });
        }
        void OnSiPassword(bool firstTime)
        {
            var vm = GetInstance<IShellViewModel>();
            var mre = new ManualResetEvent(false);
            Invoke(DispatcherPriority.Normal, () =>
            {
                vm.Busy.IsBusy = false;
                StopTimer();
                ShowMask(true, new SiPasswordView()
                {
                    DataContext = new SiPasswordViewModel()
                    {
                        SiContext = SiModel.SiContext,
                        ManualResetEvent = mre,
                        ErrorMessage = firstTime ? "" : "密码错误 请重新输入",
                    }
                });
            });
            mre.WaitOne();
            vm.Busy.IsBusy = true;
            ShowMask(false);
        }

        public Result SiPrePay()
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

            var context = new SiContext
            {
                经办人 = FrameworkConst.OperatorId,
                住院号_门诊号 = $"0{prefix}{CardModel.CardNo}",
                本次结算总金额 = PatientModel.住院患者信息.cost,
                本次结算明细总条数 = PatientModel.住院患者信息.hosAccBalanceCount,
            };
            Logger.Main.Info("医保信息赋值完成");
            var result预结算 = context.预结算();
            Logger.Main.Info("医保预结算处理:" + (!result预结算.IsSuccess ? $"失败:{result预结算.Message}" : "成功"));


            if (!result预结算.IsSuccess)
            {
                var result2 = context.冲正交易();
                Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                ShowAlert(false, "医保结算", $"医保结算失败:{result预结算.Message}");
                return result预结算;
            }
            var res = context.res预结算;

            PaymentModel.Self = decimal.Parse(res.现金支付) * 100m;
            PaymentModel.Insurance = res.SumNonSelf() * 100m;
            PaymentModel.CivilServant = decimal.Parse(res.公务员补助) * 100m;
            PaymentModel.FundPayment = decimal.Parse(res.统筹支付) * 100m;
            PaymentModel.AccountPay = decimal.Parse(res.帐户支付) * 100m;

            return result预结算;
        }

        private Result ZYJS()
        {
            //lp.ChangeText("正在进行缴费，请稍候...");

            var patientInfo = PatientModel.住院患者信息;
            //var record = BillRecordModel.所选缴费概要;

            //是否使用社保影响卡类型
            //ByWCL20170724
            string t_cardType = "0";
            string t_cardNo = patientInfo.cardNo;
            if (SiModel.UseSi)//使用社保，一律社保卡
            {
                t_cardType = ((int)CardType.社保卡).ToString();
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
            var req = new req自助出院结算
            {
                patientId = patientInfo.patientId,
                cardType = t_cardType,
                cardNo = t_cardNo,
                patientTypeId = "1",
                operId = FrameworkConst.OperatorId,
                tradeMode = IpRechargeModel.RechargeMethod.GetEnumDescription(),
                accountNo = patientInfo.patientId,
            };
            FillRechargeRequest(req);

            var context = SiModel.SiContext;
            if (SiModel.UseSi)
            {
                context.OnSiPassword = OnSiPassword;
                var result = context.RunSecondHalf();
                Logger.Main.Info("医保结算处理:" + (!result.IsSuccess ? $"失败:{result.Message}" : "成功"));

                if (!result.IsSuccess)
                {
                    var result2 = context.冲正交易();
                    Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                    return Result.Fail("医保支付失败");
                }

                req.preYbinfo = result.Value.ToJsonString();
            }

            var r_cyjs = DataHandlerEx.自助出院结算(req);
            if (r_cyjs.success)
            {
                //支付完成
                if (SiModel.UseSi)
                    context.待冲正 = false;

                ExtraPaymentModel.Complete = true;
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "结算成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分结算",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    PrintablesList = new List<Queue<IPrintable>>(2) {
                         ZYJSPrintables()
                    },
                    //Printables = BillPayPrintables(),
                    TipImage = "提示_凭条"
                });
                Navigate(A.JF.Print);
                return Result.Success();
            }

            if (SiModel.UseSi)
            {
                var result2 = context.冲正交易();
                Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
            }

            //第三方支付失败时去支付流程里面处理，不在业务里面处理
            if (NavigationEngine.State != A.Third.PosUnion)
            {
                //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                PrintModel.SetPrintInfo(false, new PrintInfo
                {
                    TypeMsg = "结算失败",
                    DebugInfo = r_cyjs.msg
                });
                Navigate(A.JF.Print);
            }

            ExtraPaymentModel.Complete = true;
            return Result.Fail(r_cyjs.code, r_cyjs.msg);
        }

        protected void FillRechargeRequest(req自助出院结算 req)
        {
            var patientInfo = PatientModel.住院患者信息;
            var tradeModeList = new List<支付信息>();
            //押金余额
            if (decimal.Parse(patientInfo.accBalance) > 0)
            {
                var item1 = new 支付信息
                {
                    tradeMode = "OC",
                    accountNo = patientInfo.patientId,
                    cash = patientInfo.accBalance,

                };
                tradeModeList.Add(item1);
            }
            //欠款
            var item = new 支付信息
            {
                tradeMode = req.tradeMode,
                accountNo = req.accountNo,
                cash = ExtraPaymentModel.TotalMoney.ToString(),
                bankSettlementTime="",
            };

            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var posinfo = extraPaymentModel.PaymentResult as TransResDto;
                if (posinfo != null)
                {
                    item.bankCardNo = posinfo.CardNo;
                    item.bankTime = posinfo.TransTime;
                    item.bankDate = posinfo.TransDate;
                    item.posTransNo = posinfo.Trace;
                    item.bankTransNo = posinfo.Ref;
                    item.sellerAccountNo = posinfo.MId;
                    item.posIndexNo = posinfo.TId;

                }
            }
            else if (extraPaymentModel.CurrentPayMethod == PayMethod.支付宝 ||
                     extraPaymentModel.CurrentPayMethod == PayMethod.微信支付)
            {
                var thirdpayinfo = extraPaymentModel.PaymentResult as 订单状态;
                if (thirdpayinfo != null)
                {
                    item.payAccountNo = thirdpayinfo.buyerAccount;
                    item.transNo = thirdpayinfo.outPayNo;
                    item.outTradeNo = thirdpayinfo.outTradeNo;
                    //item.tradeTime = thirdpayinfo.paymentTime;
                }
            }

            if (SiModel.UseSi)
            {
                var context = SiModel.SiContext;
                tradeModeList.Add(new 支付信息
                {
                    tradeMode = "MIC",
                    cash = PaymentModel.Insurance.ToString("0"),
                    bankTransNo = context.res结算.交易流水号,
                    bankCardNo = context.社保卡卡号
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
        }
        protected Queue<IPrintable> ZYJSPrintables()
        {
            var queue = PrintManager.NewQueue("住院结算");
            //var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.住院患者信息;
            //var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：结算成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");

            sb.Append($"住院号：{patientInfo.patientId}\n");

            sb.Append($"交易类型：自助结算\n");
            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");
            sb.Append($"支付方式：{PaymentModel.PayMethod}\n");
            //sb.Append($"收据号：{billPay.receiptNo}\n");
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
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"如需退费请到人工窗口处理\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
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
            return queue;
        }

 
    }
}