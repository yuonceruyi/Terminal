using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Commands;
using Prism.Regions;
using System.Threading.Tasks;
using YuanTu.Devices.CashBox;
using YuanTu.Core.Extension;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Core.Reporter;
using YuanTu.Consts.Sounds;
using YuanTu.Consts.Services;


namespace YuanTu.YanTaiYDYY.Component.Recharge.ViewModels
{
    public class CashViewModel:YuanTu.Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {

        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent("");
            ExtraPaymentModel.Complete = false;
            //var patientInfo = PatientModel.当前病人信息;
            //Name = patientInfo.name;
            //CardNo = CardModel.CardNo;
            //Remain =decimal.Parse(patientInfo.accBalance).In元();
            Name = ExtraPaymentModel.PatientInfo.Name;
            CardNo = ExtraPaymentModel.PatientInfo.CardNo;
            Remain = ExtraPaymentModel.PatientInfo.Remain.In元();

            Business = $"{ExtraPaymentModel.CurrentBusiness}";
            InputAmount = 0;
            Tips = $"请在钱箱灯变绿后投入\r\n10或20或50或100元纸币";

            Result ret;
            if ((ret = _cashInputBox.Connect()).IsSuccess && (ret = _cashInputBox.Initialize()).IsSuccess)
            {
                Task.Run(() => {
                    _cashInputBox.StartPoll(OnMoneyStacked);
                });
            }
            else
            {
                ReportService.钱箱离线(ret.Message, ErrorSolution.钱箱离线);
                ShowAlert(false, "钱箱异常", "钱箱暂时不可用", debugInfo: ret.Message);
#if !DEBUG
                Preview();
#endif
                return;
            }

            PlaySound(SoundMapping.现金充值);

            var camera = GetInstance<ICameraService>();
            camera.StartRecording("现金充值");
        }
    }
}
