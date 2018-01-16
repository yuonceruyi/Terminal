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
using SerialPrinter = YuanTu.Devices.Printer;
using Microsoft.Practices.ServiceLocation;

namespace YuanTu.WeiHaiZXYY.Component.ZYRecharge.ViewModels
{
    public class MethodViewModel : YuanTu.Default.Component.ZYRecharge.ViewModels.MethodViewModel
    {

        protected override void OnPayButtonClick(Info i)
        {
            var payMethod = (PayMethod)i.Tag;
            IpRechargeModel.RechargeMethod = payMethod;
            ExtraPaymentModel.Complete = false;
            ExtraPaymentModel.CurrentBusiness = Business.住院押金;
            ExtraPaymentModel.CurrentPayMethod = payMethod;
            ExtraPaymentModel.FinishFunc = OnRechargeCallback;
            //准备住院充值所需病人信息
            ExtraPaymentModel.PatientInfo = new PatientInfo
            {
                Name = PatientModel.住院患者信息.name,
                PatientId = PatientModel.住院患者信息.patientHosId,
                IdNo = PatientModel.住院患者信息.idNo,
                GuardianNo = PatientModel.住院患者信息.guardianNo,
                CardNo = PatientModel.住院患者信息.patientHosId,
                CardType = CardType.住院号,
                Remain = decimal.Parse(PatientModel.住院患者信息.accBalance ?? "0")
            };

            ChangeNavigationContent(IpRechargeModel.RechargeMethod.ToString());
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
                    Navigate(A.ZYCZ.InputAmount);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnCAClick()
        {
            Navigate(A.Third.JCMCash);
        }

        protected override Task<Result> OnRechargeCallback()
        {
            return DoCommand(p =>
            {
                try
                {
                    p.ChangeText("正在进行充值，请稍候...");
                    IpRechargeModel.Res住院预缴金充值 = null;
                    var patientInfo = PatientModel.住院患者信息;
                    IpRechargeModel.Req住院预缴金充值 = new req住院预缴金充值
                    {
                        patientId = patientInfo.patientHosId,
                        operId = FrameworkConst.OperatorId,
                        cash = ExtraPaymentModel.TotalMoney.ToString(),
                        tradeMode = ExtraPaymentModel.CurrentPayMethod.GetEnumDescription(),
                        accountNo = patientInfo.patientHosId
                    };

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
                        //PrintModel.SetPrintImage(GetInstance<IResourceEngine>().GetImageResourceUri("提示_凭条"));
                        if (FrameworkConst.DeviceType == "YT-740")
                        {
                            SerialPrint(true);
                            PrintModel.SetPrintInfo(true, new PrintInfo
                            {
                                TypeMsg = "缴住院押金成功",
                                TipMsg = $"您已于{DateTimeCore.Now.ToString("HH:mm")}分成功缴押金{ExtraPaymentModel.TotalMoney.In元()}",
                                TipImage = "提示_凭条_740"
                            });
                        }
                        else if (FrameworkConst.DeviceType == "YT-BG350")
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
                        patientInfo.accBalance = (decimal.Parse(patientInfo.accBalance)+ ExtraPaymentModel.TotalMoney).ToString();
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
                        return Result.Fail(IpRechargeModel.Res住院预缴金充值?.code ?? -100, IpRechargeModel.Res住院预缴金充值.msg);
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
                        PatientId = PatientModel?.住院患者信息?.patientHosId,
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