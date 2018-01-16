using System;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.QDKFQRM.Component.ViewModels
{
    public class ChoiceViewModel : QDKouQiangYY.Component.ViewModels.ChoiceViewModel
    {
        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            var patientModel = GetInstance<IPatientModel>();
            return DoCommand(lp =>
            {
                if (patientModel.住院患者信息.status == "在院")
                {
                    return Result<FormContext>.Success(default(FormContext));
                }
                ShowAlert(false, "住院缴押金", "已出院患者不能缴押金");
                return Result<FormContext>.Fail("");
            });
        }

        protected override void OnInRecharge(ChoiceButtonInfo param)
        {
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
                this.IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.PayerName), param.Name);
        }

        protected override void Do(ChoiceButtonInfo param)
        {
            Result result;
            var choiceModel = GetInstance<IChoiceModel>();

            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;

                case Business.挂号:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
                    OnInRecharge(param);
                    break;

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}