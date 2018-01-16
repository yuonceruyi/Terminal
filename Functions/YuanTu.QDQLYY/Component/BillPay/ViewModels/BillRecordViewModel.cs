using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.UserControls;

namespace YuanTu.QDQLYY.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.QDKouQiangYY.Component.BillPay.ViewModels.BillRecordViewModel
    {
        protected override Queue<IPrintable> BillPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊费用缴费");
            var billPay = BillPayModel.Res缴费结算.data;
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var recordAll = BillRecordModel.Res获取缴费概要信息.data;
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"状态：缴费成功\n");
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"交易类型：自助缴费\n");
            sb.Append($"交易金额：{PaymentModel.Total.In元()}\n");

            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - PaymentModel.Total;
                    sb.Append($"缴费方式：预缴金\n");

                    sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                    sb.Append($"交易流水号：{billPay.transNo}\n");
                    sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
                    break;
                case PayMethod.银联:
                    var posinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    sb.Append($"缴费方式：银行卡\n");
                    if (!string.IsNullOrEmpty(posinfo?.Receipt))
                    {
                        sb.Append($"{posinfo?.Receipt}\n");
                    }
                    else
                    {
                        sb.Append($"银行卡号：{posinfo.CardNo}\n");
                        sb.Append($"授权号：{posinfo.Auth}\n");
                        sb.Append($"终端编号：{posinfo.TId}\n");
                        sb.Append($"终端流水：{posinfo.Trace}\n");
                        sb.Append($"批次号：{posinfo.Batch}\n");
                        sb.Append($"检索参考号：{posinfo.Ref}\n");
                        sb.Append($"交易日期：{posinfo.TransDate}\n");
                        sb.Append($"交易时间：{posinfo.TransTime}\n");
                    }
                    break;
                case PayMethod.社保:
                    var ybinfo = ExtraPaymentModel.PaymentResult as TransResDto;

                    reMain = Convert.ToDecimal(ExtraPaymentModel.ThridRemain); //余额

                    sb.Append($"缴费方式：医保卡\n");
                    sb.Append($"缴费后余额：{reMain.In元()}\n");
                    sb.Append($"卡号：{ybinfo.CardNo}\n");
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();

                    var str1 = string.Empty;
                    var str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Ref))
                    {
                        if (ybinfo.Ref.Length > 12)
                        {
                            str1 = ybinfo.Ref.Substring(0, 12);
                            str2 = ybinfo.Ref.Substring(12, ybinfo.Ref.Length - 12);
                        }
                        else
                        {
                            str1 = ybinfo.Ref;
                        }
                    }

                    sb.Append($"医院支付流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    str1 = string.Empty;
                    str2 = string.Empty;
                    if (!string.IsNullOrEmpty(ybinfo.Trace))
                    {
                        if (ybinfo.Trace.Length > 15)
                        {
                            str1 = ybinfo.Trace.Substring(0, 15);
                            str2 = ybinfo.Trace.Substring(15, ybinfo.Trace.Length - 15);
                        }
                        else
                        {
                            str1 = ybinfo.Trace;
                        }
                    }
                    sb.Append($"交易流水号：{str1}\n");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        sb.Append($"{str2}\n")
                            ;
                    }
                    queue.Enqueue(new PrintItemText { Text = sb.ToString() });
                    sb = new StringBuilder();
                    sb.Append($"批次号：{ybinfo.TId}\n");
                    sb.Append($"交易时间：{ybinfo.TransDate + ybinfo.TransTime}\n");
                    break;
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                    var thirdpayinfo = ExtraPaymentModel.PaymentResult as 订单状态;

                    sb.Append($"缴费方式：{PaymentModel.PayMethod.ToString()}\n");
                    sb.Append($"支付账号：{thirdpayinfo.buyerAccount}\n");
                    str1 = string.Empty;
                    str2 = string.Empty;
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
                    sb.Append($"交易时间：{thirdpayinfo.paymentTime}\n");
                    break;
                default:
                    break;
            }

            sb.Append($"发药窗口：{billPay.takeMedWin}\n");
            sb.Append($"收费项目明细：\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            sb.Clear();
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额", 220));

            foreach (var detail in recordAll.SelectMany(detail缴费概要 => detail缴费概要.billItem))
            {
                queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB(), 220));
            }

            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");            
            sb.Append($"请妥善保管好您的缴费凭证，\n");
            sb.Append($"如需打印儿童发票请到\n");
            sb.Append($"    儿童门诊大厅发票打印窗口，\n");
            sb.Append($"如需打印妇产发票请到\n");
            sb.Append($"    妇产门诊大厅发票打印窗口。\n");           
            if (PaymentModel.Total > 100)
            {
                sb.Append($"当日消费凭条可作为当日出门凭证使用\n");                
            }
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}
