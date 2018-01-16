using Microsoft.Practices.Unity;
using Prism.Regions;
using YuanTu.Devices.CashBox;
using YuanTu.ZheJiangHospital.Component.Recharge.Models;

namespace YuanTu.ZheJiangHospital.Component.Recharge.ViewModels
{
    public class CashViewModel : Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);

            Recharge.Counts = new int[7];
        }

        [Dependency]
        public IAccountModel Account { get; set; }
        [Dependency]
        public IRechargeModel Recharge { get; set; }
        protected override void OnMoneyStacked(int money)
        {
            StartTimer();
            InputAmount += money;
            switch (money / 100)
            {
                case 1:
                    Recharge.Counts[0]++;
                    break;

                case 2:
                    Recharge.Counts[1]++;
                    break;

                case 5:
                    Recharge.Counts[2]++;
                    break;

                case 10:
                    Recharge.Counts[3]++;
                    break;

                case 20:
                    Recharge.Counts[4]++;
                    break;

                case 50:
                    Recharge.Counts[5]++;
                    break;

                case 100:
                    Recharge.Counts[6]++;
                    break;
            }
            if (InputAmount + Account.Balance > 190000)
            {
                _cashInputBox.UnInitialize();
                ShowAlert(true, "温馨提示", "银医通账户余额上限为2000元\n请点击结束投币完成充值");
            }
        }
    }
}