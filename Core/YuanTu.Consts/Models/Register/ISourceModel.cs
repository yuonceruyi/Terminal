using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
namespace YuanTu.Consts.Models.Register
{
    public  interface ISourceModel:IModel
    {
        req号源明细查询 Req号源明细查询 { get; set; }
        res号源明细查询 Res号源明细查询 { get; set; }
        号源明细 所选号源 { get; set; }
    }

    public class SourceModel : ModelBase, ISourceModel
    {
        public req号源明细查询 Req号源明细查询 { get; set; }
        public res号源明细查询 Res号源明细查询 { get; set; }
        public 号源明细 所选号源 { get; set; }
    }
}
