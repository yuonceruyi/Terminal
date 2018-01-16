using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Log;
using YuanTu.Devices.UnionPay;

namespace YuanTu.BJJingDuETYY.Clinic.Component.Tools.ViewModels
{
    /// <summary>
    /// 继承自Clinic，逻辑copy自YuanTu.BJJingDuETYY.Component.Tools.ViewModels.PosViewModel
    /// @2017-04-09
    /// </summary>
    public class PosViewModel : Default.Clinic.Component.Tools.ViewModels.PosViewModel
    {
        public override void OnSet()
        {
            base.OnSet();
            BackUri = ResourceEngine.GetImageResourceUri("插银行卡");
        }

        protected override bool InitializePos()
        {
            if (FrameworkConst.VirtualThridPay)
            {
                return true;
            }
            var banCardMediaType = BanCardMediaType.磁条 | BanCardMediaType.IC芯片;

            if (CurrentStrategyType() == DeviceType.Clinic ||
                ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 ||
                ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
            {
                banCardMediaType = BanCardMediaType.闪付;
            }

            var ret = MisposUnionService.Initialize(ExtraPaymentModel.CurrentBusiness, (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.充值) ? "UMSIPS\\umsapi.dll" : "MISPOS\\umsapi.dll", banCardMediaType);
            if (!ret.IsSuccess)
            {
                ShowAlert(false, "银联异常", ret.Message);

            }
            Logger.POS.Info($"银联支付，初始化银联参数，结果:{ret.IsSuccess} 内容:{ret.Message}");
            return ret.IsSuccess;
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
                        TransDate = "0815",
                        TransTime = "094117",
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
                                    PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), RefundFailPrintables(payRet?.Result.Message, refundRet.Message));
                                    ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                }
                                else
                                {
                                    ShowAlert(false, "扣费失败", $"{payRet?.Result.Message}");
                                }
                                TryPreview();
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