using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.ZheJiangHospital.Component.Auth.Models;
using YuanTu.ZheJiangHospital.Component.BillPay.Models;
using YuanTu.ZheJiangHospital.Component.BillPay.Views;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IAuthModel Auth { get; set; }

        [Dependency]
        public IBillPayModel BillPay { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "缴费";

            Collection = new List<PageData>
            {
                new PageData
                {
                    CatalogContent = $"金额 {(BillPay.Total).In元()}",
                    List = BillPay.Records
                }
            };
            BillCount = $"{BillPay.Records?.Count}条处方记录";
            TotalAmount = (BillPay.Total).In元();
        }

        protected override void Do()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");
                var preReq = new Req自助预结算();
                Construct(preReq);
                var preRes = DataHandler.RunExe<Res自助预结算>(preReq);
                if (!preRes.Success)
                {
                    ShowAlert(false, "预结算失败", $"预结算失败:{preRes.Message}");
                    return;
                }
                var preInfo = preRes.预结算信息;

                var amount = decimal.Parse(preInfo.还需支付) * 100m;
                if (amount > 0)
                {
                    var bank = decimal.Parse(preInfo.银行支付) * 100m;
                    var insurance = decimal.Parse(preInfo.医保支付) * 100m;
                    var total = decimal.Parse(preInfo.总计金额) * 100m;
                    PaymentModel.Self = amount;
                    PaymentModel.Insurance = insurance;
                    PaymentModel.Total = total;
                    PaymentModel.NoPay = false;
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.MidList = new List<PayInfoItem>
                    {
                        new PayInfoItem("总计金额：", total.In元(), true),
                        new PayInfoItem("医保支付：", insurance.In元()),
                        new PayInfoItem("银行账户支付：", bank.In元()),
                        new PayInfoItem("还需支付：", amount.In元()),
                    };

                    var info = Auth.Info;
                    PatientModel.PatientInfoIndex = 0;
                    PatientModel.Res病人信息查询 = new res病人信息查询()
                    {
                        data = new List<病人信息>()
                        {
                            new 病人信息()
                            {
                                name = info.NAME,
                                patientId = info.PATIENTNO,
                                idNo = info.IDNO,
                                accBalance = "0"
                            }
                        }
                    };

                    Next();
                }
                else
                {
                    var req = new Req自助结算();
                    Construct(req);
                    if (CardModel.CardType == CardType.省医保卡)
                    {
                        Invoke(DispatcherPriority.Input, () =>
                        {
                            var element = new SiHint()
                            {
                                DataContext = new
                                {
                                    Card = ResourceEngine.GetImageResourceUri("社保卡机器")
                                }
                            };
                            lp.ChangeMutiText(element);
                        });
                    }
                    var res = DataHandler.RunExe<Res自助结算>(req);
                    if (!res.Success)
                    {
                        ShowAlert(false, "缴费失败", $"缴费失败:{res.Message}");
                        return;
                    }
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        //Printables = BillPayPrintables(res),
                        PrintablesList = BillPayPrintables(res),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                }
            });
        }

        protected override Result Confirm()
        {
            return DoCommand(ConfirmAct).Result;
        }

        private Result ConfirmAct(LoadingProcesser lp)
        {
            lp.ChangeText("正在进行缴费结算...");
            var req = new Req自助结算支付宝();
            Construct(req);
            var payRes = ExtraPaymentModel.PaymentResult as 订单状态;
            req.支付金额 = (decimal.Parse(payRes.fee) / 100m).ToString("F2");
            req.支付宝流水号 = payRes.outPayNo;
            req.远图流水号 = payRes.outTradeNo;
            if (CardModel.CardType == CardType.省医保卡)
            {
                Invoke(DispatcherPriority.Input, () =>
                {
                    var element = new SiHint()
                    {
                        DataContext = new
                        {
                            Card = ResourceEngine.GetImageResourceUri("社保卡机器")
                        }
                    };
                    lp.ChangeMutiText(element);
                });
            }
            try
            {
                var res = DataHandler.RunExe<Res自助结算>(req);
                if (!res.Success)
                {
                    ShowAlert(false, "缴费失败", $"缴费失败:{res.Message}");
                    return Result.Fail(res.Message);
                }
                PrintModel.SetPrintInfo(true, new PrintInfo
                {
                    TypeMsg = "缴费成功",
                    TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                    //Printables = BillPayPrintables(res),
                    PrintablesList = BillPayPrintables(res, payRes),
                    TipImage = "提示_凭条"
                });
                Navigate(A.JF.Print);
                return Result.Success();
            }
            catch (Exception e)
            {
                return Result.Fail(-100, $"自助结算异常:{e.Message}");
            }
        }

        void Construct(Req自助结算 req)
        {
            var auth = GetInstance<IAuthModel>();
            var cardModel = GetInstance<ICardModel>();

            req.病人编号 = auth.Info.PATIENTID;
            switch (cardModel.CardType)
            {
                case CardType.身份证:
                    req.医保类别 = "1";
                    break;

                case CardType.市医保卡:
                    req.医保类别 = "2";
                    break;

                case CardType.省医保卡:
                    req.医保类别 = "3";
                    break;
            }
            var type1 = BillPay.Records.Where(i => i.YPFY == "1").ToList();
            if (type1.Any())
                req.处方识别 = type1.Aggregate(new StringBuilder(), (b, i) => b.Append($"{i.CFSB}|")).ToString();

            var type2 = BillPay.Records.Where(i => i.YPFY == "2").ToList();
            if (type2.Any())
                req.医技序号 = type2.Aggregate(new StringBuilder(), (b, i) => b.Append($"{i.CFSB}|")).ToString();
        }

        private List<Queue<IPrintable>> BillPayPrintables(Res自助结算 res, 订单状态 payRes = null)
        {
            var list = new List<Queue<IPrintable>>();
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{res.病人姓名}\n");
            sb.Append($"就诊卡号：{res.就诊卡号}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"医保支付：{res.医保支付}元\n");
            sb.Append($"自费支付：{res.自费支付}元\n");
            sb.Append($"金额总计：{res.总金额}元\n");
            sb.Append($"账户余额：{res.银医通余额}元\n");
            if (payRes != null)
            {
                sb.Append($"支付方式：支付宝\n");
                sb.Append($"支付账户：{payRes.buyerAccount}\n");
                sb.Append($"支付流水：{payRes.outPayNo}\n");
            }
            sb.Append($"发票号码：{res.发票号码}\n");
            sb.Append($"发药窗口：{res.发药窗口}\n");
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额"));
            if (res.处方信息List != null && res.处方信息List.Any())
                foreach (var detail in res.处方信息List)
                    queue.Enqueue(new PrintItemTriText(detail.项目名称, detail.数量, detail.金额 + "元"));
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            list.Add(queue);
            if (res.医技信息List != null && res.医技信息List.Any())
                list.AddRange(res.医技信息List.Select(BillPayPrintables));

            return list;
        }

        Queue<IPrintable> BillPayPrintables(医技信息 detail)
        {
            var queue = PrintManager.NewQueue("门诊导引单");
            var generator = GetInstance<IBarQrCodeGenerator>();
            var image = generator.BarcodeGenerate(detail.条码);
            queue.Enqueue(new PrintItemImage()
            {
                Align = ImageAlign.Center,
                Image = image,
                Width = 240,
                Height = 60,
            });
            var sb = new StringBuilder();
            sb.Append($"{detail.姓名} {detail.性别} {detail.年龄} {detail.医保类型}\n");
            sb.Append($"检查科室：{detail.检查科室}\n");
            sb.Append($"科室位置：{detail.检查地址}\n");
            sb.Append($"申检医生：{detail.开单医生}\n");
            sb.Append($"收费日期：{DateTimeCore.Now:yyyy-MM-dd}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"检查项目：{detail.检查名称}\n");
            sb.Append($"\n注：退费时请将此单与凭条一起给收费处\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}