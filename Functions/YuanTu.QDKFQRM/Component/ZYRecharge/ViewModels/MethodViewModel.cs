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

namespace YuanTu.QDKFQRM.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.QDKouQiangYY.Component.ZYRecharge.ViewModels.MethodViewModel
    {
        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    IpRechargeModel.Res住院预缴金充值 = null;
                    var patientInfo = PatientModel.住院患者信息;
                    IpRechargeModel.Req住院预缴金充值.cash = ExtraPaymentModel.TotalMoney.ToString();
                    IpRechargeModel.Req住院预缴金充值.tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription();                                         

                    //填充各种支付方式附加数据
                    FillRechargeRequest(IpRechargeModel.Req住院预缴金充值);

                    IpRechargeModel.Res住院预缴金充值 = DataHandlerEx.住院预缴金充值(IpRechargeModel.Req住院预缴金充值);
                    ExtraPaymentModel.Complete = true;
                    if (IpRechargeModel.Res住院预缴金充值.success)
                    {
                        ExtraPaymentModel.Complete = true;
                        //PrintModel.SetPrintInfo(true, "缴住院押金成功",
                        //    $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(true));
                        //PrintModel.SetPrintImage(ResourceEngine.GetImageResourceUri("提示_凭条"));
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(true);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {                                
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",                              
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(true),
                                TipImage = "提示_凭条"
                            });
                        }
                        patientInfo.accBalance = IpRechargeModel.Res住院预缴金充值.data.cash;
                        ChangeNavigationContent(A.ZY.InPatientInfo, A.ZhuYuan_Context,
                            $"{patientInfo.name}\r\n余额{patientInfo.accBalance.In元()}");
                        Navigate(A.ZYCZ.Print);
                        return Result.Success();
                    }
                    else
                    {
                        //PrintModel.SetPrintInfo(false, "充值失败",
                        //    $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                        //    ConfigurationManager.GetValue("Printer:Receipt"), IpRechargePrintables(false),
                        //    IpRechargeModel.Res住院预缴金充值?.msg);
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(false);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                TipImage = "提示_凭条"
                            });
                        }
                        else
                        {
                            PrintModel.SetPrintInfo(false, new PrintInfo
                            {
                                TypeMsg = "充值失败",
                                TipMsg = $"您于{DateTimeCore.Now.ToString("HH:mm")}分充值{ExtraPaymentModel.TotalMoney.In元()}失败",
                                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                                Printables = IpRechargePrintables(false),
                                TipImage = "提示_凭条",
                                DebugInfo = IpRechargeModel.Res住院预缴金充值?.msg
                            });
                        }
                        Navigate(A.ZYCZ.Print);
                        ExtraPaymentModel.Complete = true;
                        return Result.Fail(IpRechargeModel.Res住院预缴金充值?.code??-100, IpRechargeModel.Res住院预缴金充值.msg);
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
                    DBManager.Insert(new ZYRechargeInfo
                    {
                        PatientId = PatientModel?.当前病人信息?.cardNo,
                        RechargeMethod = ExtraPaymentModel.CurrentPayMethod,
                        TotalMoney = ExtraPaymentModel.TotalMoney,
                        Success = IpRechargeModel.Res住院预缴金充值?.success ?? false,
                        ErrorMsg = IpRechargeModel.Res住院预缴金充值?.msg
                    });
                }
            });
        }
    }
}