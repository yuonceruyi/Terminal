using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YuanTu.ChongQingArea.Component.Auth.Models;
using YuanTu.ChongQingArea.Component.Auth.ViewModels;
using YuanTu.ChongQingArea.Component.Auth.Views;
using YuanTu.ChongQingArea.SiHandler;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Models.Register;
using YuanTu.ChongQingArea.Models.Payment;

namespace YuanTu.ChongQingArea.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel : Default.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        [Dependency]
        public  IPaymentModels PaymentModels { get; set; }

        protected override void FillRechargeRequest(req预约取号 req)
        {
            base.FillRechargeRequest(req);
            req.tradeModeList = new[]
            {
                new 支付信息
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
                    outTradeNo = req.outTradeNo
                }
            }.ToJsonString();
            req.tradeMode = "MIX";
        }

        protected override void ConfirmAction()
        {
            var record = RecordModel.所选记录;
            ChangeNavigationContent(record.doctName);

            //PaymentModel.Date = record.medDate;
            //PaymentModel.Time = record.medAmPm.SafeToAmPm();
            //PaymentModel.Department = record.deptName;
            //PaymentModel.Doctor = record.doctName;

            PaymentModel.Self = decimal.Parse(record.regAmount);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModels.CivilServant = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(record.regAmount);
            PaymentModel.NoPay = PaymentModel.Self == 0;
            PaymentModel.ConfirmAction = Confirm;
            SiModel.ConfirmSi = SiPrePay;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("日期：",record.medDate),
                new PayInfoItem("时间：",record.medAmPm.SafeToAmPm()),
                new PayInfoItem("科室：",record.deptName),
                new PayInfoItem("医生：",record.doctName),
            };

            if (PaymentModels.CivilServant == 0 )
            {
                PaymentModel.RightList = new List<PayInfoItem>()
                {
                    new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                    new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                };
            }
            else {

                PaymentModel.RightList = new List<PayInfoItem>()
                {
                    new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                    new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                    new PayInfoItem("公务员补：",PaymentModels.CivilServant.In元()),
                    new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
                };

            }
           
            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                Logger.Main.Info("开始取号Confirm");

                lp.ChangeText("正在进行取号，请稍候...");

                var patientInfo = PatientModel.当前病人信息;
                var record = RecordModel.所选记录;

                //是否使用社保影响卡类型
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

                Logger.Main.Info($"开始准备预约取号信息 实际卡类型|卡号={t_cardType}|{t_cardNo}");

                TakeNumModel.Req预约取号 = new req预约取号
                {
                    patientId = patientInfo.patientId,
                    cardType = ((int)CardModel.CardType).ToString(),
                    cardNo = CardModel.CardNo,

                    appoNo = RecordModel.所选记录.appoNo,
                    //searchType = ((int)regMode.预约).ToString(),
                    orderNo = RecordModel.所选记录.orderNo,
                    operId = FrameworkConst.OperatorId,
                    tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                    accountNo = patientInfo.patientId,
                    cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                    medDate = record.medDate,
                    scheduleId = record.scheduleId,
                    medAmPm = record.medAmPm
#pragma warning restore 612
                };

                Logger.Main.Info("准备预约取号信息完成");

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
                        ShowAlert(false, "医保结算", "医保结算失败:" + result.Message);
                        if (context.InputPWD == -1 || (!result2.IsSuccess && result2.Message == "用户取消操作"))
                        {//输入中返回，表示取消
                            Logger.Main.Info("取消密码输入，返回上一界面");
                            ShowMask(false);
                            context.InputPWD = 0;
                            try
                            {
                                var sm = GetInstance<IShellViewModel>();
                                Logger.Main.Info($"阻断状态={sm.Busy.IsBusy.ToString()}|{sm.Busy.BusyContent}|{sm.Busy.BusyMutiContent}");
                            }
                            catch { }

                            StackNavigate(A.QuHao_Context, B.QH.SelectSi);
                            return Result.Fail("用户取消交易");
                        }
                        else
                        {
                            context.InputPWD = 0;
                            return Result.Fail("医保支付失败");
                        }
                    }
                    context.InputPWD = 0;

                    TakeNumModel.Req预约取号.ybInfo = result.Value.ToJsonString();
                }


                FillRechargeRequest(TakeNumModel.Req预约取号);
                Logger.Main.Info("准备支付信息完成");

                TakeNumModel.Res预约取号 = DataHandlerEx.预约取号(TakeNumModel.Req预约取号);
                if (TakeNumModel.Res预约取号?.success ?? false)
                {
                    ExtraPaymentModel.Complete = true;

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
                        TypeMsg = "取号成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now.ToString("HH:mm")}分取号",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = TakeNumPrintables(),
                        TipImage = "提示_凭条"
                    });

                    Navigate(A.QH.Print);
                    return Result.Success();
                }
                else
                {
                    if (SiModel.UseSi)
                    {
                        var result2 = context.冲正交易();
                        Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                    }

                    //第三方支付失败时去支付流程里面处理，不在业务里面处理
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "取号失败",
                            DebugInfo = TakeNumModel.Res预约取号?.msg
                        });
                        Navigate(A.QH.Print);
                    }

                    ExtraPaymentModel.Complete = true;

                    return Result.Fail(TakeNumModel.Res预约取号?.code ?? -100, TakeNumModel.Res预约取号?.msg);
                }
            }).Result;
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

            //var itemList = TakeNumModel.List
            //    .Select(i => (ISiItem)i.Title.ToJsonObject<ITEM>())
            //    .ToList();

            Logger.Main.Info("开始基础赋值(获取extend内容):" + RecordModel.所选记录.extend);

            var itemList = RecordModel.所选记录.extend
                 .ToJsonObject<List<ITEM>>()
                .Select(i =>
                {
                    i.RECEIPTNO = 处方号;
                    i.RECEIPTTIME = 开方日期;
                    i.MARKDESC = RecordModel.所选记录.doctName;
                    return (ISiItem)i;
                })
                .ToList();
            
            var date = itemList.Select(i => Convert.ToDateTime(i.RECEIPTTIME)).Min();

            Logger.Main.Info("基础信息赋值完成");

            var context = new SiContext
            {
                经办人 = FrameworkConst.OperatorId,
                住院号_门诊号 = $"0{prefix}{SiModel.社保卡卡号}",
                社保卡卡号 = SiModel.社保卡卡号,
                入院日期 = $"{date:yyyy-MM-dd}",
                职工医疗类别 = "11",
                居民医疗类别 = "12",

                科室编码 = RecordModel.所选记录.deptCode,
                医生编码 = RecordModel.所选记录.doctCode,

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
                Logger.Main.Info("医保冲正交易:" + (!result2.IsSuccess ? $"失败:{result2.Message}" : "成功"));
                ShowAlert(false, "医保结算", $"医保结算失败:{result.Message}");
                return result;
            }

            var res = context.res预结算;

            PaymentModel.Self = decimal.Parse(res.现金支付) * 100m;
            PaymentModel.Insurance = res.SumNonSelf() * 100m;
            PaymentModels.FundPayment = decimal.Parse(res.统筹支付) * 100m;
            PaymentModels.AccountPay = decimal.Parse(res.帐户支付) * 100m;
            PaymentModels.CivilServant= decimal.Parse(res.公务员补助) * 100m;

            return result;
        }

        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;

            var sb = new StringBuilder();
            sb.Append($"状态：取号成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            if (string.IsNullOrEmpty(patientInfo.extend))
            {
                sb.Append($"门诊号：{patientInfo.patientId}\n");
            }
            else
            {
                sb.Append($"门诊号：{patientInfo.extend.Split('|')[0]}\n");
            }
            sb.Append($"交易类型：预约取号\n");
            sb.Append($"挂号费：{record.regFee.In元()}\n");
            sb.Append($"诊疗费：{record.treatFee.In元()}\n");
            sb.Append($"挂号金额：{record.regAmount.In元()}\n");
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
            sb.Append($"单据号：{takeNum.receiptNo}\n");

            sb.Append($"科室名称：{record.deptName}\n");

            sb.Append($"就诊医生：{record.doctName}\n");

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
                if (context.res结算.公务员补助 != "0" )
                {
                    sb.Append($"公务员补助：    {context.res结算.公务员补助}元\n");
                }
            }

            sb.Append($"就诊场次：{record.medAmPm.SafeToAmPm()} {record.medTime}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"挂号序号：{takeNum?.appoNo}\n");
            //sb.Append($"个人支付：{Convert.ToDouble(quhao.selfFee).In元()}\n");
            //sb.Append($"医保支付：{Convert.ToDouble(quhao.insurFee).In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"此号当日有效，如需退号请在三日内办理\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            var image = new System.Drawing.Bitmap(1, 1);
            queue.Enqueue(new PrintItemImage
            {
                Align = ImageAlign.Left,
                Image = image,
                Height = image.Height / 1.5f,
                Width = image.Width / 1.5f
            });
            return queue;
        }

        #region 

        [Dependency]
        public ISiModel SiModel { get; set; }

        #endregion
    }
}