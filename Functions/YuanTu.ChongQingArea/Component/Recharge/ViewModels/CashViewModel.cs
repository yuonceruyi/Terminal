using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Regions;
using YuanTu.Devices.CashBox;

namespace YuanTu.ChongQingArea.Component.Recharge.ViewModels
{
    public class CashViewModel:YuanTu.Default.Component.Recharge.ViewModels.CashViewModel
    {
        public CashViewModel(ICashInputBox[] cashInputBoxs) : base(cashInputBoxs)
        {
        }

        public Visibility RemainVisibility { get; set; }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.补卡)
            {
                RemainVisibility=Visibility.Collapsed;
            }
            else
            {
                RemainVisibility=Visibility.Visible;
            }
        }
    }
}
