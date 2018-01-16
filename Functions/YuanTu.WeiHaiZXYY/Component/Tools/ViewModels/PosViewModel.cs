using Microsoft.Practices.Unity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Gateway;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Devices.UnionPay;

namespace YuanTu.WeiHaiZXYY.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {

        [Dependency]
        public IPatientModel PatientModel { get; set; }
        [Dependency]
        public ICardModel CardModel { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            _hasExit = false;
            _mustClose = false;

            var patientInfo = ExtraPaymentModel.PatientInfo;
            Name = patientInfo.Name;
            CardNo = patientInfo.CardNo;
            Remain = patientInfo.Remain.In元();
            if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金)
            {
                var info = PatientModel.住院患者信息;
                Name = info.name;
                CardNo = info.patientHosId;
                Remain = decimal.Parse(info.accBalance).In元();
            }
            else
            {
                var info = PatientModel.当前病人信息;
                Name = info.name;
                CardNo = CardModel.CardNo;
                Remain = info.accountNo + "元";
            }
            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            BankPassword = "";
            Amount = ExtraPaymentModel.TotalMoney;
            ShowKeyboardAnimation = false;
            ShowInputPassWord = false;
            StartPosFlow();
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
                        Logger.Main.Info("模拟返回不为null");
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
                            Logger.Main.Info("模拟返回不为code:" + code);
                            if (DataHandler.UnKnowErrorCode.Contains(code))
                            {
                                var errorMsg = $"银联消费成功，网关返回未知结果{code}，打印凭条结束交易！\n请执凭条到人工咨询此交易结果！";
                                PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), GatewayUnknowErrorPrintables(errorMsg));
                                ShowAlert(false, "业务处理异常", errorMsg);
                                CloseDevices(errorMsg);
                                Navigate(A.Home);
                            }
                            else
                            {
                                Logger.Main.Info("开始冲正:" + code);
                                var refundRet = MisposUnionService.Refund(payRet?.Result.Message);
                                Logger.Main.Info("冲正结束:" + refundRet.IsSuccess.ToString());
                                    if (!refundRet.IsSuccess)
                                    {
                                        PrintManager.QuickPrint(ConfigurationManager.GetValue("Printer:Receipt"), RefundFailPrintables(payRet?.Result.Message, refundRet.Message));
                                        ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                                    }
                                    else
                                    {
                                        ShowAlert(false, "扣费失败", "交易失败！\n" + payRet?.Result.Message);
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

        protected override void CloseDevices(string reason)
        {
            Logger.Main.Info("释放DLL");
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
            }
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            if (MisposUnionService.IsBusy)
            {
                return false;
            }
            _mustClose = true;
            CloseDevices("取消操作");
            return true;
        }
    }
}