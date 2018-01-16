using System;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using System.Linq;
using YuanTu.Consts.Models.Payment;
using YuanTu.Core.Extension;
using System.Collections.Generic;
using YuanTu.Consts.Models.BillPay;
using Microsoft.Practices.Unity;
using System.Diagnostics;
using YuanTu.Consts.Models.Register;
using Microsoft.Practices.ServiceLocation;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.HuNanHangTianHospital.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        protected override void Do(ChoiceButtonInfo param)
        {
            var configurationManager = ServiceLocator.Current.GetInstance<IConfigurationManager>();
            var startTimeConfig = configurationManager.GetValue("停诊时间:StartTime");
            var endTimeConfig = configurationManager.GetValue("停诊时间:EndTime");
            if (string.IsNullOrEmpty(startTimeConfig) || string.IsNullOrEmpty(endTimeConfig))
            {
                ShowAlert(false, "停诊时间未设置", "停诊时间未设置");
                return;
            }
            var startTime = DateTime.Parse(startTimeConfig);
            var endTime = DateTime.Parse(endTimeConfig);
            if ((DateTimeCore.Now > startTime) && (DateTimeCore.Now < endTime))
            {
                ShowAlert(false, "停诊时间", $"您好，{startTimeConfig}到{endTimeConfig}医院处于停诊时间。");
                return;
            }
            Result result;
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    var config = GetInstance<IConfigurationManager>();
                    engine.JumpAfterFlow(
                        config.GetValue("SelectCreateType") == "1"
                            ? new FormContext(A.ChaKa_Context, A.CK.Select)
                            : new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.JianDang_Context, AInner.JD.Confirm), param.Name);
                    break;

                case Business.挂号:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Dept), param.Name);
                    break;

                case Business.预约:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                    result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                    if (!result.IsSuccess)
                    {
                        ShowAlert(false, "打印机检测", result.Message);
                        return;
                    }
                    OnInRecharge(param);
                    break;
                case Business.补打:
                    Navigate(A.PrintAgainChoice);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        protected override Task<Result<FormContext>> TakeNumJump()
        {
            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 预约取号");
            var patientModel = GetInstance<IPatientModel>();
            var recordModel = GetInstance<IAppoRecordModel>();
            var cardModel = GetInstance<ICardModel>();
            var takeNumModel = GetInstance<ITakeNumModel>();
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询预约记录，请稍候...");
                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                {
                    patientId = patientModel.当前病人信息?.patientId,
                    patientName = patientModel.当前病人信息?.name,
                    startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    searchType = "1",
                    status = "1",
                    cardNo = cardModel.CardNo,
                    cardType = ((int)cardModel.CardType).ToString()
                };
                recordModel.Res挂号预约记录查询 = DataHandlerEx.挂号预约记录查询(recordModel.Req挂号预约记录查询);
                if (recordModel.Res挂号预约记录查询?.success ?? false)
                {
                    if (recordModel.Res挂号预约记录查询?.data?.Count > 1)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    if (recordModel.Res挂号预约记录查询?.data?.Count == 1)
                    {
                        recordModel.所选记录 = recordModel.Res挂号预约记录查询.data.FirstOrDefault();
                        var record = recordModel.所选记录;

                        takeNumModel.List = new List<PayInfoItem>
                        {
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd","yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return
                            Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> BillPayJump()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询待缴费信息，请稍候...");
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
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息");
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {

            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 现场挂号");
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询科室信息，请稍候...");
                return 科室排班信息查询();
            });
        }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IChoiceModel ChoiceModel { get; set; }
        private Result<FormContext> 缴费预结算()
        {
            var billRecordModel = GetInstance<IBillRecordModel>();
            var billPrePayModel = GetInstance<IBillPrePayModel>();
            var patientModel = GetInstance<IPatientModel>();
            var cardModel = GetInstance<ICardModel>();

            var req = new req缴费预结算
            {
                patientId = patientModel.当前病人信息.patientId,
                billNo = billRecordModel.Res获取缴费概要信息.data.FirstOrDefault().billNo,//"|"分割拼接billNo
                cardType = ((int)cardModel.CardType).ToString(),
                cardNo = cardModel.CardNo,
            };
            var res = DataHandlerEx.缴费预结算(req);
            billPrePayModel.Res缴费预结算 = res;
            if (res?.success ?? false)
            {
                return Result<FormContext>.Success(default(FormContext));
            }
            ShowAlert(false, "缴费预结算", "缴费预结算失败");
            return Result<FormContext>.Fail("");

        }

        private Result<FormContext> 科室排班信息查询()
        {
            var deptartmentModel = GetInstance<IDeptartmentModel>();
            deptartmentModel.排班科室信息查询 = new req排班科室信息查询
            {
                regMode = "2",
                startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
            };
            deptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(deptartmentModel.排班科室信息查询);
            if (deptartmentModel.Res排班科室信息查询?.success ?? false)
            {
                if (deptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                {
                    return Result<FormContext>.Success(default(FormContext));
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    return Result<FormContext>.Fail("没有获得科室信息(列表为空)");
                }
            }
            else
            {
                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: deptartmentModel.Res排班科室信息查询?.msg);
                return Result<FormContext>.Fail("没有获得科室信息" + deptartmentModel.Res排班科室信息查询?.msg);
            }
        }
    }
}
