using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.ZheJiangHospital.ICBC;

namespace YuanTu.ZheJiangHospital.Component.Tools.ViewModels
{
    public class PosViewModel : Default.Component.Tools.ViewModels.PosViewModel
    {
        private int _cash;
        private Mispos.Output _outPut;

        public override void OnEntered(NavigationContext navigationContext)
        {
            TimeOut = 300;
            base.OnEntered(navigationContext);
        }

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
                _cash = Convert.ToInt32(ExtraPaymentModel.TotalMoney);
                try
                {
                    if (!Mispos.DoSale(_cash, out _outPut))
                    {
                        ShowAlert(false, "扣费失败", $"{_outPut.错误信息}");
                        return Result.Fail("扣费失败");
                    }
                }
                catch (Exception ex)
                {
                    Logger.POS.Debug($"工行消费异常，{ex.Message} {ex.StackTrace}");
                    ShowAlert(false, "扣费异常", $"{ex.Message}");
                    return Result.Fail("扣费失败");
                }

                Logger.POS.Debug($"工行消费成功...");

                ExtraPaymentModel.PaymentResult = OutPut2TransRes(_outPut);

                Logger.POS.Debug($"工行消费成功,返回转换成功...");
                return Result.Success();
            }).ContinueWith(ret =>
            {
                if (!ret.Result.IsSuccess)
                {
                    TryPreview();
                    return;
                }
                var task = ExtraPaymentModel.FinishFunc?.Invoke();
                if (task == null)
                {
                    Logger.POS.Debug($"工行消费成功,返回转换成功,医院业务处理失败（task为null）...");
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                        TipMsg = $"银联扣费成功，业务处理失败，请持凭条与医院工作人员联系",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BusinessFailPrintables(),
                        TipImage = "提示_凭条"
                    });
                    PrintManager.Print();
                    ShowAlert(false, "结算失败", "交易失败，请持凭条与医院工作人员联系");
                    TryPreview();
                    return;
                }
                task.ContinueWith(payRet =>
                {
                    if (payRet != null && payRet.Result.IsSuccess)
                    {
                        Logger.POS.Debug($"工行消费成功,返回转换成功,医院业务处理成功...");
                        return;
                    }
                    Logger.POS.Debug($"工行消费成功,返回转换成功,医院业务处理失败...");
                    PrintModel.SetPrintInfo(false, new PrintInfo
                    {
                        TypeMsg = $"银联{ExtraPaymentModel.CurrentBusiness}单边帐",
                        TipMsg = $"银联扣费成功，业务处理失败，请持凭条与医院工作人员联系",
                        PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                        Printables = BusinessFailPrintables(),
                        TipImage = "提示_凭条"
                    });
                    PrintManager.Print();
                    ShowAlert(false, "结算失败", "交易失败，请持凭条与医院工作人员联系");
                    TryPreview();
                });
            });
        }
        
        protected TransResDto OutPut2TransRes(Mispos.Output output)
        {
            return new TransResDto
            {
                RespCode = output.返回码,
                RespInfo = output.错误信息,
                CardNo = output.卡号,
                Amount = output.交易金额,
                Trace = output.终端流水号,
                Batch = output.批次号,
                TransDate = output.交易日期,
                TransTime = output.交易时间,
                Ref = output.检索参考号,
                Auth = output.授权号,
                MId = null,
                TId = output.终端编号,
                Receipt = output.Text,
            };
        }

        protected override Queue<IPrintable> BusinessFailPrintables()
        {
            var queue = PrintManager.NewQueue($"银联{ExtraPaymentModel.CurrentBusiness}单边账");
            var sb = new StringBuilder();
            sb.Append($"状态：银联扣费成功，业务处理失败\n");
            sb.Append($"姓名：{ExtraPaymentModel.PatientInfo.Name}\n");
            sb.Append($"卡号：{ExtraPaymentModel.PatientInfo.CardNo}\n");
            sb.Append($"交易类型：{ExtraPaymentModel.CurrentBusiness}\n");
            sb.Append($"交易金额：{ExtraPaymentModel.TotalMoney.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"祝您早日康复！\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });
            return queue;
        }
    }
}