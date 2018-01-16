using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;
using YuanTu.Core.Extension;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.XiaoShanZYY.Component.BillPay.Models;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.BillPay.ViewModels
{
    class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {
        [Dependency]
        public IBillPayModel BillPay { get; set; }
        [Dependency]
        public IBillPayService BillPayService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");

            TipMsg = "预结算";

            Collection = new[]{new PageData()
            {
                CatalogContent = $"总金额:{BillPay.Sum.In元()}",
                List =  BillPay.FEIYONGMX.Select(d=>new MENZHENFYXX()
                {
                    XIANGMUMC = d.XIANGMUMC,
                    XIANGMUGLMC = d.XIANGMUGLMC,
                    DANJIA = Yuan2Fen(d.DANJIA),
                    SHULIANG = d.SHULIANG,
                    JINE = Yuan2Fen(d.JINE),
                }),
                Tag = BillPay.FEIYONGMX,
            }};

            TotalAmount = BillPay.Sum.In元();

            //PlaySound(SoundMapping.选择待缴费处方);
        }

        string Yuan2Fen(string yuan)
        {
            if (decimal.TryParse(yuan, out decimal value))
                return (value * 100).ToString("0");
            return yuan;
        }

        static readonly char[] Spliter = new[] { ':', '：' };
        string SplitColon(string value)
        {
            var list = value.Split(Spliter);
            if (list.Length < 2)
                return value;
            return list[1];
        }
        protected override void Do()
        {
            DoCommand(lp =>
            {
                lp.ChangeText("正在计算费用信息，请稍候...");
                var result = BillPayService.缴费预结算();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "缴费预结算", $"缴费预结算失败{result.Message}");
                    return;
                }
                try
                {
                    var res = BillPay.Res预结算;
                    var self = decimal.Parse(SplitColon(res.应付金额)) * 100;
                    var insurance = decimal.Parse(SplitColon(res.医保报销金额)) * 100;
                    var total = decimal.Parse(SplitColon(res.单据总金额)) * 100;
                    var balance = decimal.Parse(SplitColon(res.市民卡余额)) * 100;

                    PaymentModel.Self = self;
                    PaymentModel.Insurance = insurance;
                    PaymentModel.Total = total;
                    PaymentModel.NoPay = self == 0;
                    PaymentModel.ConfirmAction = Confirm;

                    PaymentModel.MidList = new List<PayInfoItem>
                    {
                        new PayInfoItem("市民卡余额：", balance.In元()),
                        new PayInfoItem("医保报销金额：", insurance.In元()),
                        new PayInfoItem("应付金额：", self.In元()),
                        new PayInfoItem("总金额：", total.In元(), true),
                    };
                    ChangeNavigationContent(SelectData.CatalogContent);
                    Next();
                }
                catch (Exception ex)
                {
                    ShowAlert(false, "获取费用明细失败", "HIS返回的数据格式不正确，请联系导医处理.", debugInfo: ex.Message);
                }



            });
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在进行缴费，请稍候...");

                var result = BillPayService.缴费结算();
                if (result.IsSuccess)
                {
                    PrintModel.SetPrintInfo(true, new PrintInfo
                    {
                        TypeMsg = "缴费成功",
                        TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BillPayPrintables(),
                        TipImage = "提示_凭条"
                    });
                    Navigate(A.JF.Print);
                    return Result.Success();
                }

                //第三方支付失败时去支付流程里面处理，不在业务里面处理
                if (NavigationEngine.State != A.Third.PosUnion)
                {
                    //PrintModel.SetPrintInfo(false, "缴费失败", errorMsg: BillPayModel.Res缴费结算?.msg);
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = "缴费失败",
                        DebugInfo = BillPayModel.Res缴费结算?.msg
                    });
                    Navigate(A.JF.Print);
                }
                return result;
            }).Result;
        }

        private static readonly Font printFont4 = new Font("宋体", 10, FontStyle.Regular);
        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("自助缴费");
            var builder = new StringBuilder();
            builder.AppendLine($"【单据总金额】{SplitColon(BillPay.Res结算.单据总金额)} 元");
            builder.AppendLine($"【应付金额】{SplitColon(BillPay.Res结算.应付金额)} 元");
            builder.AppendLine($"【医保报销金额】{SplitColon(BillPay.Res结算.医保报销金额)} 元");
            builder.AppendLine($"【医保本年账户余额】{SplitColon(BillPay.Res结算.医保本年账户余额)} 元");
            builder.AppendLine($"【医保历年账户余额】{SplitColon(BillPay.Res结算.医保历年账户余额)} 元");
            builder.AppendLine($"【市民卡支付金额】{SplitColon(BillPay.Res结算.市民卡支付金额)} 元");
            builder.AppendLine($"【市民卡账户余额】{SplitColon(BillPay.Res结算.市民卡账户余额)} 元");
            builder.AppendLine($"【移动支付金额】{SplitColon(BillPay.Res结算.移动支付)} 元");
            var paymentModel = ServiceLocator.Current.GetInstance<IPaymentModel>();
            if (!paymentModel.NoPay)
            {
                var epm = ServiceLocator.Current.GetInstance<IExtraPaymentModel>();
                var payMethod = epm.CurrentPayMethod;
                builder.AppendLine("【支付方式】" + payMethod);
            }
            builder.AppendLine($"【结算单号】{SplitColon(BillPay.Res结算.电脑号)}");
            queue.Enqueue(new PrintItemText { Text = builder.ToString() });

            var list = BillPay.FEIYONGMX;
            if (list != null && list.Count > 0)
            {
                queue.Enqueue(new PrintItemText { Text = "【收费项目明细】\n" });
                queue.Enqueue(
                    new PrintItemRatioText("名称", "数量", "金额") { Font = printFont4 });
                foreach (var info in list)
                    queue.Enqueue(
                        new PrintItemRatioText(info.XIANGMUMC, info.SHULIANG, Convert.ToDouble(info.JINE).ToString("F2"))
                        {
                            Font = printFont4
                        });
            }
            builder.Clear();
            var address = SplitColon(BillPay.Res结算.取药窗口);
            if (!string.IsNullOrEmpty(address))
                builder.AppendLine($"【取药窗口】{address}窗口\n");
            builder.AppendLine("\n     如需打印发票请到窗口办理\n");
            builder.AppendLine(".");
            builder.AppendLine(".");
            builder.AppendLine(".");
            queue.Enqueue(new PrintItemText { Text = builder.ToString() });
            return queue;
        }
    }
}
