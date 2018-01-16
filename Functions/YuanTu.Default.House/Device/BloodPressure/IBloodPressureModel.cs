using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.BloodPressure
{
    public interface IBloodPressureModel:IModel
    {
       decimal 舒张压 { get; set; }
       decimal 收缩压 { get; set; }
       decimal 脉搏 { get; set; }
       string 参考结果 { get; set; }
    }
    public class BloodPressureModel:ModelBase,IBloodPressureModel
    {
        public decimal 舒张压 { get; set; }
        public decimal 收缩压 { get; set; }
        public decimal 脉搏 { get; set; }
        public string 参考结果 { get; set; }
    }
}
