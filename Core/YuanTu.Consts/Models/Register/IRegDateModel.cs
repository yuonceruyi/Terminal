using YuanTu.Consts.Enums;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Consts.Models.Register
{
    public interface IRegDateModel : IModel
    {
        AmPmSession AmPm { get; set; }
        string RegDate { get; set; }
    }

    public class RegDateModel : ModelBase, IRegDateModel
    {
        public AmPmSession AmPm { get; set; }
        public string RegDate { get; set; }
    }
}