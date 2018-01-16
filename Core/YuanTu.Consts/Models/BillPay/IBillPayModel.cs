using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.BillPay
{
    public interface IBillPayModel : IModel
    {
        req缴费结算 Req缴费结算 { get; set; }
        res缴费结算 Res缴费结算 { get; set; }
    }

    public class BillPayModel : ModelBase, IBillPayModel
    {
        public req缴费结算 Req缴费结算 { get; set; }
        public res缴费结算 Res缴费结算 { get; set; }
    }
}