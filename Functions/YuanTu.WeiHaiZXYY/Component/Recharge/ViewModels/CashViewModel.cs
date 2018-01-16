using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Models.Auth;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CashBox;

namespace YuanTu.WeiHaiZXYY.Component.Recharge.ViewModels
{
    public class CashViewModel : Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
        }

        [Dependency]
        public IPatientModel PatientModel { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            ExtraPaymentModel.Complete = false;
            Name = ExtraPaymentModel.PatientInfo.Name;
            CardNo = ExtraPaymentModel.PatientInfo.CardNo;
            if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.住院押金)
            {
                var info = PatientModel.住院患者信息;
                Name = info.name;
                CardNo = info.patientHosId;
                Remain = decimal.Parse(info.accBalance).In元();
            }
            else
            {
                var info = PatientModel.当前病人信息;
                Name = info.name;
                Remain = info.accountNo + "元";
            }

            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            InputAmount = 0;
            Tips = "请在钱箱灯变绿后投入10、20、50或100元纸币";

            Result ret;
            if ((ret = _cashInputBox.Connect()).IsSuccess && (ret = _cashInputBox.Initialize()).IsSuccess)
            {
                Task.Run(() => { _cashInputBox.StartPoll(OnMoneyStacked); });
            }
            else
            {
                ReportService.钱箱离线(ret.Message, ErrorSolution.钱箱离线);
                ShowAlert(false, "钱箱异常", "钱箱暂时不可用", debugInfo: ret.Message);
#if !DEBUG
                Preview();
#endif
            }

            PlaySound(SoundMapping.现金充值);

            var camera = GetInstance<ICameraService>();
            camera.StartRecording("现金充值");
        }
    }
}