using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.InfoQuery
{
    public interface IDiagReportModel : IModel
    {
        req检验基本信息查询 Req检验基本信息查询 { get; set; }
        res检验基本信息查询 Res检验基本信息查询 { get; set; }
        检验基本信息 所选检验信息 { get; set; }
    }

    public class DiagReportModel : ModelBase, IDiagReportModel
    {
        public req检验基本信息查询 Req检验基本信息查询 { get; set; }
        public res检验基本信息查询 Res检验基本信息查询 { get; set; }
        public 检验基本信息 所选检验信息 { get; set; }
    }
}
