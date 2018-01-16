using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.InfoQuery;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.ViewModels
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
            if (!ConfigBaoAnPeopleHospital.ClosePrintStatusCheck)
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
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), CreateJump, new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;
                case Business.挂号:
                    int hour = DateTimeCore.Now.Hour;//小时
                    if (hour < 7 || hour > 17 || (hour > 11 && hour < 14))  //
                    {
                        ShowAlert(false, "当前时间不开放挂号", "上午7点至12点，下午14点到18点位开放挂号时间");
                        Navigate(A.Home);
                        return;
                    }
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), RegisterJump, new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,   new FormContext(A.QuHao_Context, A.QH.Query), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),RechargeJump,new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);
                    break;

                case Business.住院押金:
                    OnInRecharge(param);
                    break;
                case Business.检验结果:
                    var queryChoiceModel = GetInstance<IQueryChoiceModel>();
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), CreateJump, new FormContext(A.DiagReportQuery, A.JYJL.Date), param.Name);
                    break;
                case Business.住院一日清单:
                    ShowConfirm("确定打印住院一日清单", "确定打印住院一日清单？\n打印住院一日清单只能打印昨天的清单\n点击“取消”可查看以往的住院费用", cb =>
                     {
                         choiceModel.AuthContext = A.ZhuYuan_Context;
                         //点击取消为false
                         DoCommand(tt =>
                         {
                             if (cb)
                                 engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), CreateJump, new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), param.Name);
                             else
                                 engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo), CreateJump, new FormContext(InnerA.InHospitalFeeList_Context, InnerA.ZYFYCX.Date), "住院费用清单查询");
                             return;
                         });
                     });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                        //#region 将处方日期加上
                        //List<缴费概要信息> temp = new List<缴费概要信息>();
                        //foreach (var item in billRecordModel.Res获取缴费概要信息.data)
                        //{
                        //    if (item.deptName.Contains("-"))
                        //    {
                        //        item.deptName = item.deptName.Split('-')[1] + "[" + Convert.ToDateTime(item.billDate).ToString("yyyy-MM-dd") + "]";
                        //    }
                        //    temp.Add(item);
                        //}
                        //billRecordModel.Res获取缴费概要信息.data = temp;
                        //#endregion
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: billRecordModel.Res获取缴费概要信息?.msg);
                return Result<FormContext>.Fail("");
            });
        }

        protected override Task<Result<FormContext>> TakeNumJump()
        {
            //直接返回，走输入订单号页面
            return null;
        }
    }
}