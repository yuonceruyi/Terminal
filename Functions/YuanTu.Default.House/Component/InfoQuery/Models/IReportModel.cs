using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House.Component.InfoQuery.Models
{
    public interface IReportModel : IModel
    {
        res上传体检报告 上传体检报告结果 { get; set; }
        查询体检报告单分页数据 查询体检报告单分页数据 { get; set; }
        查询体检报告单 选中的查询体检报告单 { get; set; }
    }

    public class ReportModel : ModelBase, IReportModel
    {
        public 查询体检报告单分页数据 查询体检报告单分页数据 { get; set; }
        public 查询体检报告单 选中的查询体检报告单 { get; set; }
        public res上传体检报告 上传体检报告结果 { get; set; }
    }
}