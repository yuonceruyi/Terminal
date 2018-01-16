using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Services;
using YuanTu.Core.Log;
using YuanTu.Consts.Enums;
using YuanTu.HuNanHangTianHospital.Common;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Gateway;

namespace YuanTu.HuNanHangTianHospital.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        protected override void StartPosFlow()
        {
            ExtraPaymentModel.CurrentPayMethod = PayMethod.银联;
            //  var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
            Task.Run(() =>
           {
               StopTimer();
               if (string.IsNullOrEmpty(ExtraPaymentModel.TotalMoney.ToString()))
               {
                   ShowAlert(false, "扣费失败", "金额有误，交易失败，请重试！");
                   return;
               }
               var tradeResBytes = new byte[1024];
               var tradeResponseCode = FrameworkConst.Local ? KY.Trade("1", ref tradeResBytes) 
                                                            : KY.Trade(ExtraPaymentModel.TotalMoney.ToString(), ref tradeResBytes);//
               var transRes = Convert(tradeResBytes);
               ExtraPaymentModel.PaymentResult = transRes;
               var bankMessage = CreateBankResMessage(transRes.RespCode);
               if (tradeResponseCode == (int)TradeResCodeType.交易成功 && transRes.RespCode == "00")
               {
                   Logger.Main.Info("卡友交易成功，进入委托方法");
                   var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
                   tsk.ContinueWith(payRet =>
                    {
                        if (!payRet?.Result.IsSuccess ?? false)
                        {
                            if (FrameworkConst.VirtualThridPay)
                            {
                                ShowAlert(false, "扣费失败", "交易失败，请重试！" + payRet?.Result.Message);
                                return;
                            }
                            var code = payRet?.Result.ResultCode ?? 0;
                            if (DataHandler.UnKnowErrorCode.Contains(code))
                            {
                                var errorMsg = $"银联消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";

                                PrintModel.SetPrintInfo(false, new PrintInfo
                                {
                                    TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边账",
                                    TipMsg = errorMsg,
                                    PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                    Printables = GatewayUnknowErrorPrintables(errorMsg),
                                    TipImage = "提示_凭条"
                                });
                                PrintManager.Print();
                                ShowAlert(false, "业务处理异常", errorMsg);
                                CloseDevices(errorMsg);
                                KY.MoveOutCard();
                                Navigate(A.Home);
                            }
                            else
                            {
                                var revokeResBytes = new byte[1024];
                                var refundRet = KY.Revoke(transRes.Trace, transRes.Amount, ref revokeResBytes);
                                var revokeRes = Convert(revokeResBytes);
                                if (!refundRet)
                                {
                                    PrintModel.SetPrintInfo(false, new PrintInfo
                                    {
                                        TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                                        TipMsg = $"银联冲正失败，请持凭条与医院工作人员联系！",
                                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                        Printables = RefundFailPrintables(payRet?.Result.Message, "冲正返回码：" + revokeRes.RespCode),
                                        TipImage = "提示_凭条"
                                    });
                                    PrintManager.Print();
                                    ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                }
                                else
                                {
                                    ShowAlert(false, "扣费失败", "交易失败，请重试！，金额已经退还");
                                }
                                KY.MoveOutCard();
                                TryPreview();
                            }
                        }
                        CloseDevices("消费结束");
                    });
                   return;
               }
               else if (tradeResponseCode == (int)TradeResCodeType.取消交易)
               {
                   ShowAlert(false, "银联交易", "用户主动取消交易");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else if (tradeResponseCode == (int)TradeResCodeType.余额不足)
               {
                   ShowAlert(false, "银联交易", "余额不足");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else if (tradeResponseCode == (int)TradeResCodeType.密码错误)
               {
                   ShowAlert(false, "银联交易", "密码错误");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else if (tradeResponseCode == (int)TradeResCodeType.初始化失败)
               {
                   ShowAlert(false, "银联交易", "银联接口初始化失败，请换其他付款方式");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else if (tradeResponseCode == (int)TradeResCodeType.未插卡或者卡已损坏)
               {
                   ShowAlert(false, "银联交易", "未插入卡,或者卡已损坏");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else if (!string.IsNullOrEmpty(bankMessage))
               {
                   ShowAlert(false, "银联交易", bankMessage);
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
               else
               {
                   ShowAlert(false, "银联交易", "银联交易异常，请换其他付款方式");
                   Logger.Main.Info($"银联交易出现异常,请换其他付款方式 卡有返回码{tradeResponseCode}，银联返回码{transRes.RespCode}");
                   KY.MoveOutCard();
                   Navigate(A.Home);
               }
           });
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            return base.OnLeaving(navigationContext);
        }
        private TransResDto Convert(byte[] b)
        {
            return new TransResDto
            {
                //Type = Encoding.Default.GetString(b, 0, 2),
                Amount = Encoding.Default.GetString(b, 2, 12),
                MId = Encoding.Default.GetString(b, 14, 15),
                TId = Encoding.Default.GetString(b, 29, 8),
                CardNo = Encoding.Default.GetString(b, 37, 19),
                TransDate =DateTimeCore.Now.Year+ Encoding.Default.GetString(b, 56, 4),
                TransTime = Encoding.Default.GetString(b, 60, 10),
                // ExpTime = Encoding.Default.GetString(b, 70, 4),
                Trace = Encoding.Default.GetString(b, 74, 6),
                Auth = Encoding.Default.GetString(b, 80, 6),
                Batch = Encoding.Default.GetString(b, 86, 6),
                Ref = Encoding.Default.GetString(b, 92, 12),
                //BankMessage = Encoding.Default.GetString(b, 104, 15),
                //CardType = Encoding.Default.GetString(b, 119, 15),
                // PeopleID = Encoding.Default.GetString(b, 134, 20),
                // AddiData = Encoding.Default.GetString(b, 154, 50),
                RespCode = Encoding.Default.GetString(b, 204, 2),
                Memo = Encoding.Default.GetString(b, 206, 200)
            };
        }
        private string CreateBankResMessage(string resCode)
        {
            switch (resCode)
            {
                case "55": return "密码错误";
                case "01": return "查发卡方";
                case "06": return "出错";
                case "09": return "请求正在处理中";
                case "12": return "无效交易";
                case "13": return "无效金额";
                case "14": return "无效卡号";
                case "15": return "无此发卡方";
                case "33": return "过期的卡";
                case "36": return "受限制的卡";
                case "39": return "无此信用卡账户";
                case "41": return "挂失卡";
                case "42": return "无此账户";
                case "54": return "过期的卡";
                case "56": return "无此卡记录";
                case "62": return "受限制的卡";
                case "94": return "重复交易";
                case "96": return "系统异常、失效";
                default:
                    return null;
            }
        }
    }
}
