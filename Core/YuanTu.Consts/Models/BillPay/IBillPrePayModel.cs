using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.BillPay
{
    public interface IBillPrePayModel : IModel
    {
        req缴费预结算 Req缴费预结算 { get; set; }
        res缴费预结算 Res缴费预结算 { get; set; }
    }

    public class BillPrePayModel : ModelBase, IBillPrePayModel
    {
        public req缴费预结算 Req缴费预结算 { get; set; }
        public res缴费预结算 Res缴费预结算 { get; set; }
    }
}