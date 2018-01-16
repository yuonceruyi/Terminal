using Microsoft.Practices.Unity;
using System;
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
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Models.TakeNum;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase.Loadings;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;


namespace YuanTu.QDHD2ZY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.QDKouQiangYY.Component.ViewModels.ChoiceViewModel
    {       
        protected override void Do(ChoiceButtonInfo param)
        {
            Result result;

            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var queryChoiceModel = GetInstance<IQueryChoiceModel>();
            var engine = NavigationEngine;

            if (FrameworkConst.DeviceType != "YT-740")
            {
                result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                //if (!result.IsSuccess)
                //{
                //    ShowAlert(false, "打印机检测", result.Message);
                //    return;
                //}
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    //  ShowMask(true, new FullInputBoard() { SelectWords = p => { Console.WriteLine(p); } ), 0.1, pt => { ShowMask(false); }); return;
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
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Dept), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                    OnInRecharge(param);
                    break;
                case Business.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                     CreateJump,
                     new FormContext(A.DiagReportQuery, A.JYJL.Date), param.Name);
                    queryChoiceModel.InfoQueryType = InfoQueryTypeEnum.检验结果;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            var DeptartmentModel = GetInstance<IDeptartmentModel>();

            var camera = GetInstance<ICameraService>();
            camera.SnapShot("主界面 现场挂号");
            Thread.Sleep(100);

            var lp = GetInstance<LoadingProcesser>();
            lp.ChangeText("正在查询排班科室，请稍候...");
            DeptartmentModel.排班科室信息查询 = new req排班科室信息查询
            {
                regMode = "2",
                regType = string.Empty,
                startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Today.ToString("yyyy-MM-dd")
            };
            DeptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(DeptartmentModel.排班科室信息查询);
            if (DeptartmentModel.Res排班科室信息查询?.success ?? false)
            {
                if (DeptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                {
                    return Task.Run(() => Result<FormContext>.Success(default(FormContext)));
                }
                else
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    return Task.Run(() => Result<FormContext>.Fail(""));
                }
            }
            else
            {
                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: DeptartmentModel.Res排班科室信息查询?.msg);
                return Task.Run(() => Result<FormContext>.Fail(""));
            }
        }
    }
}
