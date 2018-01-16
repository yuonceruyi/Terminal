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
using YuanTu.Core.Services.LightBar;
using Microsoft.Practices.Unity;
using YuanTu.Devices.CardReader;
using YuanTu.Devices;

namespace YuanTu.QDKouQiangYY.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            LightBarService?.PowerOn(LightItem.银行卡);
            base.OnEntered(navigationContext);
        }
        public override void OnSet()
        {
            base.OnSet();

            if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付)
            {
                //todo 动画资源
                BackUri = ResourceEngine.GetImageResourceUri("扫银行卡");
            }
            else if (ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
            {
                //todo 动画资源
                BackUri = ResourceEngine.GetImageResourceUri("苹果支付");
            }
            else
            {
                BackUri = ResourceEngine.GetImageResourceUri("插卡口");
            }
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

            var misposDllPath = string.Empty;
            misposDllPath = GetMisposDLLMethod();

            var ret = MisposUnionService.Initialize(ExtraPaymentModel.CurrentBusiness, misposDllPath, banCardMediaType);
            if (!ret.IsSuccess)
            {
                ShowAlert(false, "银联异常", ret.Message);
            }
            Logger.POS.Info($"银联支付，初始化银联参数，结果:{ret.IsSuccess} 内容:{ret.Message}");
            return ret.IsSuccess;
        }

        private string GetMisposDLLMethod()
        {
            string misposDllPath;
            if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.充值)
            {
                if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 || ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
                {
                    misposDllPath = "UMSIPSSF\\umsapi.dll";
                }
                else
                {
                    misposDllPath = "UMSIPS\\umsapi.dll";
                }
            }
            else
            {
                if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联闪付 || ExtraPaymentModel.CurrentPayMethod == PayMethod.苹果支付)
                {
                    misposDllPath = "MISPOSSF\\umsapi.dll";
                }
                else
                {
                    misposDllPath = "MISPOS\\umsapi.dll";
                }
            }

            return misposDllPath;
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

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
        /// <summary>
        /// 本函数重写目的：银联支付后关闭读卡器
        /// 问题：银联卡支付→退出银联支付页面→就诊卡插入A6→吞卡
        /// 银联已明确问题，银联解决后，该处理可以注释掉
        /// </summary>
        /// <param name="reason"></param>
        protected override void CloseDevices(string reason)
        {
            if (FrameworkConst.VirtualThridPay)
            {
                return;
            }
            MisposUnionService.DisConnect(reason);
            if (ExtraPaymentModel.Complete || _mustClose)
            {
                if (_hasExit)
                {
                    return;
                }
                _hasExit = true;
                MisposUnionService.UnInitialize(reason);

                Task.Run(() =>
                {
                    try
                    {
                        var reader = GetInstance<IRFCardReader[]>().FirstOrDefault(p => p.DeviceId == "Act_A6_RF");
                        if (reader != null)
                        {
                            if (reader.Connect().IsSuccess)
                            {
                                reader.UnInitialize();
                                reader.DisConnect();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error($"[银联支付后设置不允许进卡]发生异常,{ex.Message} {ex.StackTrace}");

                    }
                });

                CloseA6HuaDa2();

            }
        }

        /// <summary>
        /// 740机器，银联支付后关闭读卡器
        /// 问题：银联卡支付→退出银联支付页面→就诊卡插入A6→吞卡
        /// </summary>
        protected virtual void CloseA6HuaDa2()
        {
            var config = GetInstance<IConfigurationManager>();
            var _isA6HuaDa = (config.GetValue("A6_HuaDa") ?? "0") == "1";

            if (_isA6HuaDa)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var reader = GetInstance<IRFCardReader[]>().FirstOrDefault(p => p.DeviceId == "Act_A6_RF2");
                        if (reader != null)
                        {
                            if (reader.Connect().IsSuccess)
                            {
                                reader.UnInitialize();
                                reader.DisConnect();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Main.Error($"[银联支付后设置不允许进卡]发生异常,{ex.Message} {ex.StackTrace}");

                    }
                });
                //Task.Run(() =>
                //{
                //    try
                //    {
                //        var _port = config.GetValueInt("A6_HuaDa2Config:Port");
                //        var _baud = config.GetValueInt("A6_HuaDa2Config:Baud");

                //        var ret = UnSafeMethods.A6_Connect(_port, _baud, ref _handle);
                //        if (ret != 0)
                //        {
                //            Logger.Device.Error($"[读卡器[A6_HuaDa2]连接异常，返回码:{ret} 端口:{_port} 波特率:{_baud}");
                //            return;
                //        }
                //        UnSafeMethods.A6_LedOff(_handle);
                //        //强制退卡
                //        ret = UnSafeMethods.A6_MoveCard(_handle, 0x30);
                //        if (ret != 0)
                //        {
                //            Logger.Device.Error($"[读卡器[A6_HuaDa2]退卡失败，返回码:{ret}");
                //            return;
                //        }
                //        //设置不允许进卡
                //        ret = UnSafeMethods.A6_SetCardIn(_handle, 0x31, 0x31);
                //        if (ret != 0)
                //        {
                //            Logger.Device.Error($"[读卡器[A6_HuaDa2]禁止入卡失败，返回码:{ret}");
                //            return;
                //        }
                //        //断开链接
                //        ret = UnSafeMethods.A6_Disconnect(_handle);
                //        if (ret != 0)
                //        {
                //            Logger.Device.Error($"[读卡器[A6_HuaDa2]连接断开异常，返回码:{ret} 端口:{_port} 波特率:{_baud} 句柄:{_handle}");
                //            return;
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        Logger.Main.Error($"[银联支付后设置不允许进卡]发生异常,{ex.Message} {ex.StackTrace}");

                //    }
                //});

            }
        }
    }
}
