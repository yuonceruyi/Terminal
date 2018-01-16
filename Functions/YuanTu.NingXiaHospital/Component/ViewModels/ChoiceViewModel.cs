using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.Events;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Configs;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.Devices.CardReader;

namespace YuanTu.NingXiaHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Clinic.Component.ViewModels.ChoiceViewModel
    {
        public override void OnSetButton()
        {
            base.OnSetButton();
            if (CurrentStrategyType() == DeviceType.Clinic)
            {
                if (!string.IsNullOrWhiteSpace(Startup.VideoPath))
                {
                    var uri = new Uri(Startup.VideoPath, UriKind.Absolute);
                    VideoUri = uri;
                }
            }
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (CurrentStrategyType() != DeviceType.Clinic)
            {
                try
                {
                    var vm = GetInstance<IRFCpuCardReader[]>().FirstOrDefault(p => p.DeviceId == "DKA6_IC");
                    vm.UnInitialize();
                    vm.DisConnect();
                }
                catch (Exception)
                {
                }
            }

        }

        protected override void Do(ChoiceButtonInfo param)
        {
            if (IsNewMode && param.SubModules.Count > 0)
            {
                PushButtonsLayOut(param.SubModules);
                var eventAggregator = GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<ModulesChangeEvent>()
                    .Publish(new ModulesChangeEvent
                    {
                        ModulesChangeAction = PopButtonsLayOut,
                        ButtonStack = ButtonStack,
                        ResetAction = OnSetButton
                    });
                return;
            }
            var choiceModel = GetInstance<IChoiceModel>();
            choiceModel.Business = param.ButtonBusiness;
            choiceModel.HasAuthFlow = true; //默认有插卡流程
            var engine = NavigationEngine;

            if (CurrentStrategyType()!= DeviceType.Clinic)
            {
                var result = CheckReceiptPrinter();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "打印机检测", result.Message);
                    return;
                }
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
                    ShowAlert(true, "签到", "签到请直接刷卡或插入医保卡");
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
                    choiceModel.AuthContext = A.ZhuYuan_Context;
                    engine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.InPatientNo),
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

        protected override void OnInRecharge(ChoiceButtonInfo param)
        {
            var choiceModel = GetInstance<IChoiceModel>();
            //choiceModel.HasAuthFlow = false;
            choiceModel.AuthContext = A.ZhuYuan_Context;
            NavigationEngine.JumpAfterFlow(new FormContext(A.ZhuYuan_Context, A.ZY.Choice), this.IpRechargeJump,
                new FormContext(A.IpRecharge_Context, A.ZYCZ.RechargeWay), param.Name);
        }

        protected override Task<Result<FormContext>> IpRechargeJump()
        {
            return null;
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            var deptartmentModel = GetInstance<IDeptartmentModel>();
            return DoCommand(lp =>
            {
                lp.ChangeText("正在查询科室，请稍候...");
                deptartmentModel.排班科室信息查询 = new req排班科室信息查询
                {
                    regMode = "2",
                    regType = "",
                    startDate = DateTimeCore.Today.ToString("yyyy-MM-dd"),
                    endDate = DateTimeCore.Today.ToString("yyyy-MM-dd")
                };
                deptartmentModel.Res排班科室信息查询 = DataHandlerEx.排班科室信息查询(deptartmentModel.排班科室信息查询);
                if (deptartmentModel.Res排班科室信息查询?.success ?? false)
                    if (deptartmentModel.Res排班科室信息查询?.data?.Count > 0)
                    {
                        //ChangeNavigationContent(i.Title);
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    else
                    {
                        ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                        return Result<FormContext>.Fail("");
                    }
                ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: deptartmentModel.Res排班科室信息查询?.msg);
                return Result<FormContext>.Fail("");
            });
        }
    }
}