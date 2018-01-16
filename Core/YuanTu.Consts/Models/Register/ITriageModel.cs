using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
using YuanTu.Consts.Triage;

namespace YuanTu.Consts.Models.Register
{
    public  interface ITriageModel:IModel
    {
        req需预检科室信息查询 Req需预检科室信息查询 { get; set; }
        res需预检科室信息查询 Res需预检科室信息查询 { get; set; }
        req需预检挂号类别信息查询 Req需预检挂号类别信息查询 { get; set; }
        res需预检挂号类别信息查询 Res需预检挂号类别信息查询 { get; set; }
        req患者预检记录信息查询 Req患者预检记录信息查询 { get; set; }
        res患者预检记录信息查询 Res患者预检记录信息查询 { get; set; }
        bool IsRegTypeNeedTriage { get; set; }
        bool IsDeptNeedTriage { get; set; }
    }

    public class TriageModel : ModelBase, ITriageModel
    {
        public req需预检科室信息查询 Req需预检科室信息查询 { get; set; }
        public res需预检科室信息查询 Res需预检科室信息查询 { get; set; }
        public req需预检挂号类别信息查询 Req需预检挂号类别信息查询 { get; set; }
        public res需预检挂号类别信息查询 Res需预检挂号类别信息查询 { get; set; }
        public req患者预检记录信息查询 Req患者预检记录信息查询 { get; set; }
        public res患者预检记录信息查询 Res患者预检记录信息查询 { get; set; }
        public bool IsRegTypeNeedTriage { get; set; } = false;
        public bool IsDeptNeedTriage { get; set; } = false;

    }
}
