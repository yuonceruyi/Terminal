using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models.Print;
using YuanTu.Core.Extension;
using YuanTu.Consts;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.DB;
using YuanTu.Core.Log;
using System.Net;
using YuanTu.Core.Systems;
using YuanTu.Default.Tools;
using YuanTu.Consts.Models;
using YuanTu.Default.Component.Tools.Models;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using YuanTu.Consts.Models.Auth;

namespace YuanTu.WeiHaiZXYY.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {
        //protected override void OnCAClick()
        //{
        //    //Navigate(A.Third.Cash);
        //    ExtraPaymentModel.TotalMoney = 100;
        //    ExtraPaymentModel.FinishFunc();
        //}
        protected override void OnPayButtonClick(Info i)
        {
            var payMethod = (PayMethod)i.Tag;
            OpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.充值;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnRechargeCallback;
            //准备门诊充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.当前病人信息.name,
                PatientId = PatientModel.当前病人信息.patientId,
                IdNo = PatientModel.当前病人信息.idNo,
                GuardianNo = PatientModel.当前病人信息.guardianNo,
                CardNo = CardModel.CardNo,
                CardType = CardModel.CardType,
                Remain = decimal.Parse(PatientModel.当前病人信息.accBalance)
            };

            ChangeNavigationContent(OpRechargeModel.RechargeMethod.ToString());
            switch (payMethod)
            {
                case PayMethod.未知:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    throw new ArgumentOutOfRangeException();
                case PayMethod.现金:
                    var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
                    var startTimeConfig = configurationManager.GetValue("停止充值:StartTime");
                    var endTimeConfig = configurationManager.GetValue("停止充值:EndTime");
                    if (string.IsNullOrEmpty(startTimeConfig) || string.IsNullOrEmpty(endTimeConfig))
                    {
                        ShowAlert(false, "停止充值时间未设置", "停止充值时间未设置");
                        return;
                    }
                    var startTime = DateTime.Parse(startTimeConfig);
                    var endTime = DateTime.Parse(endTimeConfig);
                    if ((DateTimeCore.Now > startTime) && (DateTimeCore.Now < endTime))
                    {
                        ShowAlert(false, "停止充值时间", $"您好，{startTimeConfig}到{endTimeConfig}医院处于停止充值时间。");
                        return;
                    }
                    OnCAClick();
                    break;
                    
                case PayMethod.银联:
                case PayMethod.支付宝:
                case PayMethod.微信支付:
                case PayMethod.苹果支付:
                    Navigate(A.CZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    Logger.Main.Info("开始充值");
                    p.ChangeText("正在进行充值，请稍候...");
                    OpRechargeModel.Res预缴金充值 = null;
                    var patientInfo = PatientModel.当前病人信息;
                    var posinfo = ExtraPaymentModel?.PaymentResult as TransResDto;
                    OpRechargeModel.Req预缴金充值 = new req预缴金充值
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        cash = ((ExtraPaymentModel.TotalMoney) / 100).ToString("F2"),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = NetworkManager.IP,
                        name = patientInfo.name,
                    };
                    Logger.Main.Info("构造Req预缴金充值1成功");
                    OpRechargeModel.Req预缴金充值.extend = $"{DateTimeCore.Now.ToString("yyyyMMdd")},{posinfo?.Ref ?? OpRechargeModel?.Req预缴金充值?.flowId},{posinfo?.CardNo}";
                    Logger.Main.Info("构造Req预缴金充值2成功");
                    //填充各种支付方式附加数据
                    FillRechargeRequest(OpRechargeModel.Req预缴金充值);
                    Logger.Main.Info("构造Req预缴金充值3成功");
                    OpRechargeModel.Res预缴金充值 = DataHandlerEx.预缴金充值(OpRechargeModel.Req预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (OpRechargeModel.Res预缴金充值.success)
                    {
                        Logger.Main.Info("充值成功");
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true),
                            TipImage = "提示_凭条"
                        });

                        patientInfo.accountNo = (decimal.Parse(patientInfo.accountNo) + decimal.Parse(OpRechargeModel.Req预缴金充值.cash)).ToString();
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context,
                            $"{patientInfo.name}\r\n余额{patientInfo.accountNo}");
                        Navigate(A.CZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(false),
                            TipImage = "提示_凭条",
                            DebugInfo = OpRechargeModel.Res预缴金充值?.msg
                        });
                        Navigate(A.CZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(OpRechargeModel.Res预缴金充值?.code ?? -100, OpRechargeModel.Res预缴金充值.msg);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Main.Error(
                        $"[{ExtraPaymentModel.CurrentPayMethod}充值]发起存钱交易时出现异常，原因:{ex.Message}\r\n{ex.StackTrace}\r\n{ex.InnerException}");
                    ShowAlert(false, "充值失败", "发生系统异常 ");
                    return Result.Fail("系统异常");
                }
                finally
                {
                    DBManager.Insert(new RechargeInfo
                    {
                        CardNo = CardModel?.CardNo,
                        PatientId = PatientModel?.当前病人信息?.patientId,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = OpRechargeModel.Res预缴金充值?.success ?? false,
                        ErrorMsg = OpRechargeModel.Res预缴金充值?.msg
                    });
                }
            });
        }

        public override void DoubleClick()
        {
            var ret = InputTextView.ShowDialogView("测试：请输入 充值金额");
            if (!ret.IsSuccess)
                return;
            var list = ret.Value;
            ExtraPaymentModel.TotalMoney = decimal.Parse(list) * 100;
            ExtraPaymentModel.PaymentResult = new TransResDto();
            OnRechargeCallback();
        }


        protected override Queue<IPrintable> OpRechargePrintables(bool success)
        {
            if (!success)
            {
                if (ExtraPaymentModel.CurrentPayMethod != PayMethod.现金) //只有现金才需要打凭条
                {
                    return null;
                }
            }
            var queue = PrintManager.NewQueue("");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"-------------充值{(success ? "成功" : "失败")}-------------\n");
            sb.Append($"就诊卡号:{CardModel?.CardNo}\n");
            sb.Append($"门诊号码:\n");
            sb.Append($"{patientInfo.patientId}\n");
            sb.Append($"姓    名:{patientInfo.name}\n");
            sb.Append($"收费日期:{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"本次充值:{double.Parse(OpRechargeModel.Req预缴金充值.cash).ToString("F2")}元\n");
            if (success)
            {
                sb.Append($"卡内余额:{(decimal.Parse(patientInfo.accountNo) + decimal.Parse(OpRechargeModel.Req预缴金充值.cash)).ToString("F2")}元\n");
            }
            else
            {
                sb.Append($"异常原因:{OpRechargeModel.Res预缴金充值.msg}\n");
            }
            sb.Append($"支付方式:{ExtraPaymentModel.CurrentPayMethod}\n");
            if (ExtraPaymentModel.CurrentPayMethod == PayMethod.银联)
            {
                var extraPaymentModel = GetInstance<IExtraPaymentModel>();
                var pos = extraPaymentModel.PaymentResult as TransResDto;
                sb.Append($"银行卡号码:{pos?.CardNo}\n");
                sb.Append($"银行流水号:{pos?.Trace}\n");
                sb.Append($"银行参考号:{pos?.Ref}\n");
            }
            sb.Append($"-------------------------------------\n");
            sb.Append($"打印时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"设备编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请妥善保存您的凭条\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}
