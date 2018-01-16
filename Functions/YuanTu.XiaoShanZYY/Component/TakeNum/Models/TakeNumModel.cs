using YuanTu.Consts.FrameworkBase;
using YuanTu.XiaoShanZYY.HIS;

namespace YuanTu.XiaoShanZYY.Component.TakeNum.Models
{
    public interface ITakeNumModel : IModel
    {
        string 取号密码 { get; set; }
        Res挂号取号 Res挂号预结算 { get; set; }
        Res挂号取号 Res挂号结算 { get; set; }
        Res预约退号处理 Res预约退号处理 { get; set; }
    }

    public class TakeNumModel : ModelBase, ITakeNumModel
    {
        public string 取号密码 { get; set; }
        public Res挂号取号 Res挂号预结算 { get; set; }
        public Res挂号取号 Res挂号结算 { get; set; }
        public Res预约退号处理 Res预约退号处理 { get; set; }
    }
}