using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.TakeNum
{
    public interface ICancelAppoModel : IModel
    {
        req取消预约 Req取消预约 { get; set; }
        res取消预约 Res取消预约 { get; set; }
    }

    public class CancelAppoModel : ModelBase, ICancelAppoModel
    {
        public req取消预约 Req取消预约 { get; set; }
        public res取消预约 Res取消预约 { get; set; }
    }
}