using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;
using YuanTu.Consts.Enums;

namespace YuanTu.QDQLYY.Component.Tools.ViewModels
{
    public class PosViewModel:YuanTu.QDKouQiangYY.Component.Tools.ViewModels.PosViewModel
    {
        public PosViewModel():base()
        {           
        }

        protected override void StartPay()
        {
            DoCommand(p =>
            {
                if (FrameworkConst.VirtualThridPay)
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
                    return Result<TransResDto>.Success(pret);
                }
                else
                {
                    return MisposUnionService.DoSale(ExtraPaymentModel.TotalMoney);
                }
            }).ContinueWith(ret =>
            {
                if (ret.Result.IsSuccess)
                {
                    ExtraPaymentModel.PaymentResult = ret.Result.Value;
                    var tsk = ExtraPaymentModel.FinishFunc?.Invoke();
                    if (tsk != null)
                    {
                        tsk.ContinueWith(payRet =>
                        {
                            if (!payRet?.Result.IsSuccess ?? false)
                            {
                                if (FrameworkConst.VirtualThridPay)
                                {
                                    ShowAlert(false, "扣费失败", "交易失败，请重试！\n" + payRet?.Result.Message);
                                    return;
                                }
                                var code = payRet?.Result.ResultCode ?? 0;
                                if (DataHandler.UnKnowErrorCode.Contains(code))
                                {
                                    var errorMsg = $"银联消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                                    PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), GatewayUnknowErrorPrintables(errorMsg));
                                    ShowAlert(false, "业务处理异常", errorMsg);
                                    CloseDevices(errorMsg);
                                    Navigate(A.Home);
                                }
                                {
                                    if (CurrentStrategyType() == DeviceType.Clinic ||
                                        ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 ||
                                        ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
                                    {
                                        var success = InitializePos();
                                        if (!success)
                                        {
                                            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), RefundFailPrintables(payRet?.Result.Message, "冲正初始化失败"));
                                            ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                        }
                                        var retReq = MisposUnionService.SetReq(TransType.冲正, ExtraPaymentModel.TotalMoney);
                                        if (!retReq.IsSuccess)
                                        {
                                            PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), RefundFailPrintables(payRet?.Result.Message, "冲正设置入参失败"));
                                            ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                        }
                                    }
                                    var refundRet = MisposUnionService.Refund(payRet?.Result.Message);
                                    if (!refundRet.IsSuccess)
                                    {
                                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"),
                                            RefundFailPrintables(payRet?.Result.Message, refundRet.Message));
                                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                    }
                                    else
                                    {
                                        ShowAlert(false, "扣费失败",  $"{payRet?.Result.Message}");
                                    }
                                    TryPreview();
                                }
                            }
                            CloseDevices("消费结束");
                        });
                    }
                    else
                    {
                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), BusinessFailPrintables());
                        ShowAlert(false, "扣费失败", "交易失败，请重试！");
                        CloseDevices("系统异常，交易操作没有任何返回！");
                        TryPreview();
                    }
                }
                else
                {
                    ShowAlert(false, "扣费失败", $"{ret.Result.Message}");
                    CloseDevices("扣费失败");
                    TryPreview();
                }
            });
        }
    }
}
