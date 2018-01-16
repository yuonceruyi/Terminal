using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Log;
using YuanTu.XiaoShanArea.MisPos.MisPosSp30;

namespace YuanTu.XiaoShanHealthStation.Component.Tools.ViewModels
{
    public class PosViewModel:Default.Component.Tools.ViewModels.PosViewModel
    {
        private Response _posOutSp30;
        protected override void StartPosFlow()
        {
            Tips = "请按提示完成进行操作...";
            if (FrameworkConst.VirtualThridPay)
                return;
            var block = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 20,
                Text = "输入完６位密码后，请按键盘上'输入'键",
                Foreground = new SolidColorBrush(Colors.Red)
            };

            DoCommand(p =>
            {
              
               
                p.ChangeMutiText(block);
                //todo 银联扣费
              
                
                try
                {
                    var result = MisPosSp30.DoSale(ExtraPaymentModel.TotalMoney);
                    if (!result.IsSuccess)
                    {
                        Logger.POS.Debug($"银联消费异常，{result.Message}");
                        ShowAlert(false, "银联扣费异常", $"{result.Message}\n请重试！");
                        MisPosSp30.DoLogon();
                        TryPreview();
                        return Result<TransResDto>.Fail("扣费失败");
                    }
                    _posOutSp30 = result.Value;
                    
                    if (!_posOutSp30.成功)
                    {
                        ShowAlert(false, "银联扣费失败", $"{_posOutSp30.错误信息}");
                        TryPreview();
                        return Result<TransResDto>.Fail("扣费失败");
                    }
                }
                catch (Exception ex)
                {
                    Logger.POS.Debug($"银联消费异常，{ex.Message} {ex.StackTrace}");
                    ShowAlert(false, "银联扣费异常", $"{ex.Message}\n请重试！");
                    MisPosSp30.DoLogon();
                    TryPreview();
                    return Result<TransResDto>.Fail("扣费失败");
                }
                Logger.POS.Debug($"银联消费成功...");
                //todo 返回值转换
                ExtraPaymentModel.PaymentResult = OutPut2TransRes(_posOutSp30);
                if (ExtraPaymentModel.PaymentResult == null)
                {
                    ShowAlert(false, "扣费异常", $"银联消费返回内容异常");
                    return Result<TransResDto>.Fail("扣费失败");
                }
                Logger.POS.Debug($"银联消费成功,返回转换成功...");
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
                                Logger.POS.Debug($"银联消费成功,返回转换成功,医院业务处理失败...");
                                if (FrameworkConst.VirtualThridPay)
                                {
                                    ShowAlert(false, "结算失败", "交易失败，请重试！" + payRet?.Result.Message);
                                    return;
                                }
                                _posOutSp30 = MisPosSp30.DoRefund(ExtraPaymentModel.TotalMoney, _posOutSp30.Seq).Value;
                                if (!_posOutSp30.成功)
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
                                    ShowAlert(false, "结算失败", $"交易失败，请重试！\n{payRet?.Result.Message}");
                                }
                                TryPreview();
                            }
                            else
                            {
                                Logger.POS.Debug($"银联消费成功,返回转换成功,医院业务处理成功...");
                            }
                        });
                    }
                    else
                    {
                        Logger.POS.Debug($"银联消费成功,返回转换成功,医院业务处理失败（task为null）...");
                        _posOutSp30 = MisPosSp30.DoRefund(ExtraPaymentModel.TotalMoney, _posOutSp30.Seq).Value;
                        if (!_posOutSp30.成功)
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
                            ShowAlert(false, "结算失败", "交易失败，请重试！\ntask=null");
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
                var outPut = res as Response;
                return new TransResDto
                {
                    RespCode = "00",
                    RespInfo = outPut?.错误信息,
                    CardNo = outPut?.卡号,
                    Amount = outPut?.交易金额,
                    Trace = outPut?.流水号,
                    Batch = outPut?.批次号,
                    TransDate =DateTime.Parse(outPut?.交易时间).ToString("MMdd"),
                    TransTime = DateTime.Parse(outPut?.交易时间).ToString("HHmmss"),
                    Ref = outPut?.系统参考号,
                    Auth = outPut?.授权号,
                    MId = outPut?.商户代码,
                    TId = outPut?.终端号
                };
          
            
           
        }
    }
}
