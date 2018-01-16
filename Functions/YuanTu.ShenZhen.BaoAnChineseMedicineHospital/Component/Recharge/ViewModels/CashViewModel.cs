using Prism.Commands;
using System.Linq;
using YuanTu.Consts.Enums;
using YuanTu.Core.DB;
using YuanTu.Core.Log;
using YuanTu.Devices.CashBox;

namespace YuanTu.ShenZhen.BaoAnChineseMedicineHospital.Component.Recharge.ViewModels
{
    public class CashViewModel : Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
            _cashInputBox = cashInputBoxs.FirstOrDefault(p => p.DeviceId == "CashCode");
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
            ExtraPaymentModel.TotalMoney = InputAmount * 100;
            ExtraPaymentModel.FinishFunc?.Invoke();
        }

    }
}