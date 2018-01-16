using System;
using System.Threading.Tasks;
using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.Tablet.Component.Cashier.Models
{
    public interface ICashierModel : IModel
    {
        CashierTypeEnum Business { get; set; }
        PayMethod PayMethod { get; set; }
        decimal Amount { get; set; }
        string CardNo { get; set; }

        string CardType { get; set; }

        Func<string, string, Task> GotCardFunc { get; set; }
        Func<string, string, Task> GotCardBareFunc { get; set; }
        string OutTradeNo { get; set; }
    }

    internal class CashierModel : ModelBase, ICashierModel
    {
        public CashierTypeEnum Business { get; set; }
        public PayMethod PayMethod { get; set; }
        public decimal Amount { get; set; }
        public string CardNo { get; set; }
        public string CardType { get; set; }
        public Func<string, string, Task> GotCardFunc { get; set; }
        public Func<string, string, Task> GotCardBareFunc { get; set; }
        public string OutTradeNo { get; set; }
    }
}