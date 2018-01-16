using YuanTu.Consts.FrameworkBase;
using YuanTu.YuHangFYBJY.NativeService.Dto;

namespace YuanTu.YuHangFYBJY.Component.Register.Models
{
    public interface IPreRegModel : IModel
    {
        PerRegisterPay Res挂号预处理 { get; set; }

        RegisterPay Res挂号结果 { get; set; }
    }

    public class PreRegModel : ModelBase, IPreRegModel
    {
        public PerRegisterPay Res挂号预处理 { get; set; }
        public RegisterPay Res挂号结果 { get; set; }
    }
}