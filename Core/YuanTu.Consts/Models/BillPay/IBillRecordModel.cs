using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.BillPay
{
    public interface IBillRecordModel : IModel
    {
        req获取缴费概要信息 Req获取缴费概要信息 { get; set; }
        res获取缴费概要信息 Res获取缴费概要信息 { get; set; }
        缴费概要信息 所选缴费概要 { get; set; }
    }

    public class BillRecordModel : ModelBase, IBillRecordModel
    {
        public req获取缴费概要信息 Req获取缴费概要信息 { get; set; }
        public res获取缴费概要信息 Res获取缴费概要信息 { get; set; }
        public 缴费概要信息 所选缴费概要 { get; set; }
    }
}