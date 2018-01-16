using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using YuanTu.Core.Extension;
using YuanTu.Devices.CashBox;
using YuanTu.XiaoShanHealthStation.Component.Auth.Models;

namespace YuanTu.XiaoShanHealthStation.Component.Recharge.ViewModels
{
    public class CashViewModel:Default.Component.Recharge.ViewModels.CashViewModel
    {
        [Dependency]
        public IChaKaModel ChaKaModel { get; set; }

        //限额5000元
        private const decimal  Quota=500000;
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
            _cashInputBox = cashInputBoxs.FirstOrDefault(p=>p.DeviceId=="MEI");
        }
        protected override void OnMoneyStacked(int money)
        {
            StartTimer();
            InputAmount += money;
            if (ChaKaModel.Remain + InputAmount <= Quota-10000)
                return;
            ShowAlert(false, "温馨提示", $"市民卡账户金额上限为{Quota.In元()}\n请点击结束投币完成充值");
            _cashInputBox.UnInitialize();
            _cashInputBox.DisConnect();
        }
        protected override void Confirm()
        {
            base.Confirm();
        }
    }
}
