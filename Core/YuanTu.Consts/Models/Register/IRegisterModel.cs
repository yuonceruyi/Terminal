using YuanTu.Consts.FrameworkBase;
using YuanTu.Consts.Gateway;

namespace YuanTu.Consts.Models.Register
{
    public interface IRegisterModel : IModel
    {
        req预约挂号 Req预约挂号 { get; set; }
        res预约挂号 Res预约挂号 { get; set; }
        req挂号锁号 Req挂号锁号 { get; set; }
        res挂号锁号 Res挂号锁号 { get; set; }
        req挂号解锁 Req挂号解锁 { get; set; }
        res挂号解锁 Res挂号解锁 { get; set; }
    }

    public class RegisterModel : ModelBase, IRegisterModel
    {
        public req预约挂号 Req预约挂号 { get; set; }
        public res预约挂号 Res预约挂号 { get; set; }
        public req挂号锁号 Req挂号锁号 { get; set; }
        public res挂号锁号 Res挂号锁号 { get; set; }
        public req挂号解锁 Req挂号解锁 { get; set; }
        public res挂号解锁 Res挂号解锁 { get; set; }
    }
}