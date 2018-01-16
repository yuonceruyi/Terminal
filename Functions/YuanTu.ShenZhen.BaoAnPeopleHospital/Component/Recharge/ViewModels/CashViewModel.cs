using Prism.Regions;
using System.Linq;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Services;
using YuanTu.Consts.Sounds;
using YuanTu.Core.DB;
using YuanTu.Core.Extension;
using YuanTu.Core.Log;
using YuanTu.Core.Reporter;
using YuanTu.Devices.CashBox;

namespace YuanTu.ShenZhen.BaoAnPeopleHospital.Component.Recharge.ViewModels
{
    public class CashViewModel : Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
            _cashInputBox = cashInputBoxs.FirstOrDefault(p => p.DeviceId == "NV200");
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
            Tips = "请在钱箱灯变绿后投入50或100元纸币";

            var cashStatus = _cashInputBox.GetDeviceStatus();
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

        protected override void OnMoneyStacked(int money)
        {
            StartTimer();
            InputAmount += money;// ConfirmCommand
            if (InputAmount >= 5000)
            {
                ShowAlert(true, "充值达到最大限额", "已达到单次充值的最大金额，系统将自动进行充值", extend: new Consts.Models.AlertExModel{
                    HideCallback = p =>
                    {
                        if (p == Consts.Models.AlertHideType.ButtonClick)
                            ConfirmCommand.Execute();
                    }
                });
            }
        }


        protected override void Confirm()
        {
            if (InputAmount <= 0)
            {
                ShowAlert(false, "请投币", "请您塞入纸币");
                return;
            }
            var ret = _cashInputBox.GetDeviceStatus();
            if (ret.IsSuccess)
            {
                if (ret.Value == DeviceStatus.Busy)
                {
                    ShowAlert(false, "正在充值", "纸币识别器正忙\n请稍候", debugInfo: ret.Value.ToString());
                    return;
                }
            }
            _cashInputBox.UnInitialize();
            _cashInputBox.DisConnect();
            Logger.Main.Info("投币金额是:" + InputAmount + ",开始充值");
            //充值成功后再记账
            //DBManager.Insert(new CashInputInfo
            //{
            //    TotalSeconds = InputAmount * 100
            //});
            ExtraPaymentModel.TotalMoney = InputAmount * 100;
            ExtraPaymentModel.FinishFunc?.Invoke();
        }

    }
}