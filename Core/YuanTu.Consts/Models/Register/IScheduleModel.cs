using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Register
{
    public interface IScheduleModel : IModel
    {
        req排班信息查询 排班信息查询 { get; set; }
        res排班信息查询 Res排班信息查询 { get; set; }
        排班信息 所选排班 { get; set; }
    }   

    public class DefaultScheduleModel : ModelBase, IScheduleModel
    {
        public req排班信息查询 排班信息查询 { get; set; }
        public res排班信息查询 Res排班信息查询 { get; set; }
        public 排班信息 所选排班 { get; set; }
    }
}