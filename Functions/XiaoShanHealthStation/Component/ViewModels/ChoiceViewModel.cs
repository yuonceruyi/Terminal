using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Core.Navigating;
using YuanTu.XiaoShanArea.CYHIS.WebService;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;
using YuanTu.XiaoShanHealthStation.Component.BillPay.Models;

namespace YuanTu.XiaoShanHealthStation.Component.ViewModels
{
    public class ChoiceViewModel:Default.Component.ViewModels.ChoiceViewModel
    {
        protected override Task<Result<FormContext>> BillPayJump()
        {
            var chakaModel = GetInstance<IChaKaModel>();
            var billModel = GetInstance<IBillModel>();
            return DoCommand(lp =>
            {
                var camera = GetInstance<ICameraService>();
                camera.SnapShot("主界面 结算");
             
                lp.ChangeText("正在查询待缴费信息，请稍候...");
                MENZHENFYMX_OUT res;
                var req = new MENZHENFYMX_IN
                {
                    BASEINFO = Instance.Baseinfo,
                    JIUZHENKLX = " ",
                    JIUZHENKH = chakaModel.查询建档Out.就诊卡号,
                    BINGRENLB = chakaModel.PatientType,
                    BINGRENXZ = " ",
                    YIBAOKLX = " ",
                    YIBAOKMM = " ",
                    YIBAOKXX = " ",
                    YIBAOBRXX = " ",
                    YILIAOLB = " ",
                    JIESUANLB = " ",
                    HISBRXX = " ",
                    GUAHAOID = " "
                };
                if (!DataHandler.MENZHENFYMX(req, out res))
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息");
                    return Result<FormContext>.Fail("");
                }
                billModel.门诊费用明细Out = res;
                if (res.FEIYONGMXTS == "0" || res.FEIYONGMX == null)
                {
                    ShowAlert(false, "待缴费信息查询", "没有获得待缴费信息(列表为空)");
                    return Result<FormContext>.Fail("");
                }
              
                return Result<FormContext>.Success(default(FormContext));
            });
        }
    }
}
