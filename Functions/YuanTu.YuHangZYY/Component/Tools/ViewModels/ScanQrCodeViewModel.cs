using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;

namespace YuanTu.YuHangZYY.Component.Tools.ViewModels
{
    public class ScanQrCodeViewModel: YuanTu.Default.Component.Tools.ViewModels.ScanQrCodeViewModel
    {
        protected override void InitQrCode()
        {
            var code = -1;
            switch (ExtraPaymentModel.CurrentPayMethod)
            {
                case PayMethod.微信支付:
                    Tips = "请使用微信扫一扫支付";
                    code = 2;
                    break;

                case PayMethod.支付宝:
                    Tips = "请使用支付宝扫一扫支付";
                    code = 1;
                    break;

                default:
                    ShowAlert(false, "温馨提示", "不支持该支付方式!");
                    return;
            }
            DoCommand(p =>
            {
                p.ChangeText("正在创建扫码订单，请稍候...");
                var optType = OptDic.ContainsKey(ExtraPaymentModel.CurrentBusiness)
                    ? OptDic[ExtraPaymentModel.CurrentBusiness]
                    : string.Empty;
                var rest = DataHandlerEx.创建扫码订单(new req创建扫码订单
                {
                    idNo = ExtraPaymentModel.PatientInfo.IdNo,
                    idType = "1",
                    patientName = ExtraPaymentModel.PatientInfo.Name,
                    patientId = ExtraPaymentModel.PatientInfo.PatientId,
                    guarderId = ExtraPaymentModel.PatientInfo.GuardianNo,
                    billNo = GetInstance<IBusinessConfigManager>().GetFlowId("创建扫码订单的billNo"),
                    fee = ((int)ExtraPaymentModel.TotalMoney).ToString(),
                    optType = optType,
                    subject = ExtraPaymentModel.CurrentBusiness.ToString(),
                    deviceInfo = FrameworkConst.OperatorId,
                    feeChannel = code.ToString(),
                    source = FrameworkConst.OperatorId,
                    outId = OuterId.ToString(),
                    extend = $"{ExtraPaymentModel.PatientInfo.CardNo}|{(int)ExtraPaymentModel.PatientInfo.CardType}"
                });
                if (!rest.success)
                {
                    ShowAlert(false, "温馨提示", "获取支付二维码失败\r\n" + rest.msg);
                    Exitloop = true;
                    Preview(); //回退
                    return;
                }
                Logger.Main.Info(
                    $"病人[{ExtraPaymentModel.PatientInfo.Name} {ExtraPaymentModel.PatientInfo.PatientId}] 开始[{ExtraPaymentModel.CurrentPayMethod}]");
                订单扫码 = rest.data;

                var barQrCodeGenerater = GetInstance<IBarQrCodeGenerator>();
                QrCodeImage = barQrCodeGenerater.QrcodeGenerate(订单扫码.qrCode,
                    Image.FromFile(
                       ResourceEngine
                            .GetResourceFullPath(PayLogoMapping[ExtraPaymentModel.CurrentPayMethod])));


                Looping = true;
                Procuding = false;
                Exitloop = false;
                Task.Factory.StartNew(AskingLoop); //创建成功则进行轮询
            });
        }
        protected override void StartPay()
        {
            Looping = false;
            Procuding = true;
            var rechargeb = new[] { Business.充值, Business.住院押金 };
            var isrecharge = rechargeb.Contains(ExtraPaymentModel.CurrentBusiness);
            //上送交易信息
            TradeUploadService.UploadAsync(new TradeInfo()
            {
                TradeName = ExtraPaymentModel.CurrentBusiness.ToString(),
                PatientId = ExtraPaymentModel.PatientInfo.PatientId,
                PatientName = ExtraPaymentModel.PatientInfo.Name,
                CardNo = ExtraPaymentModel.PatientInfo.CardNo,
                CardType = ExtraPaymentModel.PatientInfo.CardType,
                IdNo = ExtraPaymentModel.PatientInfo.IdNo,
                GuardianIdNo = ExtraPaymentModel.PatientInfo.GuardianNo,
                PayMethod = ExtraPaymentModel.CurrentPayMethod,
                TradeType = isrecharge ? TradeType.充值成功 : TradeType.交易成功,
                TradeId = 订单状态?.outTradeNo,
                OriginTradeId = "",
                AccountNo = 订单状态?.buyerAccount,
                Amount = ExtraPaymentModel.TotalMoney,
                TradeDetail = 订单状态.ToJsonString(),
            });
            ExtraPaymentModel.FinishFunc?.Invoke().ContinueWith(rt =>
            {
                var actRest = rt.Result;
                var code = actRest.ResultCode;
                if (!actRest.IsSuccess)
                {
                    var errorMsg =
                        $"{actRest.Message.BackNotNullOrEmpty("扫码消费成功，HIS返回未知结果")}\r\nHIS交易失败\r\n交易返回码:{code}\r\n请执凭条到人工服务台咨询此交易结果！";
                    // var errorMsg = $"扫码消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                    ShowAlert(false, "温馨提示", errorMsg);
                    打印网关未知异常凭证(errorMsg);
                }
                else
                {
                    if (actRest.IsSuccess)
                    {
                        try
                        {
                            Logger.Main.Info($"扫码支付成功流程");
                            var reportResult = DataHandlerEx.操作成功状态上传(new req操作成功状态上传
                            {
                                outTradeNo = 订单状态?.outTradeNo,
                                status = "200" //成功

                            });
                            if (!reportResult?.success ?? true)
                                Logger.Main.Error(
                                    $"操作状态上传失败 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 信息:{reportResult?.msg}");
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error(
                                $"操作状态上传异常 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}");
                        }

                        Procuding = false;
                        return;
                    }
                    else
                    {
                        try
                        {
                            Logger.Main.Info($"扫码支付失败流程");
                            var reportResult = DataHandlerEx.操作成功状态上传(new req操作成功状态上传
                            {
                                outTradeNo = 订单状态?.outTradeNo,
                                status = "101" //失败

                            });
                            if (!reportResult?.success ?? true)
                                Logger.Main.Error(
                                    $"操作状态上传失败 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 信息:{reportResult?.msg}");

                            //上送交易信息
                            TradeUploadService.UploadAsync(new TradeInfo()
                            {
                                TradeName = ExtraPaymentModel.CurrentBusiness.ToString(),
                                PatientId = ExtraPaymentModel.PatientInfo.PatientId,
                                PatientName = ExtraPaymentModel.PatientInfo.Name,
                                CardNo = ExtraPaymentModel.PatientInfo.CardNo,
                                CardType = ExtraPaymentModel.PatientInfo.CardType,
                                IdNo = ExtraPaymentModel.PatientInfo.IdNo,
                                GuardianIdNo = ExtraPaymentModel.PatientInfo.GuardianNo,
                                PayMethod = ExtraPaymentModel.CurrentPayMethod,
                                TradeType = isrecharge ? TradeType.充值撤销成功 : TradeType.交易撤销成功,
                                TradeId = 订单状态?.outTradeNo,
                                OriginTradeId = "",
                                AccountNo = 订单状态?.buyerAccount,
                                Amount = ExtraPaymentModel.TotalMoney,
                                TradeDetail = reportResult.ToJsonString()
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Main.Error(
                                $"操作状态上传异常 用户Id:{ExtraPaymentModel.PatientInfo.PatientId} 平台支付流水:{订单扫码?.outTradeNo} 二维码:{订单扫码?.qrCode} 异常:{ex.Message}");
                        }

                        Procuding = false;
                        return;
                    }
                }

                Procuding = false;
                if (NavigationEngine.State == A.ThirdPay) //只有在当前页，才允许Leaving
                {
                    Preview();
                    return;
                }
                Procuding = false;
            });
        }
        protected override void 打印网关未知异常凭证(string 异常描述)
        {
            var queue = PrintManager.NewQueue($"{ExtraPaymentModel.CurrentPayMethod}单边账");
            var sb = new StringBuilder();
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo?.Name}\n");
            sb.Append($"病案号：{ExtraPaymentModel.PatientInfo?.PatientId}\n");
            sb.Append($"当前业务：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"订单号：{订单扫码?.outTradeNo}\n");
            sb.Append($"交易金额：{ExtraPaymentModel?.TotalMoney.In元()}\n");
            sb.Append($"支付账户：{订单状态?.buyerAccount}\n");
            sb.Append($"异常描述：{异常描述}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请联系导诊处理，祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), queue);
        }
    }
}
