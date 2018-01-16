using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{
    public interface IPacsReportModel : IModel
    {
        req影像诊断结果查询 Req影像诊断结果查询 { get; set; }
        res影像诊断结果查询 Res影像诊断结果查询 { get; set; }
        影像诊断结果 影像诊断结果 { get; set; }
    }

    public class PacsReportModel : ModelBase, IPacsReportModel
    {
        public req影像诊断结果查询 Req影像诊断结果查询 { get; set; }
        public res影像诊断结果查询 Res影像诊断结果查询 { get; set; }
        public 影像诊断结果 影像诊断结果 { get; set; }
    }
}
