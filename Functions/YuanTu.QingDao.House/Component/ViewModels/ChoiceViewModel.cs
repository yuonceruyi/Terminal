using System.Threading.Tasks;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.UserCenter.Register;
using YuanTu.Consts.UserCenter;
using YuanTu.Core.Navigating;
using YuanTu.Default.House;
using YuanTu.Default.House.Common;
using YuanTu.Devices.PrinterCheck;

namespace YuanTu.QingDao.House.Component.ViewModels
{
    public class ChoiceViewModel : Default.House.Component.ViewModels.ChoiceViewModel
    {
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            FrameworkConst.HospitalName = "健康自助终端";
        }

        protected override void Confirm(ChoiceButtonInfo param)
        {
            DoCommand(p =>
            {
                var result = GetInstance<IReceiptPrinterCheckService>().CheckReceiptPrinter();
                if (!result.IsSuccess)
                {
                    ShowAlert(false, "打印机检测", result.Message);
                    return;
                }
                var choiceModel = GetInstance<IChoiceModel>();

                choiceModel.Business = param.ButtonBusiness;

                var engine = NavigationEngine;

                switch (param.ButtonBusiness)
                {
                    case Business.健康服务:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                            null,
                            new FormContext(AInner.Health_Context, ViewContexts.ViewContextList[0].Address), "健康服务");
                        break;

                    case Business.挂号:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                            RegisterJump,
                            new FormContext(A.XianChang_Context, AInner.XC.ChoiceHospital), "自助挂号");
                        break;

                    case Business.预约:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                            AppointJump,
                            new FormContext(A.YuYue_Context, AInner.YY.ChoiceHospital), "自助预约");
                        break;

                    case Business.体测查询:
                        engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Card),
                            null,
                            new FormContext(AInner.Query_Context, AInner.Query.DateTimeView), "时间选择");
                        break;

                    case Business.建档:
                        engine.JumpAfterFlow(new FormContext(AInner.Create_Context, AInner.JD.SelectType),
                            null,
                            new FormContext(AInner.Create_Context, AInner.JD.SelectType), "");
                        break;

                    default:
                        ShowAlert(false, "温馨提示", "业务未开通");
                        break;
                }
            });
        }

        protected virtual Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                //var camera = GetInstance<ICameraService>();
                //camera.SnapShot("主界面 挂号");
                var registerModel = GetInstance<IRegisterModel>();
                lp.ChangeText("正在查询医院列表，请稍候...");
                var req = new req医院列表
                {
                    unionId = FrameworkConst.UnionId
                };

                var res = DataHandlerEx.医院列表(req);
                if (res?.success ?? false)
                {
                    if (res?.data?.Count > 0)
                    {
                        registerModel.Res医院列表 = res;
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "获取医院列表", "没有获得医院列表信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "获取医院列表", $"没有获得医院列表信息:{res?.msg}");
                return Result<FormContext>.Fail("");
            });
        }

        protected virtual Task<Result<FormContext>> AppointJump()
        {
            return DoCommand(lp =>
            {
                //var camera = GetInstance<ICameraService>();
                //camera.SnapShot("主界面 预约");
                var registerModel = GetInstance<IRegisterModel>();
                lp.ChangeText("正在查询医院列表，请稍候...");
                var req = new req医院列表
                {
                    unionId = FrameworkConst.UnionId
                };

                var res = DataHandlerEx.医院列表(req);
                if (res?.success ?? false)
                {
                    if (res?.data?.Count > 0)
                    {
                        registerModel.Res医院列表 = res;
                        return Result<FormContext>.Success(default(FormContext));
                    }
                    ShowAlert(false, "获取医院列表", "没有获得医院列表信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                ShowAlert(false, "获取医院列表", $"没有获得医院列表信息:{res?.msg}");
                return Result<FormContext>.Fail("");
            });
        }
    }
}