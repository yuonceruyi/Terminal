using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangSecondHospital.NativeService.Dto;

namespace YuanTu.YuHangSecondHospital.Component.BillPay.Models
{
    public interface IPrePayModel : IModel
    {
        PerCheckout PerCheckout { get; set; }
    }

    public class PrePayModel : ModelBase, IPrePayModel
    {
        public PerCheckout PerCheckout { get; set; }
    }
}