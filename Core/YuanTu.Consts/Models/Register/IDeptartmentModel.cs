using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Register
{
    public interface IDeptartmentModel : IModel
    {
        req排班科室信息查询 排班科室信息查询 { get; set; }
        res排班科室信息查询 Res排班科室信息查询 { get; set; }
        排班科室信息 所选科室 { get; set; }
        排班科室信息 所选父科室 { get; set; }
    }

    public class DefaultDeptartmentModel : ModelBase, IDeptartmentModel
    {
        public req排班科室信息查询 排班科室信息查询 { get; set; }
        public res排班科室信息查询 Res排班科室信息查询 { get; set; }
        public 排班科室信息 所选科室 { get; set; }
        public 排班科室信息 所选父科室 { get; set; }
    }
}