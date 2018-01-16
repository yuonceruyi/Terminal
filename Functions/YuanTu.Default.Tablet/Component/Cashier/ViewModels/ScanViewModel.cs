using System;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts.Services;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Tablet.Component.Cashier.Models;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    public class ScanViewModel : ViewModelBase
    {
        public override string Title => "支付码扫描";
        
        public DelegateCommand ConfirmCommand { get; set; }

        [Dependency]
        public ICashierModel CashierModel { get; set; }

        public ScanViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        protected virtual async void Confirm()
        {
            var payCode = PayCode;
            PayCode = string.Empty;
            await CashierModel.GotCardFunc(payCode, "99");
            InputFocus = true;
        }

        public override void OnSet()
        {
            BarCodeUri = ResourceEngine.GetImageResourceUri("卡_慧医扫码");
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            InputFocus = true;
        }

        public override bool OnLeaving(NavigationContext navigationContext)
        {
            return true;
        }

        #region DataBinding

        private bool _inputFocus;
        public bool InputFocus
        {
            get => _inputFocus;
            set
            {
                _inputFocus = value;
                OnPropertyChanged();
            }
        }

        private string _payCode;

        public string PayCode
        {
            get => _payCode;
            set
            {
                _payCode = value; 
                OnPropertyChanged();
            }
        }

        private Uri _barCodeUri;

        public Uri BarCodeUri
        {
            get => _barCodeUri;
            set
            {
                _barCodeUri = value; 
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
