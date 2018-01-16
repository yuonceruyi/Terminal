using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Prism.Regions;

namespace YuanTu.ChongQingArea.Component.Tools.ViewModels
{
    public class PosViewModel:YuanTu.Default.Component.Tools.ViewModels.PosViewModel
    {
        private int _remainHeight;

        public int RemainHeight
        {
            get { return _remainHeight; }
            set { _remainHeight = value; OnPropertyChanged(); }
        }
        public override void OnEntered(NavigationContext navigationContext)
        {
            base.OnEntered(navigationContext);
            if (ExtraPaymentModel.CurrentBusiness == Consts.Enums.Business.补卡)
            {
                RemainHeight = 0;
            }
            else
            {
                RemainHeight = 66;
            }
        }
        //private DispatcherTimer _timer;
        //public override void OnEntered(NavigationContext navigationContext)
        //{
        //    base.OnEntered(navigationContext);
        //    _timer = new DispatcherTimer();
        //    _timer.Tick += Timer_Tick;
        //    _timer.Interval = new TimeSpan(0, 0, 5);
        //    _timer.Start();
        //}
        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    StartPay();
        //}
    }
}
