using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangZYY.NativeService.Dto;

namespace YuanTu.YuHangZYY.Component.BillPay.Models
{
    public interface IPrePayModel : IModel
    {
        PerCheckout PerCheckout { get; set; }
        Checkout Checkout { get; set; }
    }

    public class PrePayModel : ModelBase, IPrePayModel
    {
        public PerCheckout PerCheckout { get; set; }
        public Checkout Checkout { get; set; }
    }
}