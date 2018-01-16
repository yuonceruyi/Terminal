using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.Component.BillPay.Models
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