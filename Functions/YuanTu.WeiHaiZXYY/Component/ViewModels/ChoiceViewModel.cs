using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Devices.PrinterCheck;
using YuanTu.Devices.PrinterCheck.MsPrinterCheck;


namespace YuanTu.WeiHaiZXYY.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        public override void OnSetButton()
        {
            var resource = ResourceEngine;
            var config = GetInstance<IConfigurationManager>();

            LayoutRule = config.GetValue("LayoutRule");
            var buttons = new List<ChoiceButtonInfo>();
            var k = Enum.GetValues(typeof(Business));
            foreach (Business buttonInfo in k)
            {
                var v = config.GetValue($"Functions:{buttonInfo}:Visabled");
                if (v != "1") continue;
                buttons.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue($"Functions:{buttonInfo}:Name") ?? "未定义",
                    ButtonBusiness = buttonInfo,
                    Order = config.GetValueInt($"Functions:{buttonInfo}:Order"),
                    IsEnabled = config.GetValueInt($"Functions:{buttonInfo}:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue($"Functions:{buttonInfo}:ImageName"))
                });
            }
            var visabled = config.GetValue("Functions:绑定车牌:Visabled");
            if (visabled == "1")
            {
                buttons.Add(new ChoiceButtonInfo
                {
                    Name = config.GetValue("Functions:绑定车牌:Name"),
                    ButtonBusiness = (Business)100,
                    Order = config.GetValueInt("Functions:绑定车牌:Order"),
                    IsEnabled = config.GetValueInt("Functions:绑定车牌:IsEnabled") == 1,
                    ImageSource = resource.GetImageResource(config.GetValue("Functions:绑定车牌:ImageName"))
                });
            }
            Data = buttons.OrderBy(b => b.Order).ToList();
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
            if (choiceModel.Business == (Business)100)
            {
                engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                    CreateJump,
                    new FormContext(AInner.BindLisencePlate,A.Home), param.Name);
                return;
            }
            switch (param.ButtonBusiness)
            {
                case Business.建档:
                    var config = GetInstance<IConfigurationManager>();
                    if (config.GetValue("SelectCreateType") == "1")
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Select),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    else
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.IDCard),
                            CreateJump,
                            new FormContext(A.JianDang_Context, A.JD.Print), param.Name);
                    break;

                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Dept), param.Name);
                    break;

                case Business.预约:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), AppointJump,
                        new FormContext(A.YuYue_Context, A.YY.Date), param.Name);
                    break;

                case Business.取号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), TakeNumJump,
                        new FormContext(A.QuHao_Context, A.QH.Record), param.Name);
                    break;

                case Business.缴费:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card), BillPayJump,
                        new FormContext(A.JiaoFei_Context, A.JF.BillRecord), param.Name);
                    break;

                case Business.充值:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                        RechargeJump,
                        new FormContext(A.ChongZhi_Context, A.CZ.RechargeWay), param.Name);
                    break;

                case Business.查询:
                    Navigate(A.QueryChoice);

                    break;
                case Business.住院押金:
                    OnInRecharge(param);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        private Result<FormContext> 科室排班信息查询()
        {
            var deptartmentModel = GetInstance<IDeptartmentModel>();
            var patientModel = GetInstance<IPatientModel>();
            deptartmentModel.排班科室信息查询 = new req排班科室信息查询
            {
                regMode = "2",
                startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                endDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                extend = patientModel.当前病人信息.patientId
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

        protected override Result CheckReceiptPrinter()
        {
            if (FrameworkConst.DeviceType == "ZJ-350")
            {
                return Result.Success();
            }
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
    }
}
