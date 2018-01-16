using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using SerialPrinter = YuanTu.Devices.Printer;

namespace YuanTu.QDKouQiangYY.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        protected override void OnCAClick()
        {
            if (ConfigurationManager.GetValueInt("StopRecharge:Enabled") == 1)
            {
                DateTime dtime;
                var time = ConfigurationManager.GetValue("StopRecharge:Time");
                if (DateTime.TryParseExact(time, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dtime))
                {
                    if (DateTimeCore.Now >
                        new DateTime(DateTimeCore.Now.Year, DateTimeCore.Now.Month, DateTimeCore.Now.Day, dtime.Hour,
                            dtime.Minute, 0))
                    {
                        ShowAlert(false, "现金缴住院押金", $"{time}后停止现金缴住院押金\r\n请使用其他支付方式");
                        return;
                    }
                }
            }
            if (ConfigurationManager.GetValue("CashBox:Type") == "JCM")
            {
                Navigate(A.Third.JCMCash);
            }
            else
            {
                Navigate(A.Third.AtmCash);
            }
        }
        protected override Queue<IPrintable> IpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("住院押金");
            var patientInfo = PatientModel.住院患者信息;
            var sb = new StringBuilder();
            sb.Append($"状态：缴押金{(success ? "成功" : "失败")}\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"住院号：{patientInfo.patientHosId}\n");
            sb.Append($"缴押金方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"缴押金前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"缴押金金额：{IpRechargeModel.Req住院预缴金充值.cash.In元()}\n");

            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    var str1 = string.Empty;
                    var str2 = string.Empty;

                    sb.Append($"缴费方式：{ExtraPaymentModel.CurrentPayMethod.ToString()}\n");
                    sb.Append($"支付账号：{thirdpayinfo?.buyerAccount}\n");

                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n");
                    }
                    sb.Append($"交易时间：{thirdpayinfo?.paymentTime}\n");
                    break;
                default:
                    break;
            }
            if (success)
            {
                sb.Append($"缴押金后余额：{IpRechargeModel.Res住院预缴金充值.data.cash.In元()}\n");
                sb.Append($"收据号：{IpRechargeModel.Res住院预缴金充值.data.receiptNo}\n");
            }
            else
            {
                sb.Append($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}\n");
            }
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保管好您的个人信息。\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
        public override void SerialPrint(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return;
                }
            }

            SerialPrinter.SerialPrinter printer = new SerialPrinter.SerialPrinter();
            var context = printer.GetContext();
            context[SerialPrinter.PrintableContext.SerialPort] = "COM6";
            var apt = printer.Connect(context);
            if (!apt.Success)
            {
                ShowAlert(false, "打印失败", "连接打印机失败:" + apt.Message);
            }
            printer.SetLeftSpacing(30, 0);
            printer.SetFontSize(2, 2);
            printer.SetAlign(1);//居中
            printer.Text("住院押金").FeedLine();
            printer.SetFontSize(1, 1);
            printer.SetAlign(0);//左侧                
            printer.Text($"状态：缴押金{(success ? "成功" : "失败")}").FeedLine();
            printer.Text($"姓名：{PatientModel.住院患者信息.name}").FeedLine();
            printer.Text($"住院号：{PatientModel.住院患者信息.patientHosId}").FeedLine();
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;
                    printer.Text($"缴费方式：银行卡").FeedLine();
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        printer.Text($"{posinfo?.Receipt}").FeedLine();
                    }
                    else
                    {
                        printer.Text($"银行卡号：{posinfo.CardNo}").FeedLine();
                        printer.Text($"授权号：{posinfo.Auth}").FeedLine();
                        printer.Text($"终端编号：{posinfo.TId}").FeedLine();
                        printer.Text($"终端流水：{posinfo.Trace}").FeedLine();
                        printer.Text($"批次号：{posinfo.Batch}").FeedLine();
                        printer.Text($"检索参考号：{posinfo.Ref}").FeedLine();
                        printer.Text($"交易日期：{posinfo.TransDate}").FeedLine();
                        printer.Text($"交易时间：{posinfo.TransTime}").FeedLine();
                    }
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;
                    var str1 = string.Empty;
                    var str2 = string.Empty;

                    printer.Text($"缴费方式：{ExtraPaymentModel.CurrentPayMethod.ToString()}").FeedLine();
                    printer.Text($"支付账号：{thirdpayinfo?.buyerAccount}").FeedLine();

                    if (!string.IsNullOrEmpty(thirdpayinfo?.outTradeNo))
                    {
                        if (thirdpayinfo?.outTradeNo.Length > 15)
                        {
                            str1 = thirdpayinfo.outTradeNo.Substring(0, 15);
                            str2 = thirdpayinfo.outTradeNo.Substring(15, thirdpayinfo.outTradeNo.Length - 15);
                        }
                        else
                        {
                            str1 = thirdpayinfo?.outTradeNo;
                        }
                    }
                    printer.Text($"交易流水号：{str1}").FeedLine();
                    if (!string.IsNullOrEmpty(str2))
                    {
                        printer.Text($"{str2}").FeedLine();
                    }
                    break;
                default:
                    printer.Text($"缴费方式：现金").FeedLine();
                    break;
            }

            printer.Text($"缴费前余额：{Convert.ToDecimal(PatientModel.住院患者信息.accBalance).In元()}").FeedLine();
            printer.Text($"缴费金额：{ExtraPaymentModel.TotalMoney.In元()}").FeedLine();
            if (success)
            {
                decimal remain = Convert.ToInt32(PatientModel.住院患者信息.accBalance) + ExtraPaymentModel.TotalMoney;
                printer.Text($"缴费后余额：{remain.In元()}").FeedLine();
            }
            else
            {
                printer.Text($"异常原因：{IpRechargeModel.Res住院预缴金充值.msg}").FeedLine();
            }
            string[] time = IpRechargeModel.Req住院预缴金充值.tradeTime.Split(' ');
            printer.Text($"交易日期：{time[0]}").FeedLine();
            printer.Text($"交易时间：{time[1]}").FeedLine();
            printer.Text($"柜员编号：{FrameworkConst.OperatorId}").FeedLine();
            printer.Text($"请妥善保管好您的个人信息").FeedLine();
            printer.Text($"祝您早日康复！").FeedLine();

            printer.CutPaper(0);
            printer.Print();
            printer.Disconnect();
        }
    }
}