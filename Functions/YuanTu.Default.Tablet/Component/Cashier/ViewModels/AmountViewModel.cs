using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Regions;
using YuanTu.Consts;
using YuanTu.Consts.Dtos;
using YuanTu.Consts.Enums;
using YuanTu.Consts.Models;
using YuanTu.Consts.Models.Payment;
using YuanTu.Consts.Models.Print;
using YuanTu.Consts.Services;
using YuanTu.Core.Extension;
using YuanTu.Core.FrameworkBase;
using YuanTu.Default.Component.Tools.Models;
using YuanTu.Default.Component.ViewModels;
using YuanTu.Default.Tablet.Component.Cashier.Models;
using YuanTu.Default.Tablet.Platform;

namespace YuanTu.Default.Tablet.Component.Cashier.ViewModels
{
    public class AmountViewModel : ViewModelBase
    {
        public override string Title => "输入金额";

        #region DataBinding

        private string _amount;
        public string Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<InfoIcon> _payOut;
        public ObservableCollection<InfoIcon> PayOut
        {
            get => _payOut;
            set
            {
                _payOut = value;
                OnPropertyChanged();
            }
        }

        #endregion

        [Dependency]
        public ICashierModel Cashier { get; set; }

        [Dependency]
        public IPaymentModel PaymentModel { get; set; }

        [Dependency]
        public IExtraPaymentModel ExtraPaymentModel { get; set; }

        public IWebService WebService { get; set; } = new MockWebService();

        [Dependency]
        public IPrintModel PrintModel { get; set; }

        [Dependency]
        public IPrintManager PrintManager { get; set; }

        [Dependency]
        public IConfigurationManager ConfigurationManager { get; set; }

        public override void OnEntered(NavigationContext navigationContext)
        {
            Amount = string.Empty;
            var listOut = PayMethodDto.GetInfoPays(
                GetInstance<IConfigurationManager>(), 
                ResourceEngine, 
                "CashierPayOut", 
                new DelegateCommand<Info>(OnButtonClick));
            PayOut = new ObservableCollection<InfoIcon>(listOut);
        }
        
        protected virtual void OnButtonClick(Info i)
        {
            if (string.IsNullOrEmpty(Amount) || !decimal.TryParse(Amount, out var amount))
            {
                ShowAlert(false, "输入金额", "请输入支付金额");
                return;
            }
            Cashier.Amount = amount * 100m;
            Cashier.GotCardFunc = PayGotCard;
            Cashier.GotCardBareFunc = PayGotCardBare;
            Confirm(i);
        }

        protected virtual void Confirm(Info i)
        {
            Cashier.PayMethod = (PayMethod)i.Tag;

            switch (Cashier.PayMethod)
            {
                case PayMethod.预缴金:
                    Navigate(AInner.SY.Card);
                    break;
                case PayMethod.微信支付:
                case PayMethod.支付宝:
                    Navigate(AInner.SY.Scan);
                    break;
                default:
                    ShowAlert(false, "支付确认", "业务未实现");
                    break;
            }
        }

        public async Task PayGotCardBare(string cardNo, string cardType)
        {
            var queryResult = await WebService.Map(cardNo, cardType);
            if (!queryResult.IsSuccess)
            {
                ShowAlert(false, "错误信息", $"查询账户信息失败:{queryResult.Value.SubMsg}");
                return;
            }
            //ShowConfirm("确认支付", "请确认支付信息:\n\n姓名:\n性别:\n应付金额:\n账户余额", yes =>
            //{
            //    if (yes)
            //        DoCommand(lp => Confirm(cardNo).Wait());
            //}, 30, ConfirmExModel.Build("确定", "取消", false));
            await Confirm(cardNo, cardType);
        }

        public async Task PayGotCard(string cardNo, string cardType)
        {
            await DoCommand(async _ =>
            {
                await PayGotCardBare(cardNo, cardType);
            });
        }

        private async Task Confirm(string cardNo, string cardType)
        {
            var createResult = await WebService.Create(cardNo, cardType, Cashier.Amount);
            if (!createResult.IsSuccess)
            {
                ShowAlert(false, "错误信息", $"创建支付订单失败:{createResult.Message}");
                return;
            }
            if (createResult.Value.SubCode != "1000")
            {
                ShowAlert(false, "错误信息", $"创建支付订单失败:{createResult.Value.SubMsg}");
                return;
            }
            var payResult = await WebService.Pay(createResult.Value, "pay_auth_code");
            if (!payResult.IsSuccess)
            {
                ShowAlert(false, "错误信息", $"订单支付失败:{payResult.Message}");
                return;
            }
            if (payResult.Value.SubCode != "1002")
            {
                ShowAlert(false, "错误信息", $"订单支付失败:{payResult.Value.SubMsg}");
                return;
            }
            Cashier.OutTradeNo = payResult.Value.OutTradeNo;
            PrintModel.SetPrintInfo(true, new PrintInfo
            {
                TypeMsg = "支付成功",
                TipMsg = $"凭证号：{payResult.Value.OutTradeNo}\n交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm}",
                PrinterName = ConfigurationManager.GetValue("Printer:Receipt"),
                Printables = Printables()
            });
            Navigate(AInner.SY.Print);
        }

        private Queue<IPrintable> Printables()
        {
            var queue = PrintManager.NewQueue("支付");
            var sb = new StringBuilder();
            //sb.Append($"商户名称：丰硕堂（某某店）\n");
            sb.Append($"设备编号：{FrameworkConst.OperatorId}\n");
            sb.Append($"支付方式：青岛区域诊疗卡\n");
            sb.Append($"卡号：{"160***4998"}\n");
            sb.Append($"支付金额：{Cashier.Amount.In元()}\n");
            sb.Append($"交易时间：{DateTimeCore.Now:yyyy-MM-dd HH:mm}\n");
            sb.Append($"凭证号：{Cashier.OutTradeNo}\n");
            sb.Append("客户签名：\n");

            return queue;
        }

        private async Task Refund(string cardNo)
        {
            var orderNo = "";
            var amount = 1m;
            var refundResult = await WebService.Refund(orderNo, amount);
            if (!refundResult.IsSuccess)
            {
                ShowAlert(false, "错误信息", $"订单退费失败:{refundResult.Message}");
                return;
            }
            if (refundResult.Value.SubCode != "1006")
                ShowAlert(false, "错误信息", $"订单退费失败:{refundResult.Value.SubMsg}");
        }
    }
}