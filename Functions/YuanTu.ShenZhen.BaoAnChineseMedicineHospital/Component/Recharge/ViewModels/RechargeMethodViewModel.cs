using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos.UnionPos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Core.Log;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.ShenZhenArea.Services;
using YuanTu.Core.Gateway;
using YuanTu.Core.Services.PrintService;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Recharge.ViewModels
{
    public class RechargeMethodViewModel : YuanTu.Default.Component.Recharge.ViewModels.RechargeMethodViewModel
    {

        [Dependency]
        public IAccountingService Account_Service { get; set; }

        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    OpRechargeModel.Res预缴金充值 = null;
                    var patientInfo = PatientModel.当前病人信息;
                    OpRechargeModel.Req预缴金充值 = new req预缴金充值
                    {
                        patientId = patientInfo.patientId,
                        cardType = ((int)CardModel.CardType).ToString(),
                        cardNo = CardModel.CardNo,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString("0"),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.accountNo,
                        name = patientInfo.name
                    };

                    //填充各种支付方式附加数据
                    FillRechargeRequest(OpRechargeModel.Req预缴金充值);

                    OpRechargeModel.Res预缴金充值 = DataHandlerEx.预缴金充值(OpRechargeModel.Req预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (OpRechargeModel.Res预缴金充值.success)
                    {
                        ExtraPaymentModel.Complete = true;
                        PrintModel.SetPrintInfo(true, new PrintInfo
                        {
                            TypeMsg = "充值成功",
                            TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功充值{ExtraPaymentModel.TotalMoney.In元()}",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(true),
                            TipImage = "提示_凭条"
                        });

                        patientInfo.accBalance = OpRechargeModel.Res预缴金充值.data.cash;
                        ChangeNavigationContent(A.CK.Select, A.ChaKa_Context, $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");

                        Account_Service.充值记账(true);
                        Navigate(A.CZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        if (DataHandler.UnKnowErrorCode.Contains(OpRechargeModel.Res预缴金充值.code))  //单边账。。
                        {
                            OpRechargeModel.Res预缴金充值.msg = $"{OpRechargeModel.Res预缴金充值.code} 服务受理异常,充值失败!";
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "充值单边账",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = BillErrSheBaoPayPrintables(),
                                TipImage = "提示_凭条"
                            });

                            ShowAlert(false, "充值结果未知", $"{OpRechargeModel.Res预缴金充值.code} 服务受理异常,充值失败!", 20);
                            Account_Service.充值记账(false, true);
                            Navigate(A.JF.Print);
                            return Result.Fail(OpRechargeModel.Res预缴金充值?.code ?? -100, OpRechargeModel.Res预缴金充值?.msg);
                        }

                        PrintModel.SetPrintInfo(false, new PrintInfo
                        {
                            TypeMsg = "充值失败",
                            TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                            PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                            Printables = OpRechargePrintables(false),
                            TipImage = "提示_凭条",
                            DebugInfo = OpRechargeModel.Res预缴金充值?.msg
                        });
                        Account_Service.充值记账(false);
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
        protected Queue<IPrintable> BillErrSheBaoPayPrintables()
        {
            var queue = PrintManager.NewQueue("门诊充值单边账");
            var patientInfo = PatientModel.Res病人信息查询.data[PatientModel.PatientInfoIndex];
            var sb = new StringBuilder();
            sb.Append($"姓名：{patientInfo.name}\n");
            sb.Append($"门诊号：{patientInfo.patientId}\n");
            sb.Append($"充值方式：{ExtraPaymentModel.CurrentPayMethod}\n");
            sb.Append($"充值前余额：{patientInfo.accBalance.In元()}\n");
            sb.Append($"充值金额：{OpRechargeModel.Req预缴金充值.cash.In元()}\n");
            sb.Append($"异常原因：{OpRechargeModel.Res预缴金充值.msg}\n");
            sb.Append($"交易时间：{DateTimeCore.Now.ToString("yyyy-MM-dd HH:mm:ss")}\n");
            sb.Append($"柜员编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"请凭该凭条找工作人员处理。\n");
            queue.Enqueue(new PrintItemText { Text = sb.ToString() });

            return queue;
        }
    }
}