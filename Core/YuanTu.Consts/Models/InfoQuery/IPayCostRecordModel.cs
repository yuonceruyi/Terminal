using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{
    public interface IPayCostRecordModel : IModel
    {
        req获取已结算记录 Req获取已结算记录 { get; set; }
        res获取已结算记录 Res获取已结算记录 { get; set; }

        已缴费概要信息 当前的已缴费记录 { get; set; }
    }

    public class PayCostRecordModel : ModelBase, IPayCostRecordModel
    {
        public req获取已结算记录 Req获取已结算记录 { get; set; }
        public res获取已结算记录 Res获取已结算记录 { get; set; }
        public 已缴费概要信息 当前的已缴费记录 { get; set; }
    }
}