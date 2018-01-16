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
using YuanTu.Consts.Models.BillPay;

namespace YuanTu.QDJZZXYY.Component.BillPay.ViewModels
{
    public class BillRecordViewModel : YuanTu.QDKouQiangYY.Component.BillPay.ViewModels.BillRecordViewModel
    {
        IBillPrePayModel billPrePayModel = GetInstance<IBillPrePayModel>();
        public override void OnEntered(NavigationContext navigationContext)
        {
            TipMsg = "我要缴费";
            ChangeNavigationContent("");
            BillRecordModel.Res获取缴费概要信息.data.ForEach(
                n => n.billItem.ForEach(l => l.billType = n.billType));
            Collection = BillRecordModel.Res获取缴费概要信息.data.Select(p => new PageData
            {
                CatalogContent = $"{p.doctName} {p.deptName}\r\n金额 {p.billFee.In元()}",
                List = p.billItem,
                Tag = p,
            }).ToArray();
            BillCount = $"{BillRecordModel.Res获取缴费概要信息.data.Count}张处方单";
            TotalAmount = billPrePayModel.Res缴费预结算.data.selfFee.In元();
            
            
            PlaySound(SoundMapping.选择待缴费处方);
        }

        /// <summary>
        /// 多个处方同时缴费
        /// </summary>
        protected override void Do()
        {

            BillRecordModel.所选缴费概要 = SelectData.Tag.As<缴费概要信息>();
            var recordInfo = BillRecordModel.所选缴费概要;
            ChangeNavigationContent($"金额 {TotalAmount}");

            PaymentModel.Self = decimal.Parse(billPrePayModel.Res缴费预结算.data.selfFee);//总费用
            PaymentModel.Insurance = decimal.Parse("0");
            PaymentModel.Total = PaymentModel.Self;
            PaymentModel.NoPay = false;
            PaymentModel.ConfirmAction = Confirm;

            PaymentModel.LeftList = new List<PayInfoItem>()
            {
                new PayInfoItem("卡号：",PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].patientId),
                new PayInfoItem("医保类型：",PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex].patientType??"自费"),
                new PayInfoItem("处方单数：",$"{BillRecordModel.Res获取缴费概要信息.data.Count}张"),
            };

            PaymentModel.RightList = new List<PayInfoItem>()
            {
                new PayInfoItem("个人支付：",PaymentModel.Self.In元()),
                new PayInfoItem("医保报销：",PaymentModel.Insurance.In元()),
                new PayInfoItem("支付金额：",PaymentModel.Total.In元(),true),
            };
            Next();
        }

        protected override void FillBaseRequest()
        {
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var record = BillRecordModel.所选缴费概要;

            BillPayModel.Req缴费结算 = new req缴费结算
            {
                patientId = patientInfo.patientId,
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.PayMethod.GetEnumDescription(),
                cash = PaymentModel.Total.ToString(CultureInfo.InvariantCulture),
                accountNo = patientInfo.patientId,
                billNo = string.Join("|", BillRecordModel.Res获取缴费概要信息.data.Select(one => $"{one.billNo}")),//"|"分割拼接billNo
                allSelf = PaymentModel.Insurance == 0 ? "1" : "0",
                balBillNo = billPrePayModel.Res缴费预结算.data.balBillNo,
            };
        }

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
            queue.Enqueue(new PrintItemTriText("名称", "数量", "金额", 230));

            foreach (var detail in recordAll.SelectMany(detail缴费概要 => detail缴费概要.billItem))
            {
                queue.Enqueue(new PrintItemTriText(detail.itemName, detail.itemQty, detail.billFee.InRMB(), 220));
            }

            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"制单时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"制单医生：{recordAll.FirstOrDefault()?.doctName}\n");
            sb.Append($"请妥善保管好您的缴费凭证，\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}
