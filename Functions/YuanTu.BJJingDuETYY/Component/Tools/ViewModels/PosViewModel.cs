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
using Microsoft.Practices.Unity;
using YuanTu.Core.Services.LightBar;
using YuanTu.Devices.CardReader;
using YuanTu.BJArea.Services.BankPosBOC;
using YuanTu.Core.Extension;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Core.DB;

namespace YuanTu.BJJingDuETYY.Component.Tools.ViewModels
{
    public class PosViewModel : YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        [Dependency]
        public ILightBarService LightBarService { get; set; }
        [Dependency]
        public IBankPosBOC BankPosBOC { get; set; }
        public string TerminalNo { get; set; }
        public string UserId { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            LightBarService?.PowerOn(LightItem.银行卡);
            var config = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            TerminalNo= config.GetValue($"BankPos:TerminalNo");
            UserId = config.GetValue($"BankPos:UserId");
            base.OnEntered(navigationContext);
        }
        protected override void StartPosFlow()
        {
            DoCommand(p =>
            {
                 BankPosStr r= BankPosBOC.DoSale(Amount, TerminalNo, UserId);
                return r;
            }).ContinueWith(AfterDoSale);
        }
        private void AfterDoSale(Task<BankPosStr> ret)
        {
            var doSaleResult = ret.Result;
            if (!doSaleResult)
            {
                ShowAlert(false, "扣费失败", $"{doSaleResult.ResultMsg}");
                CloseDevices("扣费失败");
                TryPreview();
                return;
            }
            //上送交易信息
            var rechargeb = new[] { Consts.Enums.Business.充值, Consts.Enums.Business.住院押金 };
            var isrecharge = rechargeb.Contains(ExtraPaymentModel.CurrentBusiness);
            var printerName = ConfigurationManager.GetValue("Printer:Receipt");

            TradeUploadService.UploadAsync(new TradeInfo
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
                TradeId = doSaleResult.CenterSeq,
                OriginTradeId = "",
                AccountNo = doSaleResult.BankNo,
                Amount = doSaleResult.Amt,
                TradeDetail = doSaleResult.ToJsonString()
            });
            ExtraPaymentModel.PaymentResult = doSaleResult;
            var task = ExtraPaymentModel.FinishFunc?.Invoke();
            if (task == null)
            {
                PrintManager.QuickPrint(printerName, BusinessFailPrintables());
                ShowAlert(false, "扣费失败", "交易失败，请重试！");
                CloseDevices("系统异常，交易操作没有任何返回！");
                TryPreview();
                return;
            }
            task.ContinueWith(payRet =>
            {
                var payResult = payRet.Result;
                if (payResult.IsSuccess)
                {
                    CloseDevices("消费结束");
                    return;
                }
                if (FrameworkConst.VirtualThridPay)
                {
                    ShowAlert(false, "扣费失败", "交易失败，请重试！" + payResult.Message);
                    return;
                }
                var code = payResult.ResultCode;
                if (DataHandler.UnKnowErrorCode.Contains(code))
                {
                    var errorMsg = $"{payResult.Message.BackNotNullOrEmpty("银联消费成功，网关返回未知结果")}\r\n交易返回码:{code}\r\n请执凭条到人工服务台咨询此交易结果！";
                    PrintManager.QuickPrint(printerName, GatewayUnknowErrorPrintables(errorMsg));
                    ShowAlert(false, "业务处理异常", errorMsg);
                    CloseDevices(errorMsg);
                    Navigate(A.Home);
                    return;
                }

                BankPosStr r = BankPosBOC.DoReverse();
                //var refundResult = MisposUnionService.Refund(payResult.Message);
                if (!r)
                {
                    PrintManager.QuickPrint(printerName, RefundFailPrintables(payResult.Message, r.ResultMsg));
                    ShowAlert(false, "扣费失败", "银联冲正失败，请持凭条与医院工作人员联系！\n请尝试其他支付方式！");
                }
                else
                {
                    //上送交易信息
                    TradeUploadService.UploadAsync(new TradeInfo
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
                        TradeId = doSaleResult.CenterSeq,
                        OriginTradeId = r.CenterSeq,
                        AccountNo = doSaleResult.BankNo,
                        Amount = doSaleResult.Amt,
                        TradeDetail = r.GetBankPosStr()
                    });
                    ShowAlert(false, "扣费失败", "交易失败！\n" + payResult.Message);
                }
                CloseDevices("消费结束");
                TryPreview();
            });
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
        
        public override bool OnLeaving(NavigationContext navigationContext)
        {
            LightBarService?.PowerOff();
            return base.OnLeaving(navigationContext);
        }
        
    }
}
