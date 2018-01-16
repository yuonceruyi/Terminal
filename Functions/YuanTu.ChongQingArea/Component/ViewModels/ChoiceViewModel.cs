using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.ChongQingArea.Component.ViewModels
{
    public class ChoiceViewModel : Default.Clinic.Component.ViewModels.ChoiceViewModel
    {
        protected override Result CheckReceiptPrinter()
        {
            if(PrintConfig.CurrentStrategyType() == DeviceType.Clinic)
                return Result.Success();
            var choiceModel = GetInstance<IChoiceModel>();
            switch (choiceModel.Business)
            {
                case Business.建档:
                case Business.挂号:
                case Business.预约:
                case Business.取号:
                case Business.缴费:
                case Business.充值:
                case Business.住院押金:
                case Business.住院押金查询:
                case Business.出院结算:
                case Business.检验结果:
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }

        protected override void OnInRecharge(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            //choiceModel.HasAuthFlow = false;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice), IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.RechargeWay), param.Name);
        }


        protected override void Do(ChoiceButtonInfo param)
        {

            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            var result = CheckReceiptPrinter();
            if (!result.IsSuccess)
            {
                ShowAlert(false, "打印机检测", result.Message);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.出院结算:
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice),
                        CYJSJump,
                        new FormContext(B.ChuYuanJieSuan_Context, B.CY.SelectSi), param.Name);
                    break;
                case Business.补卡:

                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            ReSendCardJump,
                            new FormContext(B.BuKa_Context, B.BK.Confirm), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                            ReSendCardJump,
                            new FormContext(B.BuKa_Context, B.BK.Confirm), param.Name);
                    break;

                case Business.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        DiagJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                default:
                    base.Do(param);
                    break;
            }
           
        }

        protected virtual Task<Result<FormContext>> CYJSJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 出院结算");
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }
        protected virtual Task<Result<FormContext>> ReSendCardJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 补卡");
                Thread.Sleep(100);
                return Result<FormContext>.Success(null);
            });
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 预约取号");
                var patientModel = GetInstance<IPatientModel>();
                var recordModel = GetInstance<IAppoRecordModel>();
                var cardModel = GetInstance<ICardModel>();
                var takeNumModel = GetInstance<ITakeNumModel>();
                lp.ChangeText("正在查询预约记录，请稍候...");
                recordModel.Req挂号预约记录查询 = new req挂号预约记录查询
                {
                    patientId = patientModel.当前病人信息?.patientId,
                    patientName = patientModel.当前病人信息?.name,
                    startDate = DateTimeCore.Now.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Now.AddDays(7).ToString("yyyy-MM-dd"),
                    searchType = "1",
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
                            new PayInfoItem("就诊日期：", record.medDate.SafeConvertToDate("yyyy-MM-dd", "yyyy年MM月dd日")),
                            new PayInfoItem("就诊科室：", record.deptName),
                            new PayInfoItem("就诊医生：", record.doctName),
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()+"-"+ record.medTime),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "没有获得预约记录信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected virtual Task<Result<FormContext>> DiagJump()
        {
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询检验报告信息，请稍候...");
                var reportModel = GetInstance<IDiagReportModel>();
                var cardModel = GetInstance<ICardModel>();
                var patientModel = GetInstance<IPatientModel>();
                reportModel.Req检验基本信息查询 = new req检验基本信息查询
                {
                    cardNo = cardModel.CardNo,
                    cardType = ((int)cardModel.CardType).ToString(),
                    patientId = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].patientId,
                    patientName = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].name,
                    type = "1" //门诊
                };
                reportModel.Res检验基本信息查询 = DataHandlerEx.检验基本信息查询(reportModel.Req检验基本信息查询);
                if (reportModel.Res检验基本信息查询?.success ?? false)
                {
                    if (reportModel.Res检验基本信息查询?.data?.Count > 0)
                    {
                        return Result<FormContext>.Success(new FormContext(A.DiagReportQuery, A.JYJL.DiagReport));
                    }
                    ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "检验报告信息查询", "没有获得检验报告信息");
                return Result<FormContext>.Fail("");
            });
        }
    }
}