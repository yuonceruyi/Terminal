using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.ZheJiangZhongLiuHospital.NativeService;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.Default.Component.BillPay.ViewModels.BillRecordViewModel
    {

        protected override void Do()
        {
            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            ChangeNavigationContent(SelectData.CatalogContent);

            var recordInfo = BillRecordModel.所选缴费概要;

            PaymentModel.Self = decimal.Parse(recordInfo.billFee);
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = decimal.Parse(recordInfo.billFee);
            PaymentModel.NoPay = true;
            PaymentModel.ConfirmAction = Confirm;

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
                new PayInfoItem("总金额：", PaymentModel.Self.In元()),
                //new PayInfoItem("医保报销：", PaymentModel.Insurance.In元()),
                //new PayInfoItem("支付金额：", PaymentModel.Total.In元(), true)
            };
            Next();
        }

        protected override Result Confirm()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在缴费结算,请稍候...");
                try
                {
                    var cardModel = GetInstance<ICardModel>();
                    var printModel = GetInstance<IPrintModel>();
                    var configurationManager = GetInstance<IConfigurationManager>();
                    var res = LianZhongHisService.ExcuteHospitalCheckout(cardModel.CardNo);
                    if (res.IsSuccess)
                    {
                        printModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "缴费成功",
                            TipMsg = $"您已成功于{DateTimeCore.Now:HH:mm}分缴费",
                            PrinterName = configurationManager.GetValue("Printer:Receipt"),
                            Printables = BillPayPrintables(res.Value.Replace("00|","")),
                            TipImage = "提示_凭条"
                        });
                        Navigate(A.JF.Print);
                        return Result.Success();
                    }
                    if (NavigationEngine.State != A.Third.PosUnion)
                    {
                        printModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "缴费失败",
                            DebugInfo = res.Message
                        });
                    }
                    Logger.Net.Info(res.Message);
                    if (res.Message.Contains("余额不足"))
                    {
                        Logger.Net.Info(res.Message.Split('差')[1]);
                        Logger.Net.Info(res.Message.Split('差')[1].Replace("元", ""));
                        ExtraPaymentModel.TotalMoney =decimal.Parse(res.Message.Split('差')[1].Replace("元", ""))*100;
                        BeginInvoke(DispatcherPriority.ContextIdle, () =>
                        {
                            ShowConfirm("账户余额不足", $"差额:{ExtraPaymentModel.TotalMoney.In元()}", cp =>
                            {
                                if (cp)
                                    StackNavigate(A.ChongZhi_Context, A.CZ.RechargeWay);
                            }, 30, ConfirmExModel.Build("去充值", "暂不", true));
                        });
                        return Result.Fail(res.Message);
                    }
                    ShowAlert(false, "友情提示", $"缴费失败:{res.Message}");
                    return Result.Fail(res.Message);
                }
                catch (Exception e)
                {
                    return Result.Fail($"支付异常{e}");
                }
            }).Result;
        }

        protected  Queue<IPrintable> BillPayPrintables(string output)
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var sb = new StringBuilder();
            foreach (var line in output.Split('|'))
            {
                sb.Append($"{line}\n");
            }
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的缴费凭证\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }


    }
}