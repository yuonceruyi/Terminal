using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.GateWayStatus;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;
using YuanTu.Devices.PrinterCheck;


namespace YuanTu.QDJZZXYY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.QDKouQiangYY.Component.ViewModels.ChoiceViewModel
    {
        /// <summary>
        /// 本函数重写目的：医院要求凭条打印机异常不允许业务操作
        /// </summary>
        /// <param name="param"></param>

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
                    if (FrameworkConst.DeviceType != "YT-740")
                    {
                        result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                        if (!result.IsSuccess)
                        {
                            ShowAlert(false, "打印机检测", result.Message);
                            return;
                        }
                    }
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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        protected override Task<Result<FormContext>> BillPayJump()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();
            var lp = GetInstance<LoadingProcesser>();
            lp.ChangeText("正在查询待缴费信息，请稍候...");
            System.Threading.Thread.Sleep(10 * 1000);
            billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
            {
                patientId = patientModel.当前病人信息.patientId,
                cardType = ((int)cardModel.CardType).ToString(),
                cardNo = cardModel.CardNo,
                billType = ""
            };
            billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
            if (billRecordModel.Res获取缴费概要信息?.success ?? false)
            {
                if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                {
                    return 缴费预结算();
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
            ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息");
            return Task.Run(() => Result<FormContext>.Fail(""));
        }
        private Task<Result<FormContext>> 缴费预结算()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var billPrePayModel = GetInstance<IBillPrePayModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();

            var req = new req缴费预结算
            {
                patientId = patientModel.当前病人信息.patientId,
                billNo = string.Join("|", billRecordModel.Res获取缴费概要信息.data.Select(one => $"{one.billNo}")),//"|"分割拼接billNo
                cardType = ((int)cardModel.CardType).ToString(),
                cardNo = cardModel.CardNo,
            };
            var res = DataHandlerEx.缴费预结算(req);
            billPrePayModel.Res缴费预结算 = res;
            if (res?.success ?? false)
            {
                return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
            }
            ShowAlert(false, "缴费预结算", "缴费预结算失败");
            return Task.Run(() => Result<FormContext>.Fail(""));

        }

        protected override Task<Result<FormContext>> RechargeJump()
        {
            var patientModel = GetInstance<IPatientModel>();
            var gateWayStatusModel = GetInstance<IGateWayStatusModel>();

            //临时卡不允许充值
            if (string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.idNo) &&
                string.IsNullOrWhiteSpace(patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex]?.guardianNo))
            {
                ShowAlert(false, "自助充值", "临时卡不允许充值，请到人工窗口补充身份信息；\r\n或直接进行银联或医保缴费");
                return Task.Run(() => Result<FormContext>.Fail(""));
            }

            return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
        }
    }
}
