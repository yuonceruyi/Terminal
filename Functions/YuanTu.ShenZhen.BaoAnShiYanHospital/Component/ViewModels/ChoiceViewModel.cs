using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.CePrinterCheck;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;

namespace YuanTu.ShenZhen.BaoAnShiYanHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        protected override void Do(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            #region
            //DoCommand的测试demo
            //var block = new TextBlock() { TextAlignment = TextAlignment.Center, FontSize = 17 };
            //block.Inlines.Add("您即将为\r\n姓名:");
            //block.Inlines.Add(new TextBlock() { Text = "猪头", Foreground = new SolidColorBrush(Colors.Red), FontWeight = FontWeights.Bold, FontSize = 20 });
            //block.Inlines.Add(" 卡号:");
            //block.Inlines.Add(new TextBlock() { Text = "888888", Foreground = new SolidColorBrush(Colors.Coral), FontWeight = FontWeights.Bold, FontSize = 20 });
            //block.Inlines.Add($"\r\n执行操作，\r\n确认继续操作吗？");
            //DoCommand(ctx =>
            //{
            //    while (true)
            //    {
            //        ctx.ChangeText("正在修炼...","");
            //        Thread.Sleep(3000);
            //        ctx.ChangeMutiText(block);
            //        Thread.Sleep(3000);
            //    }
            //});
            //return;
            #endregion

            #region 打印机状态检测
            if (!ConfigBaoAnShiYanHospital.ClosePrintStatusCheck)
            {
                var result = CheckReceiptPrinter();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "打印机检测", result.Message);
                    if (!result.Message.Contains("纸将尽"))  //纸将尽
                    {
                        return;
                    }
                }
            }
            #endregion
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select), CreateJump, new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), CreateJump, new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;
                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), RegisterJump, new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;
                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), AppointJump, new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;
                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), TakeNumJump, new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;
                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), BillPayJump, new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;
                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), RechargeJump, new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;
                case Business.查询:
                    Navigate(A.QueryChoice);
                    break;
                case Business.住院押金:
                    OnInRecharge(param);
                    break;
                case Business.检验结果:
                    var queryChoiceModel = GetInstance<IQueryChoiceModel>();
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), CreateJump, new FormContext(A.DiagReportQuery, A.JYJL.DiagReport), param.Name);
                    break;
                case Business.住院一日清单:
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), CreateJump, new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), param.Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        protected override Result CheckReceiptPrinter()
        {
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
                    return GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
            }
            return Result.Success();
        }


        protected override Task<Result<FormContext>> BillPayJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
                var billRecordModel = GetInstance<IBillRecordModel>();
                var patientModel = GetInstance<IPatientModel>();
                var cardModel = GetInstance<ICardModel>();
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                billRecordModel.Req获取缴费概要信息 = new req获取缴费概要信息
                {
                    patientId = patientModel.Res病人信息查询?.data[patientModel.PatientInfoIndex].patientId,
                    cardType = ((int)cardModel.CardType).ToString(),
                    cardNo = cardModel.CardNo,
                    billType = ""
                };
                billRecordModel.Res获取缴费概要信息 = DataHandlerEx.获取缴费概要信息(billRecordModel.Req获取缴费概要信息);
                if (billRecordModel.Res获取缴费概要信息?.success ?? false)
                {
                    if (billRecordModel.Res获取缴费概要信息?.data?.Count > 0)
                    {
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息，\n您没有待缴费的处方。");
                    Navigate(A.Home);
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
                return Result<FormContext>.Fail("");
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
                            new PayInfoItem("就诊时段：", record.medAmPm.SafeToAmPm()),
                            new PayInfoItem("就诊序号：", record.appoNo),
                            new PayInfoItem("挂号金额：", record.regAmount.In元(), true)
                        };
                        return Result<FormContext>.Success(new FormContext(A.QuHao_Context, A.QH.TakeNum));
                    }
                    ShowAlert(false, "预约记录查询", "系统未能获取到该卡有对应的预约记录\n请使用名下其他卡尝试或者去人工窗口直接取号");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "预约记录查询", "没有获得预约记录信息", debugInfo: recordModel.Res挂号预约记录查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 住院预缴金充值");
                var patientModel = GetInstance<IPatientModel>();
                if (patientModel.住院患者信息.status == "在院")
                {
                    return Result<FormContext>.Success(default(FormContext));
                }
                else
                {
                    ShowAlert(false, "住院缴押金", "出院患者不能缴押金");
                    return Result<FormContext>.Fail("");
                }
            });
        }
    }
}