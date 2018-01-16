using YuanTu.Consts.FrameworkBase;

namespace YuanTu.Default.House.Device.HeightWeight
{
    public interface IHeightWeightModel : IModel
    {
        decimal 身高 { get; set; }
        decimal 体重 { get; set; }
        decimal 体质指数 { get; set; }
        string 参考结果 { get; set; }
    }

    public class HeightWeightModel : ModelBase, IHeightWeightModel
    {
        public decimal 身高 { get; set; }
        public decimal 体重 { get; set; }
        public decimal 体质指数 { get; set; }
        public string 参考结果 { get; set; }
    }
}