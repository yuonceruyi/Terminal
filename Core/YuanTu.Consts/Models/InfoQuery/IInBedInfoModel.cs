using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
namespace YuanTu.Consts.Models.InfoQuery
{
    public  interface IInBedInfoModel : IModel
    {
        req住院床位信息查询 Req住院床位信息查询 { get; set; }
        res住院床位信息查询 Res住院床位信息查询 { get; set; }
    }

    public class InBedInfoModel : ModelBase, IInBedInfoModel
    {
        public req住院床位信息查询 Req住院床位信息查询 { get; set; }
        public res住院床位信息查询 Res住院床位信息查询 { get; set; }
    }
}
