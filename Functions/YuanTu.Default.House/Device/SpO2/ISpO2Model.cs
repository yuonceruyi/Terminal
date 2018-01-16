using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.SpO2
{
    public interface ISpO2Model:IModel
    {
        decimal PI { get; set; }
        decimal PR { get; set; }
        decimal SpO2 { get; set; }
        string 参考结果 { get; set; }
    }
    public class SpO2Model : ModelBase, ISpO2Model
    {
        public decimal SpO2 { get; set; }
        public string 参考结果 { get; set; }

        public decimal PR { get; set; }

        public decimal PI { get; set; }
    }

}