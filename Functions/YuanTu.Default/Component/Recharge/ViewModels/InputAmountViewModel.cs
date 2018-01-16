using System;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models.Recharge;
using YuanTu.Consts.Sounds;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;

namespace YuanTu.Default.Component.Recharge.ViewModels
{
    public class InputAmountViewModel:ViewModelBase
    {
        public override string Title => "输入充值金额";

        public string Hint
        {
            get { return _hint; }
            set
            {
                _hint = value; 
                OnPropertyChanged();
            }
        }

        private string _amount;
        private string _hint = "金额输入";

        public string Amount
        {
            get { return _amount; }
            set
            {
                if (value?.Length == 2 && value == "00")
                    value = "0";
                if (value?.Length >= 2 && value.Contains("."))
                {
                    var temp = value.SafeToSplit('.');
                    if (temp.Length > 2)
                        value = temp[0] + "." + temp[1];
                }
                if (value?.Length > 2 && value.Contains("."))
                {
                    var val = value.SafeToSplit('.')[1];
                    if (val?.Length > 2)
                        value = value.Substring(0, value.Length - 1);
                }
            

                _amount = value;
                OnPropertyChanged();
            }
        }
        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }
        [Dependency]
        public IOpRechargeModel OpRechargeModel { get; set; }
        public InputAmountViewModel()
        {
            ConfirmCommand = new DelegateCommand(Confirm);
        }

        public override void OnEntered(NavigationContext navigationContext)
        {
            ChangeNavigationContent(null);
            Amount = null;
            
            
            PlaySound(SoundMapping.输入充值金额);
        }

        protected virtual void Confirm()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                ShowAlert(false, "温馨提示", "请输入充值金额", 10);
                return;
            }
            decimal amount ;
            if (!decimal.TryParse(Amount, out amount))
            {
                ShowAlert(false, "温馨提示", "请输入正确的充值金额", 10);
                return;
            }
            ExtraPaymentModel.TotalMoney = amount * 100;
            if (ExtraPaymentModel.TotalMoney <= 0)
            {
                ShowAlert(false,"温馨提示","请输入充值金额",10);
                return;
            }
            ChangeNavigationContent(ExtraPaymentModel.TotalMoney.In元());

            switch (OpRechargeModel.RechargeMethod)
            {
               
                case PayMethod.未知:
                case PayMethod.现金:
                case PayMethod.预缴金:
                case PayMethod.社保:
                    break;
                case PayMethod.银联:
                    Navigate(A.Third.PosUnion);
                    break;
                case PayMethod.支付宝:
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.微信支付:
                    Navigate(A.Third.ScanQrCode);
                    break;
                case PayMethod.苹果支付:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ICommand ConfirmCommand { get; set; }
    }
}
