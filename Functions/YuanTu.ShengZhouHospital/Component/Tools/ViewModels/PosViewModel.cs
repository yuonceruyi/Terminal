using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Devices.CardReader;
using YuanTu.ShengZhouHospital.ICBC;
using YuanTu.ShengZhouHospital.ISO8583;

namespace YuanTu.ShengZhouHospital.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        private bool _running = false;
        private Mispos.Output outPut;
        private int cash;
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            _hasExit = false;
            _mustClose = false;

            var patientInfo = ExtraPaymentModel?.PatientInfo;
            Name = patientInfo?.Name;
            CardNo = patientInfo?.CardNo;
            Remain = patientInfo?.Remain.In元();
            Business = (int)ExtraPaymentModel.CurrentBusiness == 100 ? "体检缴费" : $"{ExtraPaymentModel.CurrentBusiness}"; 
            BankPassword = "";
            Amount = ExtraPaymentModel.TotalMoney;
            ShowKeyboardAnimation = false;
            ShowInputPassWord = false;
            StartPosFlow();
        }
        protected override void StartPosFlow()
        {
            //ShowAlert(false, "测试提示", $"实际本该消费{ExtraPaymentModel?.TotalMoney.In元()}，模拟消费1分");
            //ExtraPaymentModel.TotalMoney = 001;
            //if (ExtraPaymentModel.TotalMoney.ToString().Contains(".00"))
            //{
            //    ExtraPaymentModel.TotalMoney = Decimal.Parse(ExtraPaymentModel.TotalMoney.ToString().Replace(".00", ""));
            //}
            //if (ExtraPaymentModel.TotalMoney.ToString().Contains(".0"))
            //{
            //    ExtraPaymentModel.TotalMoney = Decimal.Parse(ExtraPaymentModel.TotalMoney.ToString().Replace(".0", ""));
            //}
            ExtraPaymentModel.TotalMoney = (int) ExtraPaymentModel.TotalMoney;
            Logger.POS.Debug($"银联交易金额：{ExtraPaymentModel?.TotalMoney}");
            Tips = "请按提示完成进行操作...";
            if (FrameworkConst.VirtualThridPay || FrameworkConst.DoubleClick)
            {
                var pret = new TransResDto
                {
                    RespCode = "00",
                    RespInfo = "交易成功",
                    CardNo = "622319******7113",
                    Amount = ExtraPaymentModel.TotalMoney.ToString("0"),
                    Trace = "011132",
                    Batch = "000021",
                    TransDate = DateTimeCore.Now.ToString("yyyyMMdd"),
                    TransTime = DateTimeCore.Now.ToString("HHmmss"),
                    Ref = DateTimeCore.Now.Ticks.ToString(),//"094117503517",
                    Auth = "",
                    MId = "302053280620002",
                    TId = "00020026",
                    Memo = "",
                    Lrc = ""
                };
                ExtraPaymentModel.PaymentResult = pret;
                ExtraPaymentModel.FinishFunc?.Invoke();
                return;
            }
            var block = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 20,
                Text = "输入完６位密码后，请按键盘上'输入'键",
                Foreground = new SolidColorBrush(Colors.Red)
            };
            var rechargeb = new[] { Consts.Enums.Business.充值, Consts.Enums.Business.住院押金 };
            var isrecharge = rechargeb.Contains(ExtraPaymentModel.CurrentBusiness);
            DoCommand(p =>
            {

                p.ChangeMutiText(block);
                //todo 银联扣费
                //Mispos.Output outPut;
                cash = int.Parse(ExtraPaymentModel.TotalMoney.ToString(CultureInfo.InvariantCulture));
                try
                {
                    if (!Mispos.DoSale(cash, out outPut))
                    {
                        ShowAlert(false, "扣费失败", $"{outPut.错误信息}");
                        TryPreview();
                        return Result<TransResDto>.Fail("扣费失败");
                    }

                }
                catch (Exception ex)
                {
                    Logger.POS.Error($"工行消费异常，{ex.Message} {ex.StackTrace}");
                    ShowAlert(false, "扣费异常", $"{ex.Message}");
                    return Result<TransResDto>.Fail("扣费失败");
                }
                //todo 返回值转换
                var ret = OutPut2TransRes(outPut);
                ExtraPaymentModel.PaymentResult = ret;
                if (ExtraPaymentModel.PaymentResult == null)
                {
                    ShowAlert(false, "扣费异常", $"工行消费返回空");
                    return Result<TransResDto>.Fail("扣费失败");
                }
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
                    TradeId = ret.Ref,
                    OriginTradeId = "",
                    AccountNo = ret.CardNo,
                    Amount = decimal.Parse(ret.Amount),
                    TradeDetail = ret.ToJsonString()
                });
                return Result<TransResDto>.Success((TransResDto)ExtraPaymentModel.PaymentResult);

            }).ContinueWith(ret =>
            {
                if (ret.Result.IsSuccess)
                {
                    //todo his结算
                    var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
                    if (tsk != null)
                    {
                        tsk.ContinueWith(payRet =>
                        {

                            if (!payRet?.Result.IsSuccess ?? false)
                            {
                                if (FrameworkConst.VirtualThridPay)
                                {
                                    ShowAlert(false, "结算失败", "交易失败，请重试！" + payRet?.Result.Message);
                                    return;
                                }

                                if (!Mispos.DoCancel(outPut.检索参考号, cash, out outPut))
                                {
                                    PrintModel.SetPrintInfo(false, new PrintInfo
                                    {
                                        TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                                        TipMsg = $"银联扣费成功，业务处理失败，请持凭条与医院工作人员联系",
                                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                        Printables = BusinessFailPrintables(),
                                        TipImage = "提示_凭条"
                                    });
                                    PrintManager.Print();
                                    ShowAlert(false, "结算失败", "交易失败，请重试！\n银联冲正失败，请持凭条与医院工作人员联系");
                                }
                                else
                                {
                                    var refret = OutPut2TransRes(outPut);
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
                                        TradeId = ret.Result.Value.Ref,
                                        OriginTradeId = refret?.Ref,
                                        AccountNo = refret?.CardNo,
                                        Amount = decimal.Parse(refret?.Amount??"0"),
                                        TradeDetail = refret?.ToJsonString()
                                    });
                                    ShowAlert(false, "结算失败", "交易失败，请重试！\n银联冲正成功！");
                                }
                                TryPreview();
                            }
                            else
                            {
                                Logger.POS.Debug($"工行消费成功,返回转换成功,医院业务处理成功...");
                            }
                        });
                    }
                    else
                    {
                        Logger.POS.Debug($"工行消费成功,返回转换成功,医院业务处理失败（task为null）...");
                        if (!Mispos.DoCancel(outPut.检索参考号, cash, out outPut))
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                                TipMsg = $"银联扣费成功，业务处理失败，请持凭条与医院工作人员联系",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = BusinessFailPrintables(),
                                TipImage = "提示_凭条"
                            });
                            PrintManager.Print();
                            ShowAlert(false, "结算失败", "交易失败，请重试！\n银联冲正失败，请持凭条与医院工作人员联系");
                        }
                        else
                        {
                            ShowAlert(false, "结算失败", "交易失败，请重试！\n银联冲正成功！");
                        }
                        TryPreview();
                    }
                }
                else
                {
                    TryPreview();
                }
            });
        }
        protected virtual TransResDto OutPut2TransRes<T>(T res)
        {
            var outPut = res as Mispos.Output;
            return new TransResDto
            {
                RespCode = outPut?.返回码,
                RespInfo = outPut?.错误信息,
                CardNo = outPut?.卡号,
                Amount = outPut?.交易金额.ToString(),
                Trace =outPut?.终端流水号,
                //Batch = outPut.BatchNo.ToString(),
                Batch = null,
                TransDate = outPut?.交易日期,
                TransTime = outPut?.交易时间,
                Ref = outPut?.检索参考号,
                Auth = null,
                MId = null,
                TId = outPut?.终端编号
            };
        }
    }
}
