using YuanTu.Consts.FrameworkBase;
using YuanTu.ZheJiangHospital.HIS;

namespace YuanTu.ZheJiangHospital.Component.Auth.Models
{
    public interface IAuthModel : IModel
    {
        BINGRENXX Info { get; set; }
        Res读卡 Res读卡 { get; set; }
        Res建档读卡 Res建档读卡 { get; set; }
    }

    public class AuthModel : ModelBase, IAuthModel
    {
        public BINGRENXX Info { get; set; }
        public Res读卡 Res读卡 { get; set; }
        public Res建档读卡 Res建档读卡 { get; set; }
    }
}