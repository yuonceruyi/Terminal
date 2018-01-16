using System;
using System.Threading.Tasks;
using Prism.Events;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.EventModels;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;

namespace YuanTu.ZheJiangZhongLiuHospital.Component.ViewModels
{
    public class ChoiceViewModel : YuanTu.Default.Component.ViewModels.ChoiceViewModel
    {
        protected override void Do(ChoiceButtonInfo param)
        {
            if (IsNewMode && param.SubModules.Count > 0)
            {
                PushButtonsLayOut(param.SubModules);
                var eventAggregator = GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<ModulesChangeEvent>()
                    .Publish(new ModulesChangeEvent { ModulesChangeAction = PopButtonsLayOut, ButtonStack = ButtonStack, ResetAction = OnSetButton });
                return;
            }
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
                        new FormContext(A.XianChang_Context, A.XC.Wether), param.Name);
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
                        CreateJump,
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
        protected Task<Result<FormContext>> SigninJump()
        {
            ShowAlert(true, "签到", "签到成功");
            Navigate(A.Home);
            return null;
        }

    }
}
