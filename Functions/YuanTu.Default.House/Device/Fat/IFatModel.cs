using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.Fat
{
    public interface IFatModel : IModel
    {
        decimal 脂肪含量 { get; set; }
        decimal 体质指数 { get; set; }
        decimal 基础代谢值 { get; set; }
        string 体质参考结果 { get; set; }
        string 体型参考结果 { get; set; }
    }
    public class FatModel:ModelBase,IFatModel
    {
        public decimal 脂肪含量 { get; set; }
        public decimal 体质指数 { get; set; }
        public decimal 基础代谢值 { get; set; }
        public string 体质参考结果 { get; set; }
        public string 体型参考结果 { get; set; }
    }
}
