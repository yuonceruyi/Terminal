using YuanTu.Consts.FrameworkBase;
using YuanTu.Default.House.HealthManager;

namespace YuanTu.Default.House.Component.Auth.Models
{
    public interface IHealthModel : IModel
    {
        res查询是否已建档 Res查询是否已建档 { get; set; }
    }

    public class HealthModel : ModelBase, IHealthModel
    {
        public res查询是否已建档 Res查询是否已建档 { get; set; }
    }
}