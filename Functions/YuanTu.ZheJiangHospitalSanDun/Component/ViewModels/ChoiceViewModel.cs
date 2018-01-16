using System.Threading.Tasks;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Models.BillPay;
using YuanTu.Consts.Models.Register;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;

namespace YuanTu.ZheJiangHospitalSanDun.Component.ViewModels
{
    class ChoiceViewModel : Default.Component.ViewModels.ChoiceViewModel
    {
        protected override void SwitchBusiness(ChoiceButtonInfo param, IChoiceModel choiceModel, NavigationEngine engine,
            IConfigurationManager config)
        {
            switch (param.ButtonBusiness)
            {
                case Business.挂号:
                    engine.JumpAfterFlow(new FormContext(A.ChaKa_Context, A.CK.Choice),
                        RegisterJump,
                        new FormContext(A.XianChang_Context, A.XC.Dept), param.Name);
                    break;

                default:
                    base.SwitchBusiness(param, choiceModel, engine, config);
                    break;
            }
        }

        protected override Task<Result<FormContext>> RegisterJump()
        {
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 现场挂号");
                //Thread.Sleep(100);
                var regdateModel = GetInstance<IRegDateModel>();
                var deptartmentModel = GetInstance<IDeptartmentModel>();
                var regDateModel = GetInstance<IRegDateModel>();

                lp.ChangeText("正在查询排班科室，请稍候...");
                regdateModel.RegDate = DateTimeCore.Now.ToString("yyyy-MM-dd");
                var req = new req排班科室信息查询
                {
                    regMode = "2",
                    //regType = ((int)regTypesModel.SelectRegType.RegType).ToString(),
                    startDate = regDateModel.RegDate,
                    endDate = regDateModel.RegDate
                };
                deptartmentModel.排班科室信息查询 = req;
                var res = DataHandlerEx.排班科室信息查询(req);
                deptartmentModel.Res排班科室信息查询 = res;
                if (res == null || !res.success)
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息", debugInfo: res?.msg);
                    return Result<FormContext>.Fail("");
                }
                if (res.data == null || res.data.Count <= 0)
                {
                    ShowAlert(false, "科室列表查询", "没有获得科室信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
                return Result<FormContext>.Success(null);
            });
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
                var req = new req获取缴费概要信息
                {
                    patientId = patientModel.当前病人信息.patientId,
                    cardType = ((int)cardModel.CardType).ToString(),
                    cardNo = cardModel.CardNo,
                    billType = ""
                };
                billRecordModel.Req获取缴费概要信息 = req;
                var res = DataHandlerEx.获取缴费概要信息(req);
                billRecordModel.Res获取缴费概要信息 = res;

                if (res == null || !res.success)
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息", debugInfo: res?.msg);
                    return Result<FormContext>.Fail("");
                }

                if (res.data == null || res.data.Count == 0)
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }

                return Result<FormContext>.Success(default(FormContext));
            });
        }
    }
}
