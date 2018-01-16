using System;
using System.Windows;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Commands;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using System.Collections.Generic;
using System.Drawing;
using YuanTu.QDArea;
using YuanTu.QDArea.Enums;
using Color = System.Windows.Media.Color;
using FontStyle = System.Drawing.FontStyle;

namespace YuanTu.QDQLYY.Component.TakeNum.ViewModels
{
    public class TakeNumViewModel:YuanTu.QDKouQiangYY.Component.TakeNum.ViewModels.TakeNumViewModel
    {
        public Font HeaderFont2 = new Font("微软雅黑", 14, FontStyle.Bold);

        /// <summary>
        /// 规范取号密码字段，所以项目本地化
        /// </summary>
        protected override void CancelAction()
        {
            var record = RecordModel.所选记录;
            var textblock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            textblock.Inlines.Add("\r\n您确定要取消");
            textblock.Inlines.Add(new TextBlock { Text = $"{ record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy-MM-dd") } { record.medAmPm.SafeToAmPm() } { record.deptName }", Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0)) });
            textblock.Inlines.Add("\r\n的预约吗？\r\n\r\n\r\n\r\n");
            ShowConfirm("友好提醒", textblock, b =>
            {
                if (!b) return;
                DoCommand(lp =>
                {
                    lp.ChangeText("正在进行取消预约，请稍候...");

                    var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];

                    CancelAppoExtendModel.version = ConfigQD.ScheduleVersion;
                    CancelAppoExtendModel.lockId = "";

                    CancelAppoModel.Req取消预约 = new req取消预约
                    {
                        appoNo = record.regNo,
                        patientId = patientInfo.patientId,
                        operId = FrameworkConst.OperatorId,
                        regMode = "1",
                        cardNo = CardModel.CardNo,
                        cardType = ((int)CardModel.CardType).ToString(),
#pragma warning disable 612
                        medDate = record.medDate,
                        scheduleId = record.scheduleId,
                        medAmPm = record.medAmPm,
                        orderNo = record.orderNo,
#pragma warning restore 612
                        extend = CancelAppoExtendModel.ToJsonString(),
                    };
                    CancelAppoModel.Res取消预约 = DataHandlerEx.取消预约(CancelAppoModel.Req取消预约);
                    if (CancelAppoModel.Res取消预约?.success ?? false)
                    {
                        ShowAlert(true, "取消预约", "您已取消预约成功");
                        RecordModel.Res挂号预约记录查询.data.Remove(RecordModel.所选记录);
                        if (RecordModel.Res挂号预约记录查询.data.Count > 0)
                            Navigate(A.QH.Record);
                        else Navigate(A.Home);
                    }
                    else
                    {
                        ShowAlert(false, "取消预约", "取消预约失败", debugInfo: CancelAppoModel.Res取消预约?.msg);


                    }

                });

            }, 60, ConfirmExModel.Build("是", "否", true));
        }

        protected override void FillBaseRequest()
        {
            var patientInfo = PatientModel.当前病人信息;
            var record = RecordModel.所选记录;
            TakeNumExtendModel.version = ConfigQD.ScheduleVersion;
            var extraPaymentModel = GetInstance<IExtraPaymentModel>();
            if (extraPaymentModel.CurrentPayMethod == PayMethod.预缴金 || PaymentModel.NoPay)
            {
                TakeNumExtendModel.payStatus = RecordModel.所选记录.payStatus;
            }
            else
            {
                TakeNumExtendModel.payStatus = ((int)ApptPayStatus.支付成功).ToString();
            }

            TakeNumModel.Req预约取号 = new req预约取号
            {
                patientId = patientInfo.patientId,
                cardType = ((int)CardModel.CardType).ToString(),
                cardNo = CardModel.CardNo,
                orderNo = RecordModel.所选记录.orderNo,
                operId = FrameworkConst.OperatorId,
                tradeMode = PaymentModel.NoPay ? PayMethod.预缴金.GetEnumDescription() : PaymentModel.PayMethod.GetEnumDescription(),
                accountNo = patientInfo.patientId,
                cash = PaymentModel.Total.ToString(),
#pragma warning disable 612
                medDate = record.medDate,
                scheduleId = record.scheduleId,
                medAmPm = record.medAmPm,
#pragma warning restore 612
                extend = TakeNumExtendModel.ToJsonString(),
            };
        }
        protected override Queue<IPrintable> TakeNumPrintables()
        {
            var queue = PrintManager.NewQueue("取号单");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var takeNum = TakeNumModel.Res预约取号.data;
            var record = RecordModel.所选记录;
            var reMain = 0m;

            var sb = new StringBuilder();
            sb.Append($"------------------------------------\n");
            sb.Append($"状态：取号成功\n");

            #region 个人主要信息
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"就诊卡号：{patientInfo.cardNo}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            #endregion

            #region  大粗字体,诊疗信息
            sb = new StringBuilder();
            sb.Append($"挂号序号：{record.deptName} {takeNum?.appoNo}号\n");
            sb.Append($"就诊医生：{record.doctName}\n");
            sb.Append($"就诊地址：{takeNum?.address}\n");
            sb.Append($"就诊时间：{record.medDate} {record.medAmPm.SafeToAmPm()} {record?.medTime}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString(), Font = HeaderFont2 });
            #endregion

            #region 交易信息
            sb = new StringBuilder();
            sb.Append($"交易类型：预约取号\n");
            if (RecordModel.所选记录.payStatus == ((int)ApptPayStatus.支付成功).ToString())
            {
                sb.Append($"支付状态：预约时已支付\n");
                sb.Append($"缴费方式：预缴金\n");
                sb.Append($"当前余额：{patientInfo.accBalance?.ToString().In元()}\n");
                sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            }
            else
            {
                sb.Append($"支付状态：取号时支付\n");
            }
            sb.Append($"交易金额：{record.regAmount.In元()}\n");
            switch (PaymentModel.PayMethod)
            {
                case PayMethod.预缴金:
                    reMain = Convert.ToDecimal(patientInfo.accBalance ?? "0") - Convert.ToDecimal(record.regAmount ?? "0");
                    sb.Append($"缴费方式：预缴金\n");
                    sb.Append($"缴费后余额：{reMain.ToString().In元()}\n");
                    sb.Append($"交易流水号：{takeNum.transNo}\n");
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
            #endregion

            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            quhaoSpicalPrint(queue);
            sb = new StringBuilder();
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
        protected virtual void quhaoSpicalPrint(Queue<IPrintable> queue)
        {
            var sb = new StringBuilder();
            var paiban = RecordModel.所选记录;

            sb.Append($"------------------------------------\n");
            sb.Append($"温馨提示：\n");
            sb.Append($"1、请妥善保管好您的凭条。\n");
            sb.Append($"2、此号当日有效，如退号请在三日内办理。\n");
            sb.Append($"3、如需打印发票，儿童到一楼门诊大厅,\n");
            sb.Append($"妇产到负一楼门诊大厅。\n");

            if (Convert.ToDecimal(paiban.regAmount) > 100)
            {
                sb.Append($"4、当日消费凭条可作为当日出门凭证使用。\n");
            }
            sb.Append($"------------------------------------\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
        }
    }
}
