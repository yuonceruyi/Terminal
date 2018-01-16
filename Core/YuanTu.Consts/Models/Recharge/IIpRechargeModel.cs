using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Recharge
{
    /// <summary>
    ///     住院充值
    /// </summary>
    public interface IIpRechargeModel : IModel
    {
        PayMethod RechargeMethod { get; set; }
        req住院预缴金充值 Req住院预缴金充值 { get; set; }
        res住院预缴金充值 Res住院预缴金充值 { get; set; }
    }

    public class IpRechargeModel : ModelBase, IIpRechargeModel
    {
        public PayMethod RechargeMethod { get; set; }
        public req住院预缴金充值 Req住院预缴金充值 { get; set; }
        public res住院预缴金充值 Res住院预缴金充值 { get; set; }
    }
}