using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.Temperature
{
    public interface ITemperatureModel : IModel
    {
        decimal 表面温度 { get; set; }
        decimal 人体温度 { get; set; }
        decimal 环境温度 { get; set; }
        string 参考结果 { get; set; }

    }
    public class TemperatureModel : ModelBase, ITemperatureModel
    {
        public decimal 表面温度 { get; set; }
        public decimal 人体温度 { get; set; }
        public decimal 环境温度 { get; set; }
        public string 参考结果 { get; set; }
    }
}
