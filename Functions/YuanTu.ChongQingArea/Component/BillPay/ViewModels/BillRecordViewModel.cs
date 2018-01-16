using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Regions;
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
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Log;
using YuanTu.Core.Services.PrintService;
using YuanTu.ChongQingArea.Models.Payment;

namespace YuanTu.ChongQingArea.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public ISiModel SiModel { get; set; }

        [Dependency]
        public IPaymentModels PaymentModels { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "我要缴费";

            Collection = BillRecordModel.Res获取缴费概要信息.data.Select(p => new PageData
            {
                CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem,
                Tag = p
            }).ToArray();
            BillCount = $"{BillRecordModel.Res获取缴费概要信息.data.Count}张处方单";
            TotalAmount = BillRecordModel.Res获取缴费概要信息.data.Sum(p => decimal.Parse(p.billFee)).In元();
            PlaySound(SoundMapping.选择待缴费处方);
        }
        //private List<缴费明细信息> groupBillList(List<缴费明细信息> detail)
        //{
        //    List<缴费明细信息> r = new List<缴费明细信息>();
        //    foreach (var i in detail)
        //    {
        //        var f = r.Find(t => t.billNo == i.billNo && t.itemSpecs == i.itemSpecs);
        //        if (f != null)
        //        {
        //            f.billFee = (int.Parse(f.billFee) + int.Parse(i.billFee)).ToString();
        //            f.itemPrice = (int.Parse(f.itemPrice) + int.Parse(i.itemPrice)).ToString();
        //        }
        //        else
        //        {
        //            i.itemName = i.itemSpecs;
        //            r.Add(i);
        //        }
        //    }
        //    return r;
        //}

        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

        
            var recordInfo = BillRecordModel.所选缴费概要;

            PaymentModel.Self = decimal.Parse(recordInfo.billFee);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModels.CivilServant = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee);
            PaymentModel.ConfirmAction = Confirm;
            SiModel.ConfirmSi = SiPrePay;
            GatherConfirmInfo();
            Next();
            //if (CardModel.CardType == CardType.社保卡)
            //{
            //    DoCommand(lp =>
            //    {
            //        //var result = SiPrePay();

            //        //if (!result.IsSuccess)
            //        //{
            //        //    ShowAlert(false, "医保预结算", "医保预结算失败:" + result.Message);
            //        //    return;
            //        //}

            //        Logger.Main.Info("身份验证成功");
            //        SiModel.ConfirmSi = SiPrePay;
            //        GatherConfirmInfo();
            //        Next();
            //    });
            //}
            //else
            //{
            //    GatherConfirmInfo();
            //    Next();
            //}
        }

        private void GatherConfirmInfo()
        {
            PaymentModel.NoPay = PaymentModel.Self == 0;

            var recordInfo = BillRecordModel.所选缴费概要;
            var dateTime = recordInfo.billDate?.SafeToSplit(' ', 2);
            PaymentModel.LeftList = new List<PayInfoItem>
            {
                new PayInfoItem("日期：", dateTime?[0] ?? recordInfo.billDate),
                new PayInfoItem("时间：", dateTime?[1] ?? null),
                new PayInfoItem("科室：", recordInfo.deptName),
                new PayInfoItem("医生：", recordInfo.doctName)
            };

            PaymentModel.RightList = new List<PayInfoItem>
            {
                //new PayInfoItem("个人支付：", PaymentModel.Self.In元()),
                //new PayInfoItem("医保支付：", PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
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

            var itemList = BillRecordModel.所选缴费概要.billItem
                .Select(i => (ISiItem)i.extend.ToJsonObject<ITEM>())
                .ToList(); 
            Logger.Main.Info("基础信息赋值完成");
            var date = itemList.Select(i => Convert.ToDateTime(i.RECEIPTTIME)).Min();
            string yllb = "11";
            string jmlb = "12";

            //1：判断是否特病病人
            if (!string.IsNullOrEmpty(BillRecordModel.所选缴费概要.billGroupNo)) { yllb = "13"; jmlb = "13"; }

            //2：判断患者是职工参保 或是居民参保
            if ( SiModel.Res获取人员基本信息.参保类别 == "1" && string.IsNullOrEmpty(BillRecordModel.所选缴费概要.billGroupNo)) { yllb = "11"; jmlb = "11"; }

            if (SiModel.Res获取人员基本信息.参保类别 == "2"  && string.IsNullOrEmpty(BillRecordModel.所选缴费概要.billGroupNo)) { yllb = "12"; jmlb = "12"; }

            var context = new SiContext
            {
                经办人 = FrameworkConst.OperatorId,
                住院号_门诊号 = $"0{prefix}{SiModel.社保卡卡号}",
                社保卡卡号 = SiModel.社保卡卡号,
                入院日期 = $"{date:yyyy-MM-dd}",

                科室编码 = BillRecordModel.所选缴费概要.deptCode,
                医生编码 = BillRecordModel.所选缴费概要.doctCode,

                itemList = itemList,
                本次结算总金额 = $"{itemList.Sum(i => Convert.ToDecimal(i.NUM) * Convert.ToDecimal(i.PRICE))}",
                本次结算明细总条数 = $"{itemList.Count}",
                //特病新增
                职工医疗类别 = yllb,
                居民医疗类别 = jmlb,
                入院诊断 = BillRecordModel.所选缴费概要.billGroupNo,
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

        protected override Result Confirm()
        {
            return DoCommand(Confirm).Result;
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

        private Result Confirm(LoadingProcesser lp)
        {
            lp.ChangeText("正在进行缴费，请稍候...");

            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;

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
            var req = new req缴费结算
            {
                patientId = patientInfo.patientId,
                patientName = patientInfo.name,
                cardType = t_cardType,
                cardNo = t_cardNo,
                operId = FrameworkConst.OperatorId,
                secrityNo = patientInfo.idNo,

                tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                cash = PaymentModel.Self.ToString("0"),
               
                accountNo = patientInfo.patientId,
                billNo = record.billNo,
                allSelf = PaymentModel.Insurance == 0 ? "1" : "0"
            };

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

                req.preYbinfo = result.Value.ToJsonString();
            }
            FillRechargeRequest(req);

            BillPayModel.Req缴费结算 = req;
            BillPayModel.Res缴费结算 = DataHandlerEx.缴费结算(BillPayModel.Req缴费结算);
            if (BillPayModel.Res缴费结算.success)
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
                    patientInfo.accBalance = accLeft.ToString("F2");
                }
                ExtraPaymentModel.Complete = true;
                //PrintModel.SetPrintInfo(true, new PrintInfo
                //{
                //    TypeMsg = "缴费成功",
                //    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                //    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                //    PrintablesList=new List<Queue<IPrintable>>(2) {
                //         BillPayPrintables("患者留存"),
                //          BillPayPrintables("交执行科室")
                //    },
                //    //Printables = BillPayPrintables(),
                //    TipImage = "提示_凭条"
                //});
                //Navigate(A.JF.Print);
                JudgeNav();
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
                    TypeMsg = "缴费失败",
                    DebugInfo = BillPayModel.Res缴费结算.msg
                });
                Navigate(A.JF.Print);
            }

            ExtraPaymentModel.Complete = true;
            return Result.Fail(BillPayModel.Res缴费结算.code, BillPayModel.Res缴费结算.msg);
        }


        private void JudgeNav()
        {
            BillRecordModel.Res获取缴费概要信息.data.Remove(BillRecordModel.所选缴费概要);
            var leftCount = BillRecordModel.Res获取缴费概要信息.data.Count;
            if (leftCount > 0)
            {
                ShowConfirm("继续结算", $"您还有{leftCount}张待结算处方，是否继续结算？", cbk =>
                 {
                     if (cbk)
                     {
                         DoCommand(lp =>
                         {
                             lp.ChangeText("正在打印当前结算处方，请稍后...");
                             var printer = GetInstance<IPrintManager>();
                             printer.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), BillPayPrintables());
                             //printer.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), BillPayPrintables("交执行科室"));
                             Switch(A.JiaoFei_Context, A.JF.BillRecord);
                         });
                     }
                     else
                     {
                         PrintModel.SetPrintInfo(true, new PrintInfo
                         {
                             TypeMsg = "缴费成功",
                             TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                             PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                             //PrintablesList = new List<Queue<IPrintable>>(2) {
                             //   BillPayPrintables("患者留存"),
                             //   BillPayPrintables("交执行科室")
                             //},
                             Printables = BillPayPrintables(),
                             TipImage = "提示_凭条"
                         });
                         Navigate(A.JF.Print);
                     }
                 });
            }
            else
            {
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "缴费成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    //PrintablesList = new List<Queue<IPrintable>>(2) {
                    //    BillPayPrintables("患者留存"),
                    //    BillPayPrintables("交执行科室")
                    //},
                    Printables = BillPayPrintables(),
                    TipImage = "提示_凭条"
                });
                Navigate(A.JF.Print);
            }
        }

        protected override void FillRechargeRequest(req缴费结算 req)
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
                outTradeNo = req.outTradeNo
            };

            if (SiModel.UseSi)
            {
                var context = SiModel.SiContext;
                tradeModeList.Add(new 支付信息
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
        }


        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            queue.Enqueue(new PrintItemText
            {
                Font = new Font("微软雅黑", (CurrentStrategyType() == DeviceType.Clinic ? 8 : 12), FontStyle.Bold),
                StringFormat = PrintConfig.Center,
            });

            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            if (string.IsNullOrEmpty(patientInfo.extend))
            {
                sb.Append($"门诊号：{patientInfo.patientId}\n");
            }
            else
            {
                sb.Append($"门诊号：{patientInfo.extend.Split('|')[0]}\n");
            }
            sb.Append($"交易类型：自助缴费\n");

            sb.Append($"金额总计：{PaymentModel.Total.In元()}\n");

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

            sb.Append($"单据号：{billPay.receiptNo}\n");

            if (PaymentModel.PayMethod == PayMethod.预缴金)
            {
                // 上面修改过余额
                sb.Append($"健康卡余额：{patientInfo.accBalance.In元()}\n");
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

            if (!string.IsNullOrEmpty(billPay.takeMedWin))
            {
                sb.Append($"取药窗口：{billPay.takeMedWin}\n");

            }

            if (!string.IsNullOrEmpty(billPay.testCode))
            {
                sb.Append($"检验条码：{billPay.testCode}\n");
                queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                sb.Clear();
                var image = BarCode128.GetCodeImage(billPay.testCode, BarCode.Code128.Encode.Code128A);
                queue.Enqueue(new PrintItemImage
                {
                    Align = ImageAlign.Left,
                    Image = image,
                    Height = image.Height / 1.5f,
                    Width = image.Width / 1.5f
                });
            }
            sb.Append($"收据费目：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            if (record?.billItem != null)
            {
                var btl = new List<BillType>();
                foreach (var detail in record.billItem)
                {
                    var d = btl.Find(t => t.Name == detail.billType);
                    if (d == null)
                    {
                        btl.Add(new BillType(detail.billType, detail.billFee));
                    }
                    else
                    {
                        d.AddAmt(detail.billFee);
                        d.Num++;
                    }
                }
                queue.Enqueue(new PrintItemRatioText("名称", "数量", "金额"));
                foreach (var b in btl)
                {
                    queue.Enqueue(new PrintItemRatioText(b.Name, b.Num.ToString(), b.AmtS.InRMB()));
                }
                //queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB()));
            }
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"如需发票请到窗口打印\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");

            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                for (int i = 1; i < Startup.PrintSpaceLine; i++)
                    queue.Enqueue(new PrintItemText { Text = "　　 \r\n" });
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            }
            else
            {
                queue.Enqueue(new PrintItemText { Text = ".　　 \r\n" });
            }

            return queue;
        }
        public class BillType
        {
            public BillType(string s_name, string s_amt)
            {
                Name = s_name;
                Num = 1;
                AddAmt(s_amt);
            }
            public string Name = "";
            public int Num = 0;
            public int Amt = 0;
            public void AddAmt(string a)
            {
                int da = 0;
                if (int.TryParse(a, out da))
                {
                    Amt += da;
                }
            }
            public string AmtS { get { return Amt.ToString(); } }
        }
    }
}