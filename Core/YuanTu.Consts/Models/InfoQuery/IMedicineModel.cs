using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;
namespace YuanTu.Consts.Models.InfoQuery
{
    public interface IMedicineModel : IModel
    {
        req药品项目查询 Req药品项目查询 { get; set; }
        res药品项目查询 Res药品项目查询 { get; set; }

    }

    public class MedicineModel : ModelBase, IMedicineModel
    {
        public req药品项目查询 Req药品项目查询 { get; set; }
        public res药品项目查询 Res药品项目查询 { get; set; }

    }
}
