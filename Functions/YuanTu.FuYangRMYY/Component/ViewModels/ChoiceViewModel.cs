using System;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Regions;
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
using YuanTu.Consts.UserControls;
using YuanTu.FuYangRMYY.Component.SignIn.Models;
using YuanTu.FuYangRMYY.Services;

namespace YuanTu.FuYangRMYY.Component.ViewModels
{
    public class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {

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

                case Business.补打:
                    Navigate(A.PrintAgainChoice);
                    break;

                case Business.实名认证:
                    engine.JumpAfterFlow(null,
                                RealAuthJump,
                                new FormContext(A.RealAuth_Context, A.SMRZ.Card), param.Name);
                    break;

                case Business.生物信息录入:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        BiometricJump,
                        new FormContext(A.Biometric_Context, A.Bio.Choice), param.Name);
                    break;
                case Business.签到:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        SignInJump,
                        new FormContext(A.QueneSelect_Context, A.SignIn.RegisterInfoSelect), param.Name);
                    break;

                case Business.药品查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.MedicineQuery, A.YP.Query), param.Name);
                    break;

                case Business.项目查询:
                    choiceModel.HasAuthFlow = false;
                    engine.JumpAfterFlow(null,
                        CreateJump,
                        new FormContext(A.ChargeItemsQuery, A.XM.Query), param.Name);
                    break;

                case Business.已缴费明细:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PayCostQuery, A.JFJL.Date), param.Name);
                    break;

                case Business.住院一日清单:
                    //choiceModel.HasAuthFlow = false;
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice),
                        CreateJump,
                        new FormContext(A.InDayDetailList_Context, A.ZYYRQD.Date), param.Name);
                    break;

                case Business.检验结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.DiagReportQuery, A.JYJL.Date), param.Name);
                    break;

                case Business.影像结果:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.PacsReportQuery, A.YXBG.Date), param.Name);
                    break;
                case Business.交易记录查询:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        CreateJump,
                        new FormContext(A.ReChargeQuery, A.CZJL.Date), param.Name);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 现场挂号");

                lp.ChangeText("正在查询排班科室，请稍候...");
                var department = GetInstance<IDeptartmentModel>();
                department.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = "2",
                    regType = null,
                    startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Today.ToString("yyyy-MM-dd")
                };
                department.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(department.排班科室信息查询);
                if (department.Res排班科室信息查询?.success ?? false)
                    if (department.Res排班科室信息查询?.data?.Count > 0)
                    {
                        Next();
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                        return Result<FormContext>.Fail(null);
                    }
                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: department.Res排班科室信息查询?.msg);
                return Result<FormContext>.Fail(null);
            });
        }

        protected virtual Task<Result<FormContext>> SignInJump()
        {
            return DoCommand(lp =>
            {
                var passStatus = new[] {"报道", "过号", "到达"};
                lp.ChangeText("正在查询挂号信息，请稍后...");
                var patient = GetInstance<IPatientModel>();
                var resp = HisService.GetRegisterInfo(patient.当前病人信息);
                if (resp.IsSuccess)
                {
                    var model = GetInstance<ISignInModel>();
                    var realValue = resp.Value.Orders.Where(p => p.Status == "正常" && passStatus.Contains(p.QueueStatus)).ToArray();
                    if (!realValue.Any())
                    {
                        ShowAlert(false,"签到信息", "您没有需要签到的信息，请确认是否挂号成功或者已经签到！");
                        return Result<FormContext>.Fail("没有签到信息");
                    }
                    model.ResponseOrders = realValue;
                    return Result<FormContext>.Success(default(FormContext));
                }
                ShowAlert(false, "签到信息", "没有获得您签到的信息，请确认是否挂号成功或者已经签到！");
                return Result<FormContext>.Fail("没有签到信息！");
            });
        }

        public override void OnSet()
        {
            base.OnSet();

            var eventAggregator = GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<Clinic.VideoEvent>().Publish(new Clinic.VideoEvent
            {
                eventStatus = 1
            });
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            var eventAggregator = GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<Clinic.VideoEvent>().Publish(new Clinic.VideoEvent
            {
                eventStatus = 2
            });

        }
        protected override void OnInRecharge(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            //choiceModel.HasAuthFlow = false;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice), this.IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.RechargeWay), param.Name);
        }
    }
}