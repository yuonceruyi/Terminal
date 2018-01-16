using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Recharge
{
    /// <summary>
    ///     门诊充值
    /// </summary>
    public interface IOpRechargeModel : IModel
    {
        PayMethod RechargeMethod { get; set; }
        req预缴金充值 Req预缴金充值 { get; set; }
        res预缴金充值 Res预缴金充值 { get; set; }
        req充值数据同步到his系统 Req充值数据同步到his系统 { get; set; }
        res充值数据同步到his系统 Res充值数据同步到his系统 { get; set; }
    }

    public class OpRechargeModel : ModelBase, IOpRechargeModel
    {
        public PayMethod RechargeMethod { get; set; }
        public req预缴金充值 Req预缴金充值 { get; set; }
        public res预缴金充值 Res预缴金充值 { get; set; }
        public req充值数据同步到his系统 Req充值数据同步到his系统 { get; set; }
        public res充值数据同步到his系统 Res充值数据同步到his系统 { get; set; }
    }
}